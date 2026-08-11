using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Testing;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    public partial class AppUserTestingManager
    {
        public async Task<InvokeResult<AuthRunnerPlan>> BuildRunnerPlanAsync(string scenarioId, bool headless, EntityHeader org, EntityHeader user)
        {
            if (String.IsNullOrEmpty(scenarioId)) return InvokeResult<AuthRunnerPlan>.FromError("MissingScenarioId", "scenarioId is required.");

            var scenario = await _testScenarioRepo.GetByIdAsync(scenarioId);
            if (scenario == null) return InvokeResult<AuthRunnerPlan>.FromError("ScenarioNotFound", $"Scenario '{scenarioId}' not found.");

            await AuthorizeOrgAccessAsync(user, org, typeof(AppUserTestScenario), Actions.Read);

            if (scenario.AuthView == null || String.IsNullOrEmpty(scenario.AuthView.Id))
                return InvokeResult<AuthRunnerPlan>.FromError("MissingAuthView", "Scenario.AuthView is required.");

            var authView = await _authViewRepo.GetByIdAsync(scenario.AuthView.Id);
            if (authView == null) return InvokeResult<AuthRunnerPlan>.FromError("AuthViewNotFound", $"AuthView '{scenario.AuthView.Id}' not found.");

            AuthView expectedView = null;
            var expectedViewId = scenario.ExpectedView?.Id;
            if (!String.IsNullOrWhiteSpace(expectedViewId) && !expectedViewId.StartsWith("app.", StringComparison.OrdinalIgnoreCase))
                expectedView = await _authViewRepo.GetByIdAsync(expectedViewId);

            await AuthorizeAsync(authView, AuthorizeResult.AuthorizeActions.Read, user, org);

            var selectedAction = ResolveAction(authView, scenario.ActionId, scenario.ActionFinder, scenario.Action);
            var actionFinder = selectedAction?.Finder ?? ToTestIdFinder(scenario.ActionFinder ?? scenario.Action?.Text);

            if (String.IsNullOrEmpty(authView.Route)) return InvokeResult<AuthRunnerPlan>.FromError("MissingRoute", "AuthView.Route is required.");
            if (String.IsNullOrEmpty(authView.RouteId)) return InvokeResult<AuthRunnerPlan>.FromError("MissingRouteId", "AuthView.RouteId is required.");
            if (String.IsNullOrEmpty(authView.ViewId)) return InvokeResult<AuthRunnerPlan>.FromError("MissingViewId", "AuthView.ViewId is required.");
            if (String.IsNullOrEmpty(actionFinder)) return InvokeResult<AuthRunnerPlan>.FromError("MissingActionFinder", $"Scenario action '{scenario.ActionId ?? scenario.Action?.Text ?? scenario.Action?.Id ?? "(null)"}' does not provide a usable finder.");

            foreach (var input in scenario.Inputs ?? new List<AppUserTestSettingsValue>())
            {
                if (String.IsNullOrEmpty(input.Finder)) return InvokeResult<AuthRunnerPlan>.FromError("MissingInputFinder", $"Input '{input.Name}' is missing Finder.");
            }

            var plan = new AuthRunnerPlan
            {
                RunId = Guid.NewGuid().ToString("N"),
                Scenario = scenario.ToEntityHeader(),
                ScenarioCanonicalKey = scenario.CanonicalKey,
                ScenarioDefinitionVersion = scenario.DefinitionVersion,
                ScenarioDefinitionHash = scenario.DefinitionHash,
                StartRoute = NormalizeRoute(authView.Route),
                StartViewId = authView.ViewId,
                Inputs = (scenario.Inputs ?? new List<AppUserTestSettingsValue>()).Select(input => new AuthRunnerInput
                {
                    Name = input.Name,
                    Finder = input.Finder,
                    Value = input.Value,
                    ValueType = input.ValueType,
                    Required = input.Required,
                    Kind = authView.Fields?.FirstOrDefault(field => String.Equals(field.Finder, input.Finder, StringComparison.OrdinalIgnoreCase))?.FieldType ?? "unknown"
                }).ToList(),
                Action = new AuthRunnerAction
                {
                    Id = selectedAction?.ActionId ?? scenario.ActionId,
                    Name = selectedAction?.Name ?? scenario.ActionId ?? scenario.Action?.Id,
                    Finder = actionFinder,
                    ActionType = selectedAction?.ActionType
                },
                Observations = new AuthRunnerObservations
                {
                    ExpectedEndViewId = expectedView?.ViewId ?? expectedViewId,
                    ExpectedEndRoute = expectedView?.Route,
                    ExpectedVisibleFinders = (scenario.ExpectedVisibleFinders ?? new List<string>()).Select(ToTestIdFinder).ToList(),
                    BusyStateFinder = "[data-testid=\"state:busy\"]"
                },
                Options = new AuthRunnerOptions
                {
                    Headless = headless,
                    SlowMoMs = headless ? 0 : 50,
                    TimeoutMs = 30000,
                    EnableTracing = false
                }
            };

            var credentialsResult = await ApplySetupAsync(scenarioId, org, user);
            if (!credentialsResult.Successful) return credentialsResult.ToInvokeResult<AuthRunnerPlan>();
            plan.UserCredentials = credentialsResult.Result;

            return InvokeResult<AuthRunnerPlan>.Create(plan);
        }

        private static AuthFieldAction ResolveAction(AuthView view, string actionId, string actionFinder, EntityHeader legacyScenarioAction)
        {
            if (view?.Actions == null) return null;

            if (!String.IsNullOrWhiteSpace(actionId))
            {
                var byCanonicalId = view.Actions.FirstOrDefault(action => String.Equals(action.ActionId, actionId, StringComparison.OrdinalIgnoreCase));
                if (byCanonicalId != null) return byCanonicalId;
            }

            if (!String.IsNullOrWhiteSpace(actionFinder))
            {
                var byCanonicalFinder = view.Actions.FirstOrDefault(action => String.Equals(action.Finder, ToTestIdFinder(actionFinder), StringComparison.OrdinalIgnoreCase));
                if (byCanonicalFinder != null) return byCanonicalFinder;
            }

            if (legacyScenarioAction == null) return null;

            var byId = view.Actions.FirstOrDefault(action => String.Equals(action.Id, legacyScenarioAction.Id, StringComparison.OrdinalIgnoreCase));
            if (byId != null) return byId;

            var token = (legacyScenarioAction.Text ?? legacyScenarioAction.Id ?? String.Empty).Trim();
            if (String.IsNullOrEmpty(token)) return null;

            var finder = ToTestIdFinder(token);
            var byFinder = view.Actions.FirstOrDefault(action => String.Equals(action.Finder, finder, StringComparison.OrdinalIgnoreCase));
            if (byFinder != null) return byFinder;

            return view.Actions.FirstOrDefault(action => String.Equals(action.Name, token, StringComparison.OrdinalIgnoreCase));
        }

        private static string ToTestIdFinder(string finder)
        {
            if (String.IsNullOrWhiteSpace(finder) || finder.StartsWith("[", StringComparison.Ordinal)) return finder;
            return $"[data-testid=\"{finder}\"]";
        }

        private static string NormalizeRoute(string route)
        {
            if (String.IsNullOrEmpty(route)) return route;
            route = route.Trim();
            if (route.StartsWith("/")) route = route.Substring(1);
            return route;
        }
    }
}
