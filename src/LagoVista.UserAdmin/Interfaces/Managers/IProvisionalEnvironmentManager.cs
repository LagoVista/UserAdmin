using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface IProvisionalEnvironmentManager
    {
        Task<InvokeResult<CreateProvisionalEnvironmentResponse>> CreateAsync(CreateProvisionalEnvironmentRequest request);
        Task<InvokeResult<RestoreProvisionalEnvironmentResponse>> RestoreAsync(RestoreProvisionalEnvironmentRequest request);
        Task<InvokeResult> RecordActivityAsync(string provisionalEnvironmentId);
        Task<InvokeResult> ClaimAsync(string provisionalEnvironmentId, string appUserId);
        Task<InvokeResult<IEnumerable<ProvisionalEnvironmentLifecycleSummary>>> GetByStateAsync(ProvisionalEnvironmentState state, DateTime? dueBeforeUtc = null, int take = 100);
        Task<InvokeResult<ProvisionalEnvironmentLifecycleBatchResult>> ExpireAsync(DateTime? asOfUtc = null, int take = 100);
        Task<InvokeResult<ProvisionalEnvironmentLifecycleBatchResult>> PrepareForPurgeAsync(DateTime? asOfUtc = null, int take = 100);
        Task<InvokeResult<ProvisionalEnvironmentLifecycleBatchResult>> PurgeAsync(int take = 100);
    }
}