using LagoVista.Core;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Models.Security;
using LagoVista.UserAdmin.Models.Testing;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    public partial class AppUserTestingManager
    {
        private static EntityHeader<SetCondition> ToCondition(bool value)
        {
            return EntityHeader<SetCondition>.Create(value ? SetCondition.Set : SetCondition.NotSet);
        }

        private static bool IsLockedOut(AppUser appUser)
        {
            if (appUser == null || !appUser.LockoutEnabled || String.IsNullOrWhiteSpace(appUser.LockoutDate)) return false;

            try
            {
                return appUser.LockoutDate.ToDateTime() > DateTime.UtcNow;
            }
            catch
            {
                return false;
            }
        }

        private async Task<AuthTenantStateSnapshot> BuildFinalSnapshotAsync()
        {
            var appUser = await _appUserRepo.FindByIdAsync(TestUserSeed.User.Id);
            if (appUser == null)
            {
                return new AuthTenantStateSnapshot
                {
                    EnsureUserExists = EntityHeader<SetCondition>.Create(SetCondition.NotSet),
                    EnsureUserDoesNotExist = EntityHeader<SetCondition>.Create(SetCondition.Set)
                };
            }

            return new AuthTenantStateSnapshot
            {
                EnsureUserExists = EntityHeader<SetCondition>.Create(SetCondition.Set),
                EnsureUserDoesNotExist = EntityHeader<SetCondition>.Create(SetCondition.NotSet),
                BelongsToOrg = ToCondition(appUser.Organizations != null && appUser.Organizations.Count > 0),
                EmailConfirmed = ToCondition(appUser.EmailConfirmed),
                UserHasEmail = ToCondition(!String.IsNullOrWhiteSpace(appUser.Email)),
                UserHasFirstName = ToCondition(!String.IsNullOrWhiteSpace(appUser.FirstName)),
                UserHasLastName = ToCondition(!String.IsNullOrWhiteSpace(appUser.LastName)),
                PhoneNumberConfirmed = ToCondition(appUser.PhoneNumberConfirmed),
                TwoFactorEnabled = ToCondition(appUser.TwoFactorEnabled),
                IsAccountDisabled = ToCondition(appUser.IsAccountDisabled),
                IsLockedOut = ToCondition(IsLockedOut(appUser)),
                AccessFailedCount = appUser.AccessFailedCount,
                HasLastLogin = ToCondition(!String.IsNullOrWhiteSpace(appUser.LastLogin)),
                IsOrgAdmin = ToCondition(appUser.IsOrgAdmin),
                HasPassword = ToCondition(!String.IsNullOrWhiteSpace(appUser.PasswordHash)),
                IsAnonymous = ToCondition(appUser.IsAnonymous),
                ShowWelcome = ToCondition(appUser.ShowWelcome),
                LastMfaDateTimeUtc = appUser.LastMfaDateTimeUtc
            };
        }

        private static void CompareCondition(string name, EntityHeader<SetCondition> expected, EntityHeader<SetCondition> actual, List<string> errors)
        {
            if (expected == null || expected.Value == SetCondition.DontCare) return;
            if (actual == null || actual.Value != expected.Value) errors.Add($"Postcondition '{name}' expected '{expected.Value}' but observed '{actual?.Value.ToString() ?? "null"}'.");
        }

        private static void ComparePostConditions(AuthTenantStateSnapshot expected, AuthTenantStateSnapshot actual, List<string> errors)
        {
            if (expected == null) return;

            CompareCondition(nameof(expected.EnsureUserExists), expected.EnsureUserExists, actual.EnsureUserExists, errors);
            CompareCondition(nameof(expected.EnsureUserDoesNotExist), expected.EnsureUserDoesNotExist, actual.EnsureUserDoesNotExist, errors);
            CompareCondition(nameof(expected.BelongsToOrg), expected.BelongsToOrg, actual.BelongsToOrg, errors);
            CompareCondition(nameof(expected.EmailConfirmed), expected.EmailConfirmed, actual.EmailConfirmed, errors);
            CompareCondition(nameof(expected.UserHasEmail), expected.UserHasEmail, actual.UserHasEmail, errors);
            CompareCondition(nameof(expected.UserHasFirstName), expected.UserHasFirstName, actual.UserHasFirstName, errors);
            CompareCondition(nameof(expected.UserHasLastName), expected.UserHasLastName, actual.UserHasLastName, errors);
            CompareCondition(nameof(expected.PhoneNumberConfirmed), expected.PhoneNumberConfirmed, actual.PhoneNumberConfirmed, errors);
            CompareCondition(nameof(expected.TwoFactorEnabled), expected.TwoFactorEnabled, actual.TwoFactorEnabled, errors);
            CompareCondition(nameof(expected.IsAccountDisabled), expected.IsAccountDisabled, actual.IsAccountDisabled, errors);
            CompareCondition(nameof(expected.IsLockedOut), expected.IsLockedOut, actual.IsLockedOut, errors);
            CompareCondition(nameof(expected.HasLastLogin), expected.HasLastLogin, actual.HasLastLogin, errors);
            CompareCondition(nameof(expected.IsOrgAdmin), expected.IsOrgAdmin, actual.IsOrgAdmin, errors);
            CompareCondition(nameof(expected.HasPassword), expected.HasPassword, actual.HasPassword, errors);
            CompareCondition(nameof(expected.IsAnonymous), expected.IsAnonymous, actual.IsAnonymous, errors);
            CompareCondition(nameof(expected.ShowWelcome), expected.ShowWelcome, actual.ShowWelcome, errors);

            if (expected.AccessFailedCount.HasValue && expected.AccessFailedCount.Value != actual.AccessFailedCount)
                errors.Add($"Postcondition '{nameof(expected.AccessFailedCount)}' expected '{expected.AccessFailedCount.Value}' but observed '{actual.AccessFailedCount}'.");

            if (!String.IsNullOrWhiteSpace(expected.LastMfaDateTimeUtc) && !String.Equals(expected.LastMfaDateTimeUtc, actual.LastMfaDateTimeUtc, StringComparison.Ordinal))
                errors.Add($"Postcondition '{nameof(expected.LastMfaDateTimeUtc)}' did not match the observed value.");
        }

        private static DateTime ParseRunTime(string value, DateTime fallback)
        {
            if (String.IsNullOrWhiteSpace(value)) return fallback;
            if (DateTime.TryParse(value, null, System.Globalization.DateTimeStyles.RoundtripKind, out var parsed)) return parsed.ToUniversalTime();
            return fallback;
        }

        private async Task<List<string>> LoadObservedAuthEventsAsync(AppUserTestRun run, EntityHeader org, EntityHeader user)
        {
            var fromUtc = ParseRunTime(run.Started, DateTime.UtcNow.AddMinutes(-5)).AddSeconds(-5);
            var toUtc = ParseRunTime(run.Finished, DateTime.UtcNow).AddSeconds(5);
            var request = new ListRequest
            {
                PageSize = 100,
                StartDate = CalendarDate.Create(fromUtc.Year, fromUtc.Month, fromUtc.Day),
                EndDate = CalendarDate.Create(toUtc.Year, toUtc.Month, toUtc.Day)
            };

            var byUserId = await _authLogMgr.GetForUserIdAsync(TestUserSeed.User.Id, request, org, user);
            var byUserName = await _authLogMgr.GetForUserNameAsync(TestUserSeed.Email.ToLowerInvariant(), request, org, user);

            return (byUserId.Model ?? Enumerable.Empty<AuthenticationLog>())
                .Concat(byUserName.Model ?? Enumerable.Empty<AuthenticationLog>())
                .Where(log => log.CreationDate >= fromUtc && log.CreationDate <= toUtc)
                .Select(log => log.AuthType)
                .Where(authType => !String.IsNullOrWhiteSpace(authType))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private async Task<AuthLogReviewSummary> ReviewAuthLogsAsync(AppUserTestRun run, AppUserTestScenario scenario, EntityHeader org, EntityHeader user)
        {
            var expected = scenario.ExpectedAuthLogEvents ?? new List<string>();
            var review = new AuthLogReviewSummary
            {
                FromUtc = ParseRunTime(run.Started, DateTime.UtcNow.AddMinutes(-5)).ToString("O"),
                ToUtc = ParseRunTime(run.Finished, DateTime.UtcNow).ToString("O")
            };

            for (var attempt = 0; attempt < 5; attempt++)
            {
                review.ObservedEvents = await LoadObservedAuthEventsAsync(run, org, user);
                review.ExpectedEventsMissing = expected.Where(item => !review.ObservedEvents.Contains(item, StringComparer.OrdinalIgnoreCase)).ToList();
                if (review.ExpectedEventsMissing.Count == 0 || attempt == 4) break;
                await Task.Delay(250);
            }

            return review;
        }

        private async Task VerifyCompletedRunAsync(AppUserTestRun run, AppUserTestScenario scenario, EntityHeader org, EntityHeader user)
        {
            run.Verification ??= new TestRunVerification();
            run.Verification.Errors ??= new List<string>();
            run.Verification.Warnings ??= new List<string>();

            if (scenario.ExpectedView != null && !String.IsNullOrWhiteSpace(scenario.ExpectedView.Id))
            {
                var expectedViewId = scenario.ExpectedView.Id;
                if (!expectedViewId.StartsWith("app.", StringComparison.OrdinalIgnoreCase))
                {
                    var expectedView = await _authViewRepo.GetByIdAsync(expectedViewId);
                    if (expectedView != null)
                        expectedViewId = expectedView.ViewId;
                }

                if (!String.Equals(expectedViewId, run.FinalViewId, StringComparison.OrdinalIgnoreCase))
                    run.Verification.Errors.Add($"Expected final view '{expectedViewId}' but runner observed '{run.FinalViewId ?? "null"}'.");
            }

            run.Verification.FinalSnapshot = await BuildFinalSnapshotAsync();
            ComparePostConditions(scenario.PostConditions, run.Verification.FinalSnapshot, run.Verification.Errors);

            run.Verification.AuthLogReview = await ReviewAuthLogsAsync(run, scenario, org, user);
            foreach (var missingEvent in run.Verification.AuthLogReview.ExpectedEventsMissing)
                run.Verification.Errors.Add($"Expected auth log event '{missingEvent}' was not observed during the run.");

            if (run.Verification.Errors.Count > 0)
            {
                run.Status = TestRunStatus.Failed;
                run.ErrorMessage = String.Join(" | ", run.Verification.Errors);
            }
        }
    }
}
