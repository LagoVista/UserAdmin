using LagoVista.Core.Interfaces;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Models.Auth;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Authentication.Flows
{
    public interface IPasswordLoginFlowHandler
    {
        Task<InvokeResult<AuthenticationResponse>> HandleAsync(AuthLoginRequest request);
    }

    [CriticalCoverage]
    public class PasswordLoginFlowHandler : IPasswordLoginFlowHandler
    {
        private readonly ISignInManager _signInManager;

        public PasswordLoginFlowHandler(ISignInManager signInManager)
        {
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        }

        public Task<InvokeResult<AuthenticationResponse>> HandleAsync(AuthLoginRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            return _signInManager.PasswordSignInAsync(request);
        }
    }
}
