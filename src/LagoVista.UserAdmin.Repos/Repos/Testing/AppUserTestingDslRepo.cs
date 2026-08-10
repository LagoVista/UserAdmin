using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Models.Testing;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Testing
{
    public class AppUserTestingDslRepo : IAppUserTestingDslRepo
    {
        private const string GitArchiveUrl = "https://github.com/LagoVista/UserAdmin/archive/refs/heads/master.zip";
        private const string ScenarioPathMarker = "/auth-model/scenarios-v2/";
        private const string AuthViewPathMarker = "/auth-model/auth-views/";

        private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };

        private readonly IAdminLogger _adminLogger;
        private readonly string _authModelRoot;
        private readonly SemaphoreSlim _gitLoadLock = new SemaphoreSlim(1, 1);
        private List<AppUserTestScenario> _gitScenarioCache;

        public AppUserTestingDslRepo(IUserAdminSettings userAdminSettings, IAdminLogger adminLogger)
        {
            _ = userAdminSettings ?? throw new ArgumentNullException(nameof(userAdminSettings));
            _adminLogger = adminLogger ?? throw new ArgumentNullException(nameof(adminLogger));
            _authModelRoot = Path.Combine(AppContext.BaseDirectory, "auth-model");

            _adminLogger.Trace($"[AppUserTestingDslRepo__Ctor] BaseDirectory='{AppContext.BaseDirectory}', AuthModelRoot='{_authModelRoot}', GitArchiveUrl='{GitArchiveUrl}'.");
        }

        public Task AddDSLAsync(AppUserTestScenario dsl) => ReadOnlyAsync();

        public Task DeleteByIdAsync(string id) => ReadOnlyAsync();

        public Task UpdateTestScenarioAsync(AppUserTestScenario dsl) => ReadOnlyAsync();

        public async Task<AppUserTestScenario> GetByIdAsync(string id)
        {
            if (String.IsNullOrWhiteSpace(id))
                return null;

            _adminLogger.Trace($"[AppUserTestingDslRepo__GetById] Loading canonical scenario '{id}'.");
            var scenarios = await LoadScenariosAsync(false);
            var scenario = scenarios.FirstOrDefault(item => String.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase) || String.Equals(item.Key, id, StringComparison.OrdinalIgnoreCase));
            _adminLogger.Trace($"[AppUserTestingDslRepo__GetById] Scenario '{id}' {(scenario == null ? "was not found" : $"resolved to key '{scenario.Key}' and runtime id '{scenario.Id}'")}.");
            return scenario;
        }

        public async Task<ListResponse<AppUserTestScenarioSummary>> ListAsync(string orgId, ListRequest request)
        {
            _ = orgId;
            request ??= ListRequest.CreateForAll();

            _adminLogger.Trace($"[AppUserTestingDslRepo__List] Refresh requested. PageIndex={request.PageIndex}, PageSize={request.PageSize}.");

            // Refresh is the explicit signal to pull the latest canonical definitions from Git.
            var scenarios = await LoadScenariosAsync(true);
            var summaries = scenarios.Select(item => item.CreateSummary()).OrderBy(item => item.Name).ToList();
            var page = summaries.Skip(request.PageSize * (request.PageIndex - 1)).Take(request.PageSize);
            _adminLogger.Trace($"[AppUserTestingDslRepo__List] Returning {summaries.Count} hydrated canonical scenario summaries before paging.");
            return ListResponse<AppUserTestScenarioSummary>.Create(request, page);
        }

        private async Task<List<AppUserTestScenario>> LoadScenariosAsync(bool forceGitRefresh)
        {
            _adminLogger.Trace($"[AppUserTestingDslRepo__LoadScenarios] Begin. ForceGitRefresh={forceGitRefresh}, CacheAvailable={_gitScenarioCache != null}.");

            if (!forceGitRefresh && _gitScenarioCache != null)
            {
                _adminLogger.Trace($"[AppUserTestingDslRepo__LoadScenarios] Returning cached canonical scenarios. Count={_gitScenarioCache.Count}.");
                return _gitScenarioCache;
            }

            await _gitLoadLock.WaitAsync();
            try
            {
                if (!forceGitRefresh && _gitScenarioCache != null)
                {
                    _adminLogger.Trace($"[AppUserTestingDslRepo__LoadScenarios] Cache populated while waiting for lock. Count={_gitScenarioCache.Count}.");
                    return _gitScenarioCache;
                }

                try
                {
                    _adminLogger.Trace($"[AppUserTestingDslRepo__LoadScenarios] Attempting Git load from '{GitArchiveUrl}'.");
                    _gitScenarioCache = await LoadScenariosFromGitAsync();
                    _adminLogger.Trace($"[AppUserTestingDslRepo__LoadScenarios] SUCCESS. Loaded {_gitScenarioCache.Count} canonical auth scenarios from Git master.");
                    return _gitScenarioCache;
                }
                catch (Exception ex)
                {
                    _adminLogger.Trace($"[AppUserTestingDslRepo__LoadScenarios] GIT LOAD FAILED. ExceptionType='{ex.GetType().FullName}', Message='{ex.Message}', Stack='{ex.StackTrace}'. Falling back to published JSON.");

                    try
                    {
                        return LoadScenariosFromPublishedJson();
                    }
                    catch (Exception fallbackEx)
                    {
                        _adminLogger.Trace($"[AppUserTestingDslRepo__LoadScenarios] PUBLISHED JSON FALLBACK FAILED. ExceptionType='{fallbackEx.GetType().FullName}', Message='{fallbackEx.Message}', Stack='{fallbackEx.StackTrace}', BaseDirectory='{AppContext.BaseDirectory}', AuthModelRoot='{_authModelRoot}'.");
                        throw;
                    }
                }
            }
            finally
            {
                _gitLoadLock.Release();
            }
        }

        private async Task<List<AppUserTestScenario>> LoadScenariosFromGitAsync()
        {
            _adminLogger.Trace($"[AppUserTestingDslRepo__LoadFromGit] Sending GET '{GitArchiveUrl}'. TimeoutSeconds={_httpClient.Timeout.TotalSeconds}.");

            using var response = await _httpClient.GetAsync(GitArchiveUrl);
            _adminLogger.Trace($"[AppUserTestingDslRepo__LoadFromGit] Git response StatusCode={(int)response.StatusCode} ({response.StatusCode}), ContentType='{response.Content.Headers.ContentType}', ContentLength={response.Content.Headers.ContentLength?.ToString() ?? "unknown"}.");
            response.EnsureSuccessStatusCode();

            var archiveBytes = await response.Content.ReadAsByteArrayAsync();
            _adminLogger.Trace($"[AppUserTestingDslRepo__LoadFromGit] Downloaded Git archive. Bytes={archiveBytes.Length}.");

            using var stream = new MemoryStream(archiveBytes, false);
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read, false);

            var authViewEntries = archive.Entries.Where(IsAuthViewEntry).ToList();
            var scenarioEntries = archive.Entries.Where(IsScenarioEntry).ToList();
            _adminLogger.Trace($"[AppUserTestingDslRepo__LoadFromGit] ZIP opened. TotalEntries={archive.Entries.Count}, AuthViewEntries={authViewEntries.Count}, ScenarioEntries={scenarioEntries.Count}.");

            var authViewJson = new List<JObject>();
            foreach (var entry in authViewEntries)
            {
                try
                {
                    _adminLogger.Trace($"[AppUserTestingDslRepo__LoadFromGit] Reading AuthView '{entry.FullName}'.");
                    authViewJson.Add(ReadJson(entry));
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException($"Failed reading canonical AuthView JSON '{entry.FullName}'.", ex);
                }
            }

            var viewMap = LoadAuthViewMap(authViewJson);
            _adminLogger.Trace($"[AppUserTestingDslRepo__LoadFromGit] Built AuthView map. Count={viewMap.Count}.");

            var scenarios = new List<AppUserTestScenario>();
            foreach (var entry in scenarioEntries)
            {
                try
                {
                    _adminLogger.Trace($"[AppUserTestingDslRepo__LoadFromGit] Reading scenario '{entry.FullName}'.");
                    var json = ReadJson(entry);
                    var key = json.Value<string>("key") ?? "<missing>";
                    var runtimeEntityId = json.Value<string>("runtimeEntityId") ?? "<missing>";
                    _adminLogger.Trace($"[AppUserTestingDslRepo__LoadFromGit] Hydrating scenario Source='{entry.FullName}', Key='{key}', RuntimeEntityId='{runtimeEntityId}'.");
                    scenarios.Add(HydrateScenario(json, entry.FullName, viewMap));
                    _adminLogger.Trace($"[AppUserTestingDslRepo__LoadFromGit] Hydrated scenario Key='{key}', RuntimeEntityId='{runtimeEntityId}'.");
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException($"Failed hydrating canonical auth scenario '{entry.FullName}'.", ex);
                }
            }

            var ordered = scenarios.OrderBy(item => item.Name).ToList();
            _adminLogger.Trace($"[AppUserTestingDslRepo__LoadFromGit] Completed Git hydration successfully. ScenarioCount={ordered.Count}.");
            return ordered;
        }

        private List<AppUserTestScenario> LoadScenariosFromPublishedJson()
        {
            var scenarioRoot = Path.Combine(_authModelRoot, "scenarios-v2");
            _adminLogger.Trace($"[AppUserTestingDslRepo__LoadPublished] Attempting published fallback. ScenarioRoot='{scenarioRoot}', Exists={Directory.Exists(scenarioRoot)}, AuthModelRoot='{_authModelRoot}', BaseDirectory='{AppContext.BaseDirectory}'.");

            if (!Directory.Exists(scenarioRoot))
            {
                var authModelRootExists = Directory.Exists(_authModelRoot);
                var visibleEntries = authModelRootExists ? String.Join(", ", Directory.GetFileSystemEntries(_authModelRoot).Select(Path.GetFileName).Take(25)) : "<auth-model-root-missing>";
                _adminLogger.Trace($"[AppUserTestingDslRepo__LoadPublished] Published scenario directory missing. AuthModelRootExists={authModelRootExists}, VisibleEntries='{visibleEntries}'.");
                throw new DirectoryNotFoundException($"Published auth scenario directory was not found: '{scenarioRoot}'.");
            }

            var viewMap = LoadPublishedAuthViewMap();
            var scenarioFiles = Directory.GetFiles(scenarioRoot, "*.json", SearchOption.AllDirectories);
            _adminLogger.Trace($"[AppUserTestingDslRepo__LoadPublished] Found {scenarioFiles.Length} published scenario JSON files.");

            var scenarios = new List<AppUserTestScenario>();
            foreach (var path in scenarioFiles)
            {
                try
                {
                    _adminLogger.Trace($"[AppUserTestingDslRepo__LoadPublished] Hydrating published scenario '{path}'.");
                    scenarios.Add(HydrateScenario(JObject.Parse(System.IO.File.ReadAllText(path)), path, viewMap));
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException($"Failed hydrating published auth scenario '{path}'.", ex);
                }
            }

            scenarios = scenarios.OrderBy(item => item.Name).ToList();
            _adminLogger.Trace($"[AppUserTestingDslRepo__LoadPublished] Loaded {scenarios.Count} canonical auth scenarios from published JSON fallback.");
            return scenarios;
        }

        private Dictionary<string, EntityHeader> LoadPublishedAuthViewMap()
        {
            var viewRoot = Path.Combine(_authModelRoot, "auth-views");
            _adminLogger.Trace($"[AppUserTestingDslRepo__LoadPublishedAuthViews] ViewRoot='{viewRoot}', Exists={Directory.Exists(viewRoot)}.");
            if (!Directory.Exists(viewRoot))
                throw new DirectoryNotFoundException($"Published auth view directory was not found: '{viewRoot}'.");

            var files = Directory.GetFiles(viewRoot, "*.json", SearchOption.TopDirectoryOnly);
            _adminLogger.Trace($"[AppUserTestingDslRepo__LoadPublishedAuthViews] Found {files.Length} published AuthView JSON files.");
            return LoadAuthViewMap(files.Select(path => JObject.Parse(System.IO.File.ReadAllText(path))));
        }

        private static Dictionary<string, EntityHeader> LoadAuthViewMap(IEnumerable<JObject> authViews)
        {
            var result = new Dictionary<string, EntityHeader>(StringComparer.OrdinalIgnoreCase);
            foreach (var json in authViews)
            {
                var viewId = json.Value<string>("viewId");
                if (String.IsNullOrWhiteSpace(viewId))
                    continue;

                var runtimeEntityId = json["source"]?.Value<string>("runtimeEntityId");
                var name = json.Value<string>("name") ?? viewId;
                result[viewId] = EntityHeader.Create(String.IsNullOrWhiteSpace(runtimeEntityId) ? viewId : runtimeEntityId, name);
            }

            return result;
        }

        private static bool IsScenarioEntry(ZipArchiveEntry entry) => entry.FullName.Contains(ScenarioPathMarker, StringComparison.OrdinalIgnoreCase) && entry.FullName.EndsWith(".json", StringComparison.OrdinalIgnoreCase);

        private static bool IsAuthViewEntry(ZipArchiveEntry entry)
        {
            var markerIndex = entry.FullName.IndexOf(AuthViewPathMarker, StringComparison.OrdinalIgnoreCase);
            if (markerIndex < 0 || !entry.FullName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                return false;

            var relativePath = entry.FullName.Substring(markerIndex + AuthViewPathMarker.Length);
            return !relativePath.Contains("/");
        }

        private static JObject ReadJson(ZipArchiveEntry entry)
        {
            using var reader = new StreamReader(entry.Open());
            return JObject.Parse(reader.ReadToEnd());
        }

        private static AppUserTestScenario HydrateScenario(JObject json, string source, IReadOnlyDictionary<string, EntityHeader> viewMap)
        {
            var key = RequiredString(json, "key", source);
            var runtimeEntityId = RequiredString(json, "runtimeEntityId", source);
            var action = json["action"] as JObject;
            var actionId = action?.Value<string>("id");
            var actionFinder = action?.Value<string>("finder");

            var scenario = new AppUserTestScenario
            {
                Id = runtimeEntityId,
                Key = key,
                Name = RequiredString(json, "name", source),
                Description = json.Value<string>("summary"),
                AuthView = ResolveView(json.Value<string>("startViewKey"), viewMap),
                ExpectedView = ResolveView(json.Value<string>("expectedViewKey"), viewMap),
                Action = EntityHeader.Create(actionId, actionFinder ?? actionId),
                Inputs = HydrateInputs(json["inputs"] as JArray),
                PreConditions = HydrateState(json["preconditions"]?["state"] as JObject),
                PostConditions = HydrateState(json["postconditions"]?["state"] as JObject),
                ExpectedAuthLogEvents = json["expectedAuthLogEvents"]?.Values<string>().Where(value => !String.IsNullOrWhiteSpace(value)).ToList() ?? new List<string>()
            };

            return scenario;
        }

        private static List<AppUserTestSettingsValue> HydrateInputs(JArray inputs)
        {
            if (inputs == null)
                return new List<AppUserTestSettingsValue>();

            return inputs.OfType<JObject>().Select(input => new AppUserTestSettingsValue
            {
                Finder = ToTestIdFinder(input.Value<string>("finder")),
                Name = input.Value<string>("name"),
                Value = input.Value<string>("value")
            }).ToList();
        }

        private static AuthTenantStateSnapshot HydrateState(JObject state)
        {
            var snapshot = new AuthTenantStateSnapshot();
            if (state == null)
                return snapshot;

            foreach (var property in typeof(AuthTenantStateSnapshot).GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(property => property.CanWrite))
            {
                var token = state.Properties().FirstOrDefault(item => String.Equals(item.Name, property.Name, StringComparison.OrdinalIgnoreCase))?.Value;
                if (token == null || token.Type == JTokenType.Null)
                    continue;

                if (property.PropertyType == typeof(EntityHeader<SetCondition>))
                {
                    if (Enum.TryParse<SetCondition>(token.Value<string>(), true, out var condition))
                        property.SetValue(snapshot, EntityHeader<SetCondition>.Create(condition));
                }
                else if (property.PropertyType == typeof(int?))
                {
                    property.SetValue(snapshot, token.Value<int?>());
                }
                else if (property.PropertyType == typeof(string))
                {
                    property.SetValue(snapshot, token.Value<string>());
                }
                else if (property.PropertyType == typeof(List<string>) && token is JArray values)
                {
                    property.SetValue(snapshot, values.Values<string>().ToList());
                }
                else if (property.PropertyType == typeof(AuthOneTimeCodeState) && token is JObject code)
                {
                    var codeState = new AuthOneTimeCodeState { AttemptCount = code.Value<int?>("attemptCount") };
                    if (Enum.TryParse<AuthOneTimeCodeStatus>(code.Value<string>("status"), true, out var status))
                        codeState.Status = status;
                    property.SetValue(snapshot, codeState);
                }
            }

            return snapshot;
        }

        private static EntityHeader ResolveView(string viewKey, IReadOnlyDictionary<string, EntityHeader> viewMap)
        {
            if (String.IsNullOrWhiteSpace(viewKey))
                return null;

            if (viewMap.TryGetValue(viewKey, out var view))
                return EntityHeader.Create(view.Id, view.Text);

            return EntityHeader.Create(viewKey, viewKey);
        }

        private static string ToTestIdFinder(string finder)
        {
            if (String.IsNullOrWhiteSpace(finder) || finder.StartsWith("[", StringComparison.Ordinal))
                return finder;

            return $"[data-testid=\"{finder}\"]";
        }

        private static string RequiredString(JObject json, string propertyName, string source)
        {
            var value = json.Value<string>(propertyName);
            if (String.IsNullOrWhiteSpace(value))
                throw new InvalidDataException($"Canonical auth scenario '{source}' is missing required property '{propertyName}'.");
            return value;
        }

        private static Task ReadOnlyAsync() => Task.FromException(new NotSupportedException("Canonical authentication scenarios are read-only and must be changed in auth-model JSON."));
    }
}
