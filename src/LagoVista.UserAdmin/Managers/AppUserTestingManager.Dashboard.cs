using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Models.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    public partial class AppUserTestingManager
    {
        public async Task<ListResponse<AppUserTestScenarioSummary>> GetPublicTestScenarioDashboardAsync(ListRequest request)
        {
            request ??= ListRequest.CreateForAll();

            var scenarios = await _testScenarioRepo.ListAsync(String.Empty, request);
            var runs = await _testRunStore.GetRunsAsync(ListRequest.CreateForAll());

            foreach (var scenario in scenarios.Model)
            {
                var matchingRuns = runs.Model.Where(run => run.TestScenario != null &&
                    (String.Equals(run.TestScenario.Id, scenario.Id, StringComparison.OrdinalIgnoreCase) ||
                     String.Equals(run.TestScenario.Text, scenario.Name, StringComparison.OrdinalIgnoreCase))).ToList();

                scenario.WebStatus = CreateDashboardStatus(AppUserTestPlatform.Web, matchingRuns);
                scenario.AndroidStatus = CreateDashboardStatus(AppUserTestPlatform.Android, matchingRuns);
                scenario.IOSStatus = CreateDashboardStatus(AppUserTestPlatform.IOS, matchingRuns);
            }

            return scenarios;
        }

        private static AppUserTestPlatformStatus CreateDashboardStatus(AppUserTestPlatform platform, IEnumerable<AppUserTestRunSummary> runs)
        {
            var latest = runs.Where(run => run.Platform == platform).OrderByDescending(GetRunTimestamp).FirstOrDefault();
            if (latest == null) return AppUserTestPlatformStatus.Create(platform);

            return new AppUserTestPlatformStatus
            {
                Platform = platform,
                Status = latest.Status,
                LastRun = !String.IsNullOrWhiteSpace(latest.Finished) ? latest.Finished : latest.Started,
                LastRunId = latest.Id,
                FinalViewId = latest.FinalViewId,
                ErrorMessage = null
            };
        }

        private static DateTimeOffset GetRunTimestamp(AppUserTestRunSummary run)
        {
            var value = !String.IsNullOrWhiteSpace(run.Finished) ? run.Finished : run.Started;
            return DateTimeOffset.TryParse(value, out var timestamp) ? timestamp : DateTimeOffset.MinValue;
        }
    }
}
