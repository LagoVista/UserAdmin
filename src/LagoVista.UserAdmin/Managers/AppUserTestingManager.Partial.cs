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
            {
                expectedView = await _authViewRepo.GetByIdAsync(expectedViewId);
                if (expectedView == null) return InvokeResult<AuthRunnerPlan>.FromError("AuthViewNotFound", $"Expected View '{expectedViewId}' not found.");
            }

            await AuthorizeAsync(authView, AuthorizeResult.AuthorizeActions.Read, user, org);

            var selectedAction = ResolveAction(authView, scenario.Action);
            var actionFinder = selectedAction?.Finder ?? ToTestIdFinder(scenario.Action?.Text);

            if (String.IsNullOrEmpty(authView.Route)) return InvokeResult<AuthRunnerPlan>.FromError("MissingRoute", "AuthView.Route is required.");
            if (String.IsNullOrEmpty(authView.ViewId)) return InvokeResult<AuthRunnerPlan>.FromError("MissingViewId", "AuthView.ViewId is required.");
            if (String.IsNullOrEmpty(actionFinder)) return InvokeResult<AuthRunnerPlan>.FromError("MissingActionFinder", $"Scenario.Action '{scenario.Action?.Text ?? scenario.Action?.Id ?? "(null)"}' does not provide a usable finder.");

            foreach (var input in scenario.Inputs ?? new List<AppUserTestSettingsValue>())
            {
                if (String.IsNullOrEmpty(input.Finder))
                    return InvokeResult<AuthRunnerPlan>.FromError("MissingInputFinder", $"Input '{input.Name}' is missing Finder.");
            }

            var plan = new AuthRunnerPlan()
            {
                RunId = Guid.NewGuid().ToString("N"),
                Scenario = scenario.ToEntityHeader(),
                StartRoute = NormalizeRoute(authView.Route),
                StartViewId = authView.ViewId,
                Inputs = (scenario.Inputs ?? new List<AppUserTestSettingsValue>()).Select(i => new AuthRunnerInput()
                {
                    Name = i.Name,
                    Finder = i.Finder,
                    Value = i.Value,
                    Kind = authView.Fields?.FirstOrDefault(f => String.Equals(f.Finder, i.Finder, StringComparison.OrdinalIgnoreCase))?.FieldType ?? "unknown"
                }).ToList(),
                Action = new AuthRunnerAction()
                {
                    Name = selectedAction?.Name ?? scenario.Action?.Id,
                    Finder = actionFinder
                },
                Observations = new AuthRunnerObservations()
                {
                    ExpectedEndViewId = expectedView?.ViewId ?? expectedViewId,
                    ExpectedEndRoute = expectedView?.Route,
                    BusyStateFinder = "[data-testid=\"state:busy\"]"
                },
                Options = new AuthRunnerOptions()
                {
                    Headless = headless,
                    SlowMoMs = headless ? 0 : 50,
                    TimeoutMs = 30000,
                    EnableTracing = false
                }
            };

            var credenentialsResult = await ApplySetupAsync(scenarioId, org, user);
            if (!credenentialsResult.Successful) return credenentialsResult.ToInvokeResult<AuthRunnerPlan>();
            plan.UserCredentials = credenentialsResult.Result;

            return InvokeResult<AuthRunnerPlan>.Create(plan);
        }

        private static AuthFieldAction ResolveAction(AuthView view, EntityHeader scenarioAction)
        {
            if (scenarioAction == null || view?.Actions == null) return null;

            var byId = view.Actions.FirstOrDefault(a => String.Equals(a.Id, scenarioAction.Id, StringComparison.OrdinalIgnoreCase));
            if (byId != null) return byId;

            var token = (scenarioAction.Text ?? scenarioAction.Id ?? String.Empty).Trim();
            if (String.IsNullOrEmpty(token)) return null;

            var byName = view.Actions.FirstOrDefault(a => String.Equals(a.Name, token, StringComparison.OrdinalIgnoreCase));
            if (byName != null) return byName;

            var normalized = token.StartsWith("action:", StringComparison.OrdinalIgnoreCase) ? token.Substring(7) : token;
            var byNormalizedName = view.Actions.FirstOrDefault(a => String.Equals(a.Name, normalized, StringComparison.OrdinalIgnoreCase));
            if (byNormalizedName != null) return byNormalizedName;

            return null;
        }

        private static string ToTestIdFinder(string finder)
        {
            if (String.IsNullOrWhiteSpace(finder) || finder.StartsWith("[", StringComparison.Ordinal))
                return finder;

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
