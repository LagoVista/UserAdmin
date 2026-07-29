using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Models.Auth;
using System;

namespace LagoVista.UserAdmin.Managers
{
    public class AuthenticationResponseResolver : IAuthenticationResponseResolver
    {
        public AuthenticationResponseState ResolveState(AuthenticationResolutionContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (!context.CredentialValidated)
                return AuthenticationResponseState.InvalidCredentials;

            if (context.AccountDisabled)
                return AuthenticationResponseState.AccountDisabled;

            if (context.AccountLocked)
                return AuthenticationResponseState.AccountLocked;

            if (context.PendingIdentityExpired)
                return AuthenticationResponseState.PendingIdentityExpired;

            if (context.AuthBucketLinked)
                return context.MfaRequired ? AuthenticationResponseState.MfaRequired : AuthenticationResponseState.Authenticated;

            if (!context.UserExists && !context.HasPendingIdentity)
                return AuthenticationResponseState.RegistrationRequired;

            if (context.HasPendingIdentity && !context.EmailVerified)
                return AuthenticationResponseState.EmailVerificationRequired;

            if (context.EmailVerified && context.VerifiedEmailMatchesExistingUser)
                return AuthenticationResponseState.IdentityLinkRequired;

            if (!context.ProfileComplete || !context.DurableUserResolved)
                return AuthenticationResponseState.RegistrationRequired;

            if (context.MfaRequired)
                return AuthenticationResponseState.MfaRequired;

            return AuthenticationResponseState.Authenticated;
        }

        public AuthenticationResponse Resolve(AuthenticationResolutionContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return Apply(new AuthenticationResponse(), context);
        }

        public AuthenticationResponse Apply(AuthenticationResponse response, AuthenticationResolutionContext context)
        {
            if (response is null) throw new ArgumentNullException(nameof(response));
            if (context == null) throw new ArgumentNullException(nameof(context));

            response.AuthenticationState = ResolveState(context);
            response.AuthenticationReasonCode = context.ReasonCode;
            response.PendingIdentityId = context.PendingIdentityId;
            response.MaskedEmail = context.MaskedEmail;
            response.Provider = context.Provider;
            response.InviteId = context.InviteId;

            return response;
        }
    }
}
