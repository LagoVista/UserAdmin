using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Users;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface IAppUserMfaManager
    {
        // Enrollment
        Task<InvokeResult<AppUserTotpEnrollmentInfo>> BeginTotpEnrollmentAsync(string userId, EntityHeader org, EntityHeader user);
        Task<InvokeResult<List<string>>> ConfirmTotpEnrollmentAsync(string userId, string totp, EntityHeader org, EntityHeader user);

        // Verification (login or step-up)
        Task<InvokeResult> VerifyTotpAsync(string userId, string totp, bool isStepUp, EntityHeader org, EntityHeader user);

        // Recovery codes
        Task<InvokeResult<List<string>>> RotateRecoveryCodesAsync(string userId, EntityHeader org, EntityHeader user);
        Task<InvokeResult> ConsumeRecoveryCodeAsync(string userId, string recoveryCode, bool isStepUp, EntityHeader org, EntityHeader user);

        // Reset/disable
        Task<InvokeResult> DisableMfaAsync(string userId, EntityHeader org, EntityHeader user);
        Task<InvokeResult> ResetMfaAsync(string userId, EntityHeader org, EntityHeader user);
    }
}
