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
using System.Security.Cryptography;
using System.Text;
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
        private readonly SemaphoreSlim _gitLoadLock = new SemaphoreSlim(1, 1);
        private List<AppUserTestScenario> _gitScenarioCache;

        public AppUserTestingDslRepo(IUserAdminSettings userAdminSettings, IAdminLogger adminLogger)
        {
            _ = userAdminSettings ?? throw new ArgumentNullException(nameof(userAdminSettings));
            _adminLogger = adminLogger ?? throw new ArgumentNullException(nameof(adminLogger));
            _adminLogger.Trace($"[AppUserTestingDslRepo__Ctor] BaseDirectory='{AppContext.BaseDirectory}', GitArchiveUrl='{GitArchiveUrl}'. Git is the authoritative auth scenario source.");
        }

        public Task AddDSLAsync(AppUserTestScenario dsl) => ReadOnlyAsync();
        public Task DeleteByIdAsync(string id) => ReadOnlyAsync();
        public Task UpdateTestScenarioAsync(AppUserTestScenario dsl) => ReadOnlyAsync();

        public async Task<AppUserTestScenario> GetByIdAsync(string id)
        {
            if (String.IsNullOrWhiteSpace(id)) return null;

            _adminLogger.Trace($"[AppUserTestingDslRepo__GetById] Loading canonical scenario '{id}'.");
            var scenarios = await LoadScenariosAsync(false);
            var compatibleKey = ToLagoVistaKey(id);
            var scenario = scenarios.FirstOrDefault(item =>
                String.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase) ||
                String.Equals(item.CanonicalKey, id, StringComparison.OrdinalIgnoreCase) ||
                String.Equals(item.Key, compatibleKey, StringComparison.OrdinalIgnoreCase));
            _adminLogger.Trace($"[AppUserTestingDslRepo__GetById] Scenario '{id}' {(scenario == null ? "was not found" : $"resolved to canonical key '{scenario.CanonicalKey}', runtime key '{scenario.Key}' and runtime id '{scenario.Id}'")}.");
            return scenario;
        }

        public async Task<ListResponse<AppUserTestScenarioSummary>> ListAsync(string orgId, ListRequest request)
        {
            _ = orgId;
            request ??= ListRequest.CreateForAll();

            _adminLogger.Trace($"[AppUserTestingDslRepo__List] Refresh requested. PageIndex={request.PageIndex}, PageSize={request.PageSize}.");
            var scenarios = await LoadScenariosAsync(true);
            var summaries = scenarios.Select(item => item.CreateSummary()).OrderBy(item => item.Name).ToList();
            var page = summaries.Skip(request.PageSize * (request.PageIndex - 1)).Take(request.PageSize);
            _adminLogger.Trace($"[AppUserTestingDslRepo__List] Returning {summaries.Count} hydrated canonical scenario summaries before paging.");
            return ListResponse<AppUserTestScenarioSummary>.Create(request, page);
        }

        private async Task<List<AppUserTestScenario>> LoadScenariosAsync(bool forceGitRefresh)
        {
            _adminLogger.Trace($"[AppUserTestingDslRepo__LoadScenarios] Begin. ForceGitRefresh={forceGitRefresh}, CacheAvailable={_gitScenarioCache != null}.");

            if (!forceGitRefresh && _gitScenarioCache != null) return _gitScenarioCache;

            await _gitLoadLock.WaitAsync();
            try
            {
                if (!forceGitRefresh && _gitScenarioCache != null) return _gitScenarioCache;

                _adminLogger.Trace($"[AppUserTestingDslRepo__LoadScenarios] Attempting authoritative Git load from '{GitArchiveUrl}'.");
                try
                {
                    _gitScenarioCache = await LoadScenariosFromGitAsync();
                    _adminLogger.Trace($"[AppUserTestingDslRepo__LoadScenarios] SUCCESS. Loaded {_gitScenarioCache.Count} canonical auth scenarios from Git master.");
                    return _gitScenarioCache;
                }
                catch (Exception ex)
                {
                    _adminLogger.Trace($"[AppUserTestingDslRepo__LoadScenarios] AUTHORITATIVE GIT LOAD FAILED. Exception='{ex}'. No published JSON fallback will be attempted.");
                    throw;
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
                    authViewJson.Add(ReadJson(entry));
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException($"Failed reading canonical AuthView JSON '{entry.FullName}'.", ex);
                }
            }

            var viewMap = LoadAuthViewMap(authViewJson);
            var scenarios = new List<AppUserTestScenario>();
            foreach (var entry in scenarioEntries)
            {
                try
                {
                    var json = ReadJson(entry);
                    scenarios.Add(HydrateScenario(json, entry.FullName, viewMap));
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException($"Failed hydrating canonical auth scenario '{entry.FullName}'.", ex);
                }
            }

            return scenarios.OrderBy(item => item.Name).ToList();
        }

        private static Dictionary<string, EntityHeader> LoadAuthViewMap(IEnumerable<JObject> authViews)
        {
            var result = new Dictionary<string, EntityHeader>(StringComparer.OrdinalIgnoreCase);
            foreach (var json in authViews)
            {
                var viewId = json.Value<string>("viewId");
                if (String.IsNullOrWhiteSpace(viewId)) continue;

                var runtimeEntityId = json["source"]?.Value<string>("runtimeEntityId");
                var name = json.Value<string>("name") ?? viewId;
                result[viewId] = EntityHeader.Create(String.IsNullOrWhiteSpace(runtimeEntityId) ? ToRuntimeEntityId(viewId) : runtimeEntityId, name);
            }

            return result;
        }

        private static bool IsScenarioEntry(ZipArchiveEntry entry) => entry.FullName.Contains(ScenarioPathMarker, StringComparison.OrdinalIgnoreCase) && entry.FullName.EndsWith(".json", StringComparison.OrdinalIgnoreCase);

        private static bool IsAuthViewEntry(ZipArchiveEntry entry)
        {
            var markerIndex = entry.FullName.IndexOf(AuthViewPathMarker, StringComparison.OrdinalIgnoreCase);
            if (markerIndex < 0 || !entry.FullName.EndsWith(".json", StringComparison.OrdinalIgnoreCase)) return false;
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
            var actionId = RequiredString(action, "id", source, "action");
            var actionFinder = RequiredString(action, "finder", source, "action");
            var serverInteraction = json["serverInteraction"] as JObject;

            return new AppUserTestScenario
            {
                Id = runtimeEntityId,
                Key = ToLagoVistaKey(key),
                CanonicalKey = key,
                SchemaVersion = RequiredString(json, "schemaVersion", source),
                DefinitionVersion = json.Value<int?>("version") ?? 0,
                Maturity = RequiredString(json, "maturity", source),
                CategoryKey = RequiredString(json, "categoryKey", source),
                DefinitionHash = json.Value<string>("definitionHash"),
                Name = RequiredString(json, "name", source),
                Description = RequiredString(json, "summary", source),
                AuthView = ResolveView(RequiredString(json, "startViewKey", source), viewMap),
                ExpectedView = ResolveView(RequiredString(json, "expectedViewKey", source), viewMap),
                ActionId = actionId,
                ActionFinder = actionFinder,
                Action = EntityHeader.Create(ToRuntimeEntityId($"{key}:action:{actionId}"), actionFinder),
                Inputs = HydrateInputs(json["inputs"] as JArray),
                PreconditionExpression = json["preconditions"]?.Value<string>("expression"),
                PostconditionExpression = json["postconditions"]?.Value<string>("expression"),
                PreConditions = HydrateState(json["preconditions"]?["state"] as JObject),
                PostConditions = HydrateState(json["postconditions"]?["state"] as JObject),
                ServerInteractionRequired = serverInteraction?.Value<bool?>("required") ?? false,
                ServerInteractionIntent = serverInteraction?.Value<string>("intent"),
                TransitionKeys = ReadStringList(serverInteraction?["transitionKeys"] as JArray),
                ExpectedVisibleFinders = ReadStringList(json["expectedVisibleFinders"] as JArray),
                ExpectedAuthLogEvents = ReadStringList(json["expectedAuthLogEvents"] as JArray),
                EvidenceRequirements = ReadStringList(json["evidenceRequirements"] as JArray)
            };
        }

        private static List<AppUserTestSettingsValue> HydrateInputs(JArray inputs)
        {
            if (inputs == null) return new List<AppUserTestSettingsValue>();

            return inputs.OfType<JObject>().Select(input => new AppUserTestSettingsValue
            {
                Finder = ToTestIdFinder(input.Value<string>("finder")),
                Name = input.Value<string>("name"),
                ValueType = input.Value<string>("valueType"),
                Required = input.Value<bool?>("required") ?? false,
                Value = input.Value<string>("value"),
                Description = input.Value<string>("description")
            }).ToList();
        }

        private static List<string> ReadStringList(JArray values)
        {
            return values?.Values<string>().Where(value => !String.IsNullOrWhiteSpace(value)).ToList() ?? new List<string>();
        }

        private static AuthTenantStateSnapshot HydrateState(JObject state)
        {
            var snapshot = new AuthTenantStateSnapshot();
            if (state == null) return snapshot;

            foreach (var property in typeof(AuthTenantStateSnapshot).GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(property => property.CanWrite))
            {
                var token = state.Properties().FirstOrDefault(item => String.Equals(item.Name, property.Name, StringComparison.OrdinalIgnoreCase))?.Value;
                if (token == null || token.Type == JTokenType.Null) continue;

                if (property.PropertyType == typeof(EntityHeader<SetCondition>))
                {
                    if (Enum.TryParse<SetCondition>(token.Value<string>(), true, out var condition)) property.SetValue(snapshot, EntityHeader<SetCondition>.Create(condition));
                }
                else if (property.PropertyType == typeof(int?)) property.SetValue(snapshot, token.Value<int?>());
                else if (property.PropertyType == typeof(string)) property.SetValue(snapshot, token.Value<string>());
                else if (property.PropertyType == typeof(List<string>) && token is JArray values) property.SetValue(snapshot, values.Values<string>().ToList());
                else if (property.PropertyType == typeof(AuthOneTimeCodeState) && token is JObject code)
                {
                    var codeState = new AuthOneTimeCodeState { AttemptCount = code.Value<int?>("attemptCount") };
                    if (Enum.TryParse<AuthOneTimeCodeStatus>(code.Value<string>("status"), true, out var status)) codeState.Status = status;
                    property.SetValue(snapshot, codeState);
                }
            }

            return snapshot;
        }

        private static EntityHeader ResolveView(string viewKey, IReadOnlyDictionary<string, EntityHeader> viewMap)
        {
            if (String.IsNullOrWhiteSpace(viewKey)) return null;
            if (viewMap.TryGetValue(viewKey, out var view)) return EntityHeader.Create(view.Id, view.Text);
            if (viewKey.StartsWith("app.", StringComparison.OrdinalIgnoreCase)) return EntityHeader.Create(ToRuntimeEntityId(viewKey), viewKey);
            throw new InvalidDataException($"Canonical scenario references missing AuthView '{viewKey}'.");
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

        private static string RequiredString(JObject json, string propertyName, string source, string parent = null)
        {
            if (json == null) throw new InvalidDataException($"Canonical auth scenario '{source}' is missing required object '{parent ?? propertyName}'.");
            var value = json.Value<string>(propertyName);
            if (String.IsNullOrWhiteSpace(value)) throw new InvalidDataException($"Canonical auth scenario '{source}' is missing required property '{(String.IsNullOrWhiteSpace(parent) ? propertyName : parent + "." + propertyName)}'.");
            return value;
        }

        private static Task ReadOnlyAsync() => Task.FromException(new NotSupportedException("Canonical authentication scenarios are read-only and must be changed in auth-model JSON."));
    }
}
