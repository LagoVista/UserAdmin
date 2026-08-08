using LagoVista.Core.Interfaces;
using LagoVista.Core.Validation;
using LagoVista.JobManager.Attributes;
using LagoVista.JobManager.Interfaces;
using LagoVista.JobManager.Models;
using LagoVista.UserAdmin.Interfaces.Managers;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.JobManager.Jobs
{
    [LagoVistaJob("useradmin.provisional-environment-lifecycle", "Provisional Environment Lifecycle", "Expires inactive provisional environments and safely purges environments whose retention period has elapsed.", "User Admin")]
    public sealed class ProvisionalEnvironmentLifecycleJob : JobBase
    {
        private const int BatchSize = 500;

        private readonly IProvisionalEnvironmentManager _provisionalEnvironmentManager;

        public ProvisionalEnvironmentLifecycleJob(IProvisionalEnvironmentManager provisionalEnvironmentManager, INotificationPublisher notificationPublisher, IJobExecutionNotificationSink statusSink) : base(notificationPublisher, statusSink)
        {
            _provisionalEnvironmentManager = provisionalEnvironmentManager ?? throw new ArgumentNullException(nameof(provisionalEnvironmentManager));
        }

        protected override async Task<InvokeResult> ExecuteInternalAsync(JobExecutionContext context, CancellationToken cancellationToken)
        {
            await NotifyUserAsync(context.ExecutionId, 0, "Expiring inactive provisional environments.", cancellationToken).ConfigureAwait(false);

            var asOfUtc = DateTime.UtcNow;
            var expireResult = await _provisionalEnvironmentManager.ExpireAsync(asOfUtc, BatchSize).ConfigureAwait(false);
            if (!expireResult.Successful) return InvokeResult.FromError(GetError(expireResult, "Provisional environment expiration failed."));

            await NotifyUserAsync(context.ExecutionId, 33, "Preparing retained provisional environments for purge.", cancellationToken).ConfigureAwait(false);

            var prepareResult = await _provisionalEnvironmentManager.PrepareForPurgeAsync(asOfUtc, BatchSize).ConfigureAwait(false);
            if (!prepareResult.Successful) return InvokeResult.FromError(GetError(prepareResult, "Provisional environment purge preparation failed."));

            await NotifyUserAsync(context.ExecutionId, 66, "Purging eligible provisional environments.", cancellationToken).ConfigureAwait(false);

            var purgeResult = await _provisionalEnvironmentManager.PurgeAsync(BatchSize).ConfigureAwait(false);
            if (!purgeResult.Successful) return InvokeResult.FromError(GetError(purgeResult, "Provisional environment purge failed."));

            var summary = $"Expired {expireResult.Result.UpdatedCount} environment(s), prepared {prepareResult.Result.UpdatedCount} for purge, purged {purgeResult.Result.DeletedCount}, and left {purgeResult.Result.BlockedCount} blocked.";
            await CompleteAsync(context.ExecutionId, summary, cancellationToken).ConfigureAwait(false);
            return InvokeResult.Success;
        }

        protected override Task OnFailedAsync(JobExecutionContext context, string errorMessage, CancellationToken cancellationToken)
        {
            return FailAsync(context.ExecutionId, errorMessage, cancellationToken);
        }

        private static string GetError<T>(InvokeResult<T> result, string fallback)
        {
            return result.Errors.FirstOrDefault()?.Message ?? fallback;
        }
    }
}
