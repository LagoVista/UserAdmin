using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Authentication.Flows
{
    public interface ITotpTurnOffFlowHandler
    {
        Task<AuthenticationFlowResult> HandleAsync(TotpManagementFlowRequest request);
    }

    public interface ITotpRecoveryCodeRotationFlowHandler
    {
        Task<AuthenticationFlowResult<List<string>>> HandleAsync(TotpManagementFlowRequest request);
    }
}
