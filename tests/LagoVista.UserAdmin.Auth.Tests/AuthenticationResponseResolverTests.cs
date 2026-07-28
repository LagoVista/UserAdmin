using LagoVista.UserAdmin.Managers;
using LagoVista.UserAdmin.Models.Auth;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestClass]
    public class AuthenticationResponseResolverTests
    {
        private AuthenticationResponseResolver _resolver;

        [TestInitialize]
        public void Initialize()
        {
            _resolver = new AuthenticationResponseResolver();
        }

        [TestMethod]
        public void ValidResolvedUser_ReturnsAuthenticated()
        {
            var result = _resolver.Resolve(CreateResolvedContext());

            Assert.AreEqual(AuthenticationResponseState.Authenticated, result.AuthenticationState);
            Assert.IsTrue(result.CanEnterApplication);
        }

        [TestMethod]
        public void InvalidCredential_ReturnsInvalidCredentials()
        {
            var context = CreateResolvedContext();
            context.CredentialValidated = false;

            var result = _resolver.Resolve(context);

            Assert.AreEqual(AuthenticationResponseState.InvalidCredentials, result.AuthenticationState);
            Assert.IsFalse(result.CanEnterApplication);
        }

        [TestMethod]
        public void DisabledAccount_TakesPriorityOverLockedAccount()
        {
            var context = CreateResolvedContext();
            context.AccountDisabled = true;
            context.AccountLocked = true;

            var result = _resolver.Resolve(context);

            Assert.AreEqual(AuthenticationResponseState.AccountDisabled, result.AuthenticationState);
        }

        [TestMethod]
        public void LockedAccount_ReturnsAccountLocked()
        {
            var context = CreateResolvedContext();
            context.AccountLocked = true;

            var result = _resolver.Resolve(context);

            Assert.AreEqual(AuthenticationResponseState.AccountLocked, result.AuthenticationState);
        }

        [TestMethod]
        public void ExpiredPendingIdentity_ReturnsPendingIdentityExpired()
        {
            var context = CreatePendingContext();
            context.PendingIdentityExpired = true;

            var result = _resolver.Resolve(context);

            Assert.AreEqual(AuthenticationResponseState.PendingIdentityExpired, result.AuthenticationState);
        }

        [TestMethod]
        public void NewAuthenticationProofWithoutPendingIdentity_ReturnsRegistrationRequired()
        {
            var context = new AuthenticationResolutionContext
            {
                CredentialValidated = true,
                UserExists = false,
                HasPendingIdentity = false
            };

            var result = _resolver.Resolve(context);

            Assert.AreEqual(AuthenticationResponseState.RegistrationRequired, result.AuthenticationState);
        }

        [TestMethod]
        public void PendingIdentityWithoutVerifiedEmail_ReturnsEmailVerificationRequired()
        {
            var result = _resolver.Resolve(CreatePendingContext());

            Assert.AreEqual(AuthenticationResponseState.EmailVerificationRequired, result.AuthenticationState);
        }

        [TestMethod]
        public void VerifiedEmailMatchingExistingUser_ReturnsIdentityLinkRequired()
        {
            var context = CreatePendingContext();
            context.EmailVerified = true;
            context.VerifiedEmailMatchesExistingUser = true;

            var result = _resolver.Resolve(context);

            Assert.AreEqual(AuthenticationResponseState.IdentityLinkRequired, result.AuthenticationState);
        }

        [TestMethod]
        public void LinkedAuthBucketWithMfa_ReturnsMfaRequired()
        {
            var context = CreateResolvedContext();
            context.AuthBucketLinked = true;
            context.MfaRequired = true;

            var result = _resolver.Resolve(context);

            Assert.AreEqual(AuthenticationResponseState.MfaRequired, result.AuthenticationState);
        }

        [TestMethod]
        public void Resolve_CopiesSafeClientContext()
        {
            var context = CreatePendingContext();
            context.ReasonCode = "email-proof-required";
            context.PendingIdentityId = "pending-123";
            context.MaskedEmail = "k***@example.com";
            context.Provider = "google";
            context.InviteId = "invite-456";

            var result = _resolver.Resolve(context);

            Assert.AreEqual("email-proof-required", result.AuthenticationReasonCode);
            Assert.AreEqual("pending-123", result.PendingIdentityId);
            Assert.AreEqual("k***@example.com", result.MaskedEmail);
            Assert.AreEqual("google", result.Provider);
            Assert.AreEqual("invite-456", result.InviteId);
        }

        [TestMethod]
        public void Apply_PreservesExistingResponsePayload()
        {
            var response = new UserLoginResponse
            {
                RedirectPage = "/home",
                ResponseMessage = "Welcome"
            };

            var result = _resolver.Apply(response, CreateResolvedContext());

            Assert.AreSame(response, result);
            Assert.AreEqual("/home", result.RedirectPage);
            Assert.AreEqual("Welcome", result.ResponseMessage);
            Assert.AreEqual(AuthenticationResponseState.Authenticated, result.AuthenticationState);
        }

        private static AuthenticationResolutionContext CreateResolvedContext()
        {
            return new AuthenticationResolutionContext
            {
                CredentialValidated = true,
                UserExists = true,
                AuthBucketLinked = true,
                EmailVerified = true,
                ProfileComplete = true,
                DurableUserResolved = true
            };
        }

        private static AuthenticationResolutionContext CreatePendingContext()
        {
            return new AuthenticationResolutionContext
            {
                CredentialValidated = true,
                UserExists = false,
                HasPendingIdentity = true,
                EmailVerified = false,
                ProfileComplete = false,
                DurableUserResolved = false
            };
        }
    }
}
