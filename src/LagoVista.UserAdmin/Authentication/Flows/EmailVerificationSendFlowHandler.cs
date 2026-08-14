using LagoVista.Core.Interfaces;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Auth;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Authentication.Flows
{
    [CriticalCoverage]
    public class EmailVerificationSendFlowHandler : IAuthenticationFlowHandler<EmailVerificationSendFlowRequest, EmailVerificationSendResult>
    {
        public const string SentTransitionKey = "auth.transition.email-verification.code-sent";
        public const string ResentTransitionKey = "auth.transition.email-verification.code-resent";
        public const string ThrottledTransitionKey = "auth.transition.email-verification.resend-throttled";
        private const string DeliveryFailedTransitionKey = "auth.transition.email-verification.send-failed";
        private const int ResendCooldownSeconds = 60;

        private readonly IUserVerficationManager _userVerificationManager;
        private readonly IEmailVerificationCodeRepo _emailVerificationCodeRepo;

        public EmailVerificationSendFlowHandler(IUserVerficationManager userVerificationManager, IEmailVerificationCodeRepo emailVerificationCodeRepo)
        {
            _userVerificationManager = userVerificationManager ?? throw new ArgumentNullException(nameof(userVerificationManager));
            _emailVerificationCodeRepo = emailVerificationCodeRepo ?? throw new ArgumentNullException(nameof(emailVerificationCodeRepo));
        }

        public async Task<AuthenticationFlowResult<EmailVerificationSendResult>> HandleAsync(EmailVerificationSendFlowRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var latest = await _emailVerificationCodeRepo.GetLatestAsync(request.User.Id);
            if (latest != null)
            {
                var elapsed = DateTime.UtcNow - latest.CreatedUtc;
                if (elapsed.TotalSeconds < ResendCooldownSeconds)
                {
                    var retryAfterSeconds = Math.Max(1, (int)Math.Ceiling(ResendCooldownSeconds - elapsed.TotalSeconds));
                    return new AuthenticationFlowResult<EmailVerificationSendResult>(
                        ThrottledTransitionKey,
                        InvokeResult<EmailVerificationSendResult>.Create(new EmailVerificationSendResult
                        {
                            Outcome = EmailVerificationSendOutcome.Throttled,
                            RetryAfterSeconds = retryAfterSeconds
                        }));
                }
            }

            var sendResult = await _userVerificationManager.SendConfirmationEmailAsync(request.User.Id);
            if (!sendResult.Successful)
                return new AuthenticationFlowResult<EmailVerificationSendResult>(DeliveryFailedTransitionKey, InvokeResult<EmailVerificationSendResult>.FromInvokeResult(sendResult));

            var isResend = latest != null;
            return new AuthenticationFlowResult<EmailVerificationSendResult>(
                isResend ? ResentTransitionKey : SentTransitionKey,
                InvokeResult<EmailVerificationSendResult>.Create(new EmailVerificationSendResult
                {
                    Outcome = isResend ? EmailVerificationSendOutcome.Resent : EmailVerificationSendOutcome.Sent,
                    VerificationCode = sendResult.Result,
                    RetryAfterSeconds = 0
                }));
        }
    }
}
