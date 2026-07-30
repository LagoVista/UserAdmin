using LagoVista.UserAdmin.Managers;
using LagoVista.UserAdmin.Models.Auth;
using NUnit.Framework;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public class AuthenticationResponseResolverTests
    {
        private AuthenticationResponseResolver _resolver;

        [SetUp]
        public void Initialize()
        {
            _resolver = new AuthenticationResponseResolver();
        }

        [Test]
        public void ValidResolvedUser_ReturnsAuthenticated()
        {
            var result = _resolver.Resolve(CreateResolvedContext());
            Assert.That(result.AuthenticationState, Is.EqualTo(AuthenticationResponseState.Authenticated));
            Assert.That(result.CanEnterApplication, Is.True);
        }

        [Test]
        public void InvalidCredential_ReturnsInvalidCredentials()
        {
            var context = CreateResolvedContext();
            context.CredentialValidated = false;

            var result = _resolver.Resolve(context);

            Assert.That(result.AuthenticationState, Is.EqualTo(AuthenticationResponseState.InvalidCredentials));
            Assert.That(result.CanEnterApplication, Is.False);
        }

        [Test]
        public void DisabledAccount_TakesPriorityOverLockedAccount()
        {
            var context = CreateResolvedContext();
            context.AccountDisabled = true;
            context.AccountLocked = true;

            var result = _resolver.Resolve(context);

            Assert.That(result.AuthenticationState, Is.EqualTo(AuthenticationResponseState.AccountDisabled));
        }

        [Test]
        public void LockedAccount_ReturnsAccountLocked()
        {
            var context = CreateResolvedContext();
            context.AccountLocked = true;

            var result = _resolver.Resolve(context);

            Assert.That(result.AuthenticationState, Is.EqualTo(AuthenticationResponseState.AccountLocked));
        }

        [Test]
        public void ExpiredPendingIdentity_ReturnsPendingIdentityExpired()
        {
            var context = CreatePendingContext();
            context.PendingIdentityExpired = true;

            var result = _resolver.Resolve(context);

            Assert.That(result.AuthenticationState, Is.EqualTo(AuthenticationResponseState.PendingIdentityExpired));
        }

        [Test]
        public void NewAuthenticationProofWithoutPendingIdentity_ReturnsRegistrationRequired()
        {
            var context = new AuthenticationResolutionContext
            {
                CredentialValidated = true,
                UserExists = false,
                HasPendingIdentity = false
            };

            var result = _resolver.Resolve(context);

            Assert.That(result.AuthenticationState, Is.EqualTo(AuthenticationResponseState.RegistrationRequired));
        }

        [Test]
        public void PendingIdentityWithoutVerifiedEmail_ReturnsEmailVerificationRequired()
        {
            var result = _resolver.Resolve(CreatePendingContext());
            Assert.That(result.AuthenticationState, Is.EqualTo(AuthenticationResponseState.EmailVerificationRequired));
        }

        [Test]
        public void VerifiedEmailMatchingExistingUser_ReturnsIdentityLinkRequired()
        {
            var context = CreatePendingContext();
            context.EmailVerified = true;
            context.VerifiedEmailMatchesExistingUser = true;

            var result = _resolver.Resolve(context);

            Assert.That(result.AuthenticationState, Is.EqualTo(AuthenticationResponseState.IdentityLinkRequired));
        }

        [Test]
        public void LinkedAuthBucketWithMfa_ReturnsMfaRequired()
        {
            var context = CreateResolvedContext();
            context.AuthBucketLinked = true;
            context.MfaRequired = true;

            var result = _resolver.Resolve(context);

            Assert.That(result.AuthenticationState, Is.EqualTo(AuthenticationResponseState.MfaRequired));
        }

        [Test]
        public void Resolve_CopiesSafeClientContext()
        {
            var context = CreatePendingContext();
            context.ReasonCode = "email-proof-required";
            context.PendingIdentityId = "pending-123";
            context.MaskedEmail = "k***@example.com";
            context.Provider = "google";
            context.InviteId = "invite-456";

            var result = _resolver.Resolve(context);

            Assert.That(result.AuthenticationReasonCode, Is.EqualTo("email-proof-required"));
            Assert.That(result.PendingIdentityId, Is.EqualTo("pending-123"));
            Assert.That(result.MaskedEmail, Is.EqualTo("k***@example.com"));
            Assert.That(result.Provider, Is.EqualTo("google"));
            Assert.That(result.InviteId, Is.EqualTo("invite-456"));
        }

        [Test]
        public void Apply_PreservesExistingResponsePayload()
        {
            var response = new AuthenticationResponse
            {
                RedirectPage = "/home",
                ResponseMessage = "Welcome"
            };

            var result = _resolver.Apply(response, CreateResolvedContext());

            Assert.That(result, Is.SameAs(response));
            Assert.That(result.RedirectPage, Is.EqualTo("/home"));
            Assert.That(result.ResponseMessage, Is.EqualTo("Welcome"));
            Assert.That(result.AuthenticationState, Is.EqualTo(AuthenticationResponseState.Authenticated));
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
