using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Models.Testing;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Testing
{
    public class AppUserTestingDslRepo : IAppUserTestingDslRepo
    {
        private readonly IAdminLogger _adminLogger;
        private readonly string _authModelRoot;

        public AppUserTestingDslRepo(IUserAdminSettings userAdminSettings, IAdminLogger adminLogger)
        {
            _ = userAdminSettings ?? throw new ArgumentNullException(nameof(userAdminSettings));
            _adminLogger = adminLogger ?? throw new ArgumentNullException(nameof(adminLogger));
            _authModelRoot = Path.Combine(AppContext.BaseDirectory, "auth-model");
        }

        public Task AddDSLAsync(AppUserTestScenario dsl) => ReadOnlyAsync();

        public Task DeleteByIdAsync(string id) => ReadOnlyAsync();

        public Task UpdateTestScenarioAsync(AppUserTestScenario dsl) => ReadOnlyAsync();

        public Task<AppUserTestScenario> GetByIdAsync(string id)
        {
            if (String.IsNullOrWhiteSpace(id))
                return Task.FromResult<AppUserTestScenario>(null);

            var scenario = LoadScenarios().FirstOrDefault(item => String.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase) || String.Equals(item.Key, id, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(scenario);
        }

        public Task<ListResponse<AppUserTestScenarioSummary>> ListAsync(string orgId, ListRequest request)
        {
            _ = orgId;
            request ??= ListRequest.CreateForAll();

            var summaries = LoadScenarios().Select(item => item.CreateSummary()).OrderBy(item => item.Name).ToList();
            var page = summaries.Skip(request.PageSize * (request.PageIndex - 1)).Take(request.PageSize);
            return Task.FromResult(ListResponse<AppUserTestScenarioSummary>.Create(request, page));
        }

        private List<AppUserTestScenario> LoadScenarios()
        {
            var scenarioRoot = Path.Combine(_authModelRoot, "scenarios-v2");
            if (!Directory.Exists(scenarioRoot))
                throw new DirectoryNotFoundException($"Published auth scenario directory was not found: '{scenarioRoot}'.");

            var viewMap = LoadAuthViewMap();
            var scenarios = Directory.GetFiles(scenarioRoot, "*.json", SearchOption.AllDirectories).Select(path => HydrateScenario(path, viewMap)).OrderBy(item => item.Name).ToList();
            _adminLogger.Trace($"[AppUserTestingDslRepo__LoadScenarios] Loaded {scenarios.Count} canonical auth scenarios from published JSON.");
            return scenarios;
        }

        private Dictionary<string, EntityHeader> LoadAuthViewMap()
        {
            var viewRoot = Path.Combine(_authModelRoot, "auth-views");
            if (!Directory.Exists(viewRoot))
                throw new DirectoryNotFoundException($"Published auth view directory was not found: '{viewRoot}'.");

            var result = new Dictionary<string, EntityHeader>(StringComparer.OrdinalIgnoreCase);
            foreach (var path in Directory.GetFiles(viewRoot, "*.json", SearchOption.TopDirectoryOnly))
            {
                var json = JObject.Parse(System.IO.File.ReadAllText(path));
                var viewId = json.Value<string>("viewId");
                if (String.IsNullOrWhiteSpace(viewId))
                    continue;

                var runtimeEntityId = json["source"]?.Value<string>("runtimeEntityId");
                var name = json.Value<string>("name") ?? viewId;
                result[viewId] = EntityHeader.Create(String.IsNullOrWhiteSpace(runtimeEntityId) ? viewId : runtimeEntityId, name);
            }

            return result;
        }

        private static AppUserTestScenario HydrateScenario(string path, IReadOnlyDictionary<string, EntityHeader> viewMap)
        {
            var json = JObject.Parse(System.IO.File.ReadAllText(path));
            var key = RequiredString(json, "key", path);
            var action = json["action"] as JObject;
            var actionId = action?.Value<string>("id");
            var actionFinder = action?.Value<string>("finder");

            var scenario = new AppUserTestScenario
            {
                Id = key,
                Key = key,
                Name = RequiredString(json, "name", path),
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

        private static string RequiredString(JObject json, string propertyName, string path)
        {
            var value = json.Value<string>(propertyName);
            if (String.IsNullOrWhiteSpace(value))
                throw new InvalidDataException($"Canonical auth scenario '{path}' is missing required property '{propertyName}'.");
            return value;
        }

        private static Task ReadOnlyAsync() => Task.FromException(new NotSupportedException("Canonical authentication scenarios are read-only and must be changed in auth-model JSON."));
    }
}
