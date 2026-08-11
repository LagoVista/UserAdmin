using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Repos.Testing;
using LagoVista.UserAdmin.Models.Testing;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Testing
{
    internal class AuthViewRepo : IAuthViewRepo
    {
        private const string GitArchiveUrl = "https://github.com/LagoVista/UserAdmin/archive/refs/heads/master.zip";
        private const string AuthViewPathMarker = "/auth-model/auth-views/";
        private const string AuthRoutePathMarker = "/auth-model/auth-routes/";

        private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };

        private readonly IAdminLogger _adminLogger;
        private readonly SemaphoreSlim _gitLoadLock = new SemaphoreSlim(1, 1);
        private List<AuthView> _gitAuthViewCache;

        public AuthViewRepo(IUserAdminSettings userAdminSettings, IAdminLogger adminLogger)
        {
            _ = userAdminSettings ?? throw new ArgumentNullException(nameof(userAdminSettings));
            _adminLogger = adminLogger ?? throw new ArgumentNullException(nameof(adminLogger));
        }

        public Task AddAuthViewAsync(AuthView dsl) => ReadOnlyAsync();

        public Task DeleteByIdAsync(string id) => ReadOnlyAsync();

        public async Task<AuthView> GetByIdAsync(string id)
        {
            if (String.IsNullOrWhiteSpace(id)) return null;

            var views = await LoadAuthViewsAsync(false);
            var compatibleKey = ToLagoVistaKey(id);
            return views.FirstOrDefault(view => String.Equals(view.Id, id, StringComparison.OrdinalIgnoreCase) || String.Equals(view.ViewId, id, StringComparison.OrdinalIgnoreCase) || String.Equals(view.Key, compatibleKey, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<ListResponse<AuthViewSummary>> ListAsync(string orgId, ListRequest request)
        {
            _ = orgId;
            request ??= ListRequest.CreateForAll();

            var views = await LoadAuthViewsAsync(true);
            var summaries = views.Select(view => view.CreateSummary()).OrderBy(view => view.Name).ToList();
            var page = summaries.Skip(request.PageSize * (request.PageIndex - 1)).Take(request.PageSize);
            return ListResponse<AuthViewSummary>.Create(request, page);
        }

        public Task UpdateAuthViewAsync(AuthView dsl) => ReadOnlyAsync();

        private async Task<List<AuthView>> LoadAuthViewsAsync(bool forceGitRefresh)
        {
            if (!forceGitRefresh && _gitAuthViewCache != null) return _gitAuthViewCache;

            await _gitLoadLock.WaitAsync();
            try
            {
                if (!forceGitRefresh && _gitAuthViewCache != null) return _gitAuthViewCache;

                _adminLogger.Trace($"[AuthViewRepo__LoadAuthViews] Loading authoritative canonical AuthViews from '{GitArchiveUrl}'.");
                try
                {
                    _gitAuthViewCache = await LoadAuthViewsFromGitAsync();
                    _adminLogger.Trace($"[AuthViewRepo__LoadAuthViews] SUCCESS. Loaded {_gitAuthViewCache.Count} canonical AuthViews from Git master.");
                    return _gitAuthViewCache;
                }
                catch (Exception ex)
                {
                    _adminLogger.Trace($"[AuthViewRepo__LoadAuthViews] AUTHORITATIVE GIT LOAD FAILED. Exception='{ex}'.");
                    throw;
                }
            }
            finally
            {
                _gitLoadLock.Release();
            }
        }

        private async Task<List<AuthView>> LoadAuthViewsFromGitAsync()
        {
            using var response = await _httpClient.GetAsync(GitArchiveUrl);
            _adminLogger.Trace($"[AuthViewRepo__LoadFromGit] Git response StatusCode={(int)response.StatusCode} ({response.StatusCode}), ContentLength={response.Content.Headers.ContentLength?.ToString() ?? "unknown"}.");
            response.EnsureSuccessStatusCode();

            var archiveBytes = await response.Content.ReadAsByteArrayAsync();
            using var stream = new MemoryStream(archiveBytes, false);
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read, false);

            var routeEntries = archive.Entries.Where(IsAuthRouteEntry).ToList();
            var viewEntries = archive.Entries.Where(IsAuthViewEntry).ToList();
            _adminLogger.Trace($"[AuthViewRepo__LoadFromGit] ZIP opened. AuthRouteEntries={routeEntries.Count}, AuthViewEntries={viewEntries.Count}.");

            var routeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in routeEntries)
            {
                var json = ReadJson(entry);
                var routeId = json.Value<string>("routeId");
                var path = json.Value<string>("path");
                if (!String.IsNullOrWhiteSpace(routeId) && !String.IsNullOrWhiteSpace(path)) routeMap[routeId] = path;
            }

            var views = new List<AuthView>();
            foreach (var entry in viewEntries)
            {
                try
                {
                    views.Add(HydrateAuthView(ReadJson(entry), entry.FullName, routeMap));
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException($"Failed hydrating canonical AuthView '{entry.FullName}'.", ex);
                }
            }

            return views.OrderBy(view => view.Name).ToList();
        }

        private static AuthView HydrateAuthView(JObject json, string source, IReadOnlyDictionary<string, string> routeMap)
        {
            var viewId = RequiredString(json, "viewId", source);
            var runtimeEntityId = json["source"]?.Value<string>("runtimeEntityId");
            var routeId = RequiredString(json, "routeId", source);
            if (!routeMap.TryGetValue(routeId, out var route)) throw new InvalidDataException($"Canonical AuthView '{viewId}' references missing route '{routeId}'.");

            return new AuthView
            {
                Id = String.IsNullOrWhiteSpace(runtimeEntityId) ? ToRuntimeEntityId(viewId) : runtimeEntityId,
                Key = ToLagoVistaKey(viewId),
                Name = RequiredString(json, "name", source),
                Description = json.Value<string>("description"),
                ViewId = viewId,
                Route = route,
                Actions = HydrateActions(viewId, json["actions"] as JArray),
                Fields = HydrateFields(viewId, json["controls"] as JArray)
            };
        }

        private static List<AuthFieldAction> HydrateActions(string viewId, JArray actions)
        {
            if (actions == null) return new List<AuthFieldAction>();

            return actions.OfType<JObject>().Select(action =>
            {
                var actionId = action.Value<string>("id");
                return new AuthFieldAction
                {
                    Id = ToRuntimeEntityId($"{viewId}:action:{actionId}"),
                    Name = action.Value<string>("name") ?? actionId,
                    Finder = ToTestIdFinder(action.Value<string>("finder"))
                };
            }).ToList();
        }

        private static List<AuthViewField> HydrateFields(string viewId, JArray controls)
        {
            if (controls == null) return new List<AuthViewField>();

            return controls.OfType<JObject>().Select(control =>
            {
                var controlId = control.Value<string>("id");
                return new AuthViewField
                {
                    Id = ToRuntimeEntityId($"{viewId}:control:{controlId}"),
                    Name = control.Value<string>("name") ?? controlId,
                    FieldType = control.Value<string>("controlType") ?? "unknown",
                    Finder = ToTestIdFinder(control.Value<string>("finder"))
                };
            }).ToList();
        }

        private static bool IsAuthViewEntry(ZipArchiveEntry entry)
        {
            var markerIndex = entry.FullName.IndexOf(AuthViewPathMarker, StringComparison.OrdinalIgnoreCase);
            if (markerIndex < 0 || !entry.FullName.EndsWith(".json", StringComparison.OrdinalIgnoreCase)) return false;
            var relativePath = entry.FullName.Substring(markerIndex + AuthViewPathMarker.Length);
            return !relativePath.Contains("/");
        }

        private static bool IsAuthRouteEntry(ZipArchiveEntry entry)
        {
            var markerIndex = entry.FullName.IndexOf(AuthRoutePathMarker, StringComparison.OrdinalIgnoreCase);
            if (markerIndex < 0 || !entry.FullName.EndsWith(".json", StringComparison.OrdinalIgnoreCase)) return false;
            var relativePath = entry.FullName.Substring(markerIndex + AuthRoutePathMarker.Length);
            return !relativePath.Contains("/");
        }

        private static JObject ReadJson(ZipArchiveEntry entry)
        {
            using var reader = new StreamReader(entry.Open());
            return JObject.Parse(reader.ReadToEnd());
        }

        private static string ToRuntimeEntityId(string canonicalValue)
        {
            if (String.IsNullOrWhiteSpace(canonicalValue)) throw new ArgumentNullException(nameof(canonicalValue));

            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(canonicalValue));
            var builder = new StringBuilder(32);
            for (var index = 0; index < 16; index++) builder.Append(hash[index].ToString("X2"));
            return builder.ToString();
        }

        private static string ToLagoVistaKey(string canonicalValue)
        {
            if (String.IsNullOrWhiteSpace(canonicalValue)) return canonicalValue;
            return canonicalValue.Trim().ToLowerInvariant().Replace('.', '-').Replace(':', '-');
        }

        private static string ToTestIdFinder(string finder)
        {
            if (String.IsNullOrWhiteSpace(finder) || finder.StartsWith("[", StringComparison.Ordinal)) return finder;
            return $"[data-testid=\"{finder}\"]";
        }

        private static string RequiredString(JObject json, string propertyName, string source)
        {
            var value = json.Value<string>(propertyName);
            if (String.IsNullOrWhiteSpace(value)) throw new InvalidDataException($"Canonical AuthView '{source}' is missing required property '{propertyName}'.");
            return value;
        }

        private static Task ReadOnlyAsync() => Task.FromException(new NotSupportedException("Canonical AuthViews are read-only and must be changed in auth-model JSON."));
    }
}
