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

            var expectedCanonicalViewId = scenario.ExpectedView?.Text?.StartsWith("app.", StringComparison.OrdinalIgnoreCase) == true ? scenario.ExpectedView.Text : null;
            AuthView expectedView = null;
            if (String.IsNullOrWhiteSpace(expectedCanonicalViewId) && !String.IsNullOrWhiteSpace(scenario.ExpectedView?.Id))
            {
                expectedView = await _authViewRepo.GetByIdAsync(scenario.ExpectedView.Id);
                expectedCanonicalViewId = expectedView?.ViewId;
            }

            var selectedAction = ResolveAction(authView, scenario.ActionId, scenario.ActionFinder, scenario.Action);
            var actionFinder = selectedAction?.Finder ?? ToTestIdFinder(scenario.ActionFinder ?? scenario.Action?.Text);

            if (String.IsNullOrEmpty(authView.WebRoute ?? authView.Route)) return InvokeResult<AuthRunnerPlan>.FromError("MissingRoute", "AuthView web route is required.");
            if (String.IsNullOrEmpty(authView.RouteId)) return InvokeResult<AuthRunnerPlan>.FromError("MissingRouteId", "AuthView.RouteId is required.");
            if (String.IsNullOrEmpty(authView.ViewId)) return InvokeResult<AuthRunnerPlan>.FromError("MissingViewId", "AuthView.ViewId is required.");
            if (String.IsNullOrEmpty(actionFinder)) return InvokeResult<AuthRunnerPlan>.FromError("MissingActionFinder", $"Scenario action '{scenario.ActionId ?? scenario.Action?.Text ?? scenario.Action?.Id ?? "(null)"}' does not provide a usable finder.");

            foreach (var input in scenario.Inputs ?? new List<AppUserTestSettingsValue>())
            {
                if (String.IsNullOrEmpty(input.Finder)) return InvokeResult<AuthRunnerPlan>.FromError("MissingInputFinder", $"Input '{input.Name}' is missing Finder.");
            }

            // State setup is a server responsibility. Do it before constructing inputs so every
            // symbolic canonical value can be converted into a concrete value for the runner.
            var credentialsResult = await ApplySetupAsync(scenarioId, org, user);
            if (!credentialsResult.Successful) return credentialsResult.ToInvokeResult<AuthRunnerPlan>();
            var credentials = credentialsResult.Result ?? new TestUserCredentials();

            // The canonical test identity remains useful even for scenarios that deliberately
            // remove or omit the user record. Do not make the UI runner infer it locally.
            if (String.IsNullOrWhiteSpace(credentials.EmailAddress)) credentials.EmailAddress = TestUserSeed.Email;
            if (String.IsNullOrWhiteSpace(credentials.InvalidPassword) && !String.IsNullOrWhiteSpace(credentials.Password)) credentials.InvalidPassword = credentials.Password + "XYZ";

            var preparedInputs = new List<AuthRunnerInput>();
            foreach (var input in scenario.Inputs ?? new List<AppUserTestSettingsValue>())
            {
                var preparedValue = ResolvePreparedInputValue(input.Value, credentials);
                if (!String.IsNullOrWhiteSpace(preparedValue) && preparedValue.IndexOf("user.", StringComparison.OrdinalIgnoreCase) >= 0)
                    return InvokeResult<AuthRunnerPlan>.FromError("UnresolvedInputValue", $"Input '{input.Name}' contains unresolved symbolic value '{preparedValue}'.");

                preparedInputs.Add(new AuthRunnerInput
                {
                    Name = input.Name,
                    Finder = input.Finder,
                    Value = preparedValue,
                    ValueType = input.ValueType,
                    Required = input.Required,
                    Kind = authView.Fields?.FirstOrDefault(field => String.Equals(field.Finder, input.Finder, StringComparison.OrdinalIgnoreCase))?.FieldType ?? "unknown"
                });
            }

            var start = new AuthRunnerViewTarget
            {
                ViewId = authView.ViewId,
                WebRoute = NormalizeRoute(authView.WebRoute ?? authView.Route),
                MobileRoute = authView.MobileRoute
            };

            var expected = new AuthRunnerViewTarget
            {
                ViewId = expectedView?.ViewId ?? expectedCanonicalViewId,
                WebRoute = expectedView == null ? null : NormalizeRoute(expectedView.WebRoute ?? expectedView.Route),
                MobileRoute = expectedView?.MobileRoute
            };

            var plan = new AuthRunnerPlan
            {
                RunId = Guid.NewGuid().ToString("N"),
                Scenario = scenario.ToEntityHeader(),
                ScenarioCanonicalKey = scenario.CanonicalKey,
                ScenarioName = scenario.Name,
                ScenarioDefinitionVersion = scenario.DefinitionVersion,
                ScenarioDefinitionHash = scenario.DefinitionHash,
                Start = start,
                Expected = expected,
                StartRoute = start.WebRoute,
                StartViewId = start.ViewId,
                Inputs = preparedInputs,
                Action = new AuthRunnerAction
                {
                    Id = selectedAction?.ActionId ?? scenario.ActionId,
                    Name = selectedAction?.Name ?? scenario.ActionId ?? scenario.Action?.Id,
                    Finder = actionFinder,
                    ActionType = selectedAction?.ActionType
                },
                Observations = new AuthRunnerObservations
                {
                    ExpectedEndViewId = expected.ViewId,
                    ExpectedEndRoute = expected.WebRoute,
                    ExpectedVisibleFinders = (scenario.ExpectedVisibleFinders ?? new List<string>()).Select(ToTestIdFinder).ToList(),
                    BusyStateFinder = "[data-testid=\"state:busy\"]"
                },
                Options = new AuthRunnerOptions
                {
                    Headless = headless,
                    SlowMoMs = headless ? 0 : 50,
                    TimeoutMs = 30000,
                    EnableTracing = false
                },
                UserCredentials = credentials
            };

            return InvokeResult<AuthRunnerPlan>.Create(plan);
        }

        private static string ResolvePreparedInputValue(string value, TestUserCredentials credentials)
        {
            if (String.IsNullOrEmpty(value)) return value;

            var result = value;
            result = ReplaceToken(result, "user.email", credentials?.EmailAddress);
            result = ReplaceToken(result, "user.invalid-password", credentials?.InvalidPassword);
            result = ReplaceToken(result, "user.password", credentials?.Password);
            result = ReplaceToken(result, "user.password-recovery-code", credentials?.PasswordRecoveryCode);
            result = ReplaceToken(result, "user.email-verification-code", credentials?.EmailVerificationCode);
            result = ReplaceToken(result, "user.password-reset-token", credentials?.PasswordResetToken);
            result = ReplaceToken(result, "user.email-confirmation-token", credentials?.EmailConfirmationToken);
            result = ReplaceToken(result, "user.magic-link-token", credentials?.MagicLinkToken);
            result = ReplaceToken(result, "user.prelogin-link", credentials?.PreloginLink);
            result = ReplaceToken(result, "user.passkey-credentials-id", credentials?.PasskeyCredentialsId);
            result = ReplaceToken(result, "user.user-id", credentials?.UserId);
            result = ReplaceToken(result, "user.invite-id", credentials?.InviteId);
            return result;
        }

        private static string ReplaceToken(string value, string token, string replacement)
        {
            if (String.IsNullOrEmpty(value) || String.IsNullOrEmpty(replacement) || value.IndexOf(token, StringComparison.OrdinalIgnoreCase) < 0) return value;
            var index = value.IndexOf(token, StringComparison.OrdinalIgnoreCase);
            while (index >= 0)
            {
                value = value.Substring(0, index) + replacement + value.Substring(index + token.Length);
                index = value.IndexOf(token, index + replacement.Length, StringComparison.OrdinalIgnoreCase);
            }
            return value;
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
