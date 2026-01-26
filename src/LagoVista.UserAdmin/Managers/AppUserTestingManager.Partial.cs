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

            await AuthorizeAsync(scenario, AuthorizeResult.AuthorizeActions.Read, user, org);

            if (scenario.AuthView == null || String.IsNullOrEmpty(scenario.AuthView.Id))
                return InvokeResult<AuthRunnerPlan>.FromError("MissingAuthView", "Scenario.AuthView is required.");

            var authView = await _authViewRepo.GetByIdAsync(scenario.AuthView.Id);
            if (authView == null) return InvokeResult<AuthRunnerPlan>.FromError("AuthViewNotFound", $"AuthView '{scenario.AuthView.Id}' not found.");

            var expectedView = await _authViewRepo.GetByIdAsync(scenario.ExpectedView.Id);
            if (expectedView == null) return InvokeResult<AuthRunnerPlan>.FromError("AuthViewNotFound", $"Expected View '{scenario.ExpectedView.Id}' not found.");

            await AuthorizeAsync(authView, AuthorizeResult.AuthorizeActions.Read, user, org);

            // Resolve action (prefer exact id match; fall back to name/key/text comparisons).
            var selectedAction = ResolveAction(authView, scenario.Action);
            if (selectedAction == null)
                return InvokeResult<AuthRunnerPlan>.FromError("ActionNotFound",
                    $"Could not resolve Scenario.Action '{scenario.Action?.Text ?? scenario.Action?.Id ?? "(null)"}' on view '{authView.ViewId}'.");

            // Validate finders are present (minimum viable checks; more can be added later).
            if (String.IsNullOrEmpty(authView.Route)) return InvokeResult<AuthRunnerPlan>.FromError("MissingRoute", "AuthView.Route is required.");
            if (String.IsNullOrEmpty(authView.ViewId)) return InvokeResult<AuthRunnerPlan>.FromError("MissingViewId", "AuthView.ViewId is required.");
            if (String.IsNullOrEmpty(selectedAction.Finder)) return InvokeResult<AuthRunnerPlan>.FromError("MissingActionFinder", "Selected action Finder is required.");

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
                    Kind = authView.Fields?.FirstOrDefault(f => String.Equals(f.Finder, i.Finder, StringComparison.OrdinalIgnoreCase)).FieldType ?? "unknown"   
                }).ToList(),
                Action = new AuthRunnerAction()
                {
                    Name = selectedAction.Name,
                    Finder = selectedAction.Finder
                },
                Observations = new AuthRunnerObservations()
                {
                    ExpectedEndViewId = expectedView.ViewId,  // optional hint (or Id if you store it that way)
                    ExpectedEndRoute = expectedView.Route,
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

            // 1) exact id match (best)
            var byId = view.Actions.FirstOrDefault(a => String.Equals(a.Id, scenarioAction.Id, StringComparison.OrdinalIgnoreCase));
            if (byId != null) return byId;

            // 2) match on Name vs EntityHeader.Text/Key/Id
            var token = (scenarioAction.Text ?? scenarioAction.Id ?? String.Empty).Trim();
            if (String.IsNullOrEmpty(token)) return null;

            var byName = view.Actions.FirstOrDefault(a => String.Equals(a.Name, token, StringComparison.OrdinalIgnoreCase));
            if (byName != null) return byName;

            // 3) allow "action:next" style values (strip prefix)
            var normalized = token.StartsWith("action:", StringComparison.OrdinalIgnoreCase) ? token.Substring(7) : token;
            var byNormalizedName = view.Actions.FirstOrDefault(a => String.Equals(a.Name, normalized, StringComparison.OrdinalIgnoreCase));
            if (byNormalizedName != null) return byNormalizedName;

            return null;
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
