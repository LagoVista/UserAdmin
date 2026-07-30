using LagoVista.Core.Validation;
using System;

namespace LagoVista.UserAdmin.Authentication.Flows
{
    public class AuthenticationFlowResult<TResult>
    {
        public AuthenticationFlowResult(string transitionKey, InvokeResult<TResult> publicResult)
        {
            if (String.IsNullOrWhiteSpace(transitionKey)) throw new ArgumentNullException(nameof(transitionKey));

            TransitionKey = transitionKey;
            PublicResult = publicResult ?? throw new ArgumentNullException(nameof(publicResult));
        }

        public string TransitionKey { get; }
        public InvokeResult<TResult> PublicResult { get; }
    }
}
