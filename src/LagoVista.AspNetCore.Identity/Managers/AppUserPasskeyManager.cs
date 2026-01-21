using Fido2NetLib;
using Fido2NetLib.Objects;
using LagoVista.Core;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers.Passkeys;
using LagoVista.UserAdmin.Interfaces.Repos.Security.Passkeys;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Security.Passkeys;
using LagoVista.UserAdmin.Models.Users;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Twilio.TwiML.Voice;

namespace LagoVista.AspNetCore.Identity.Managers
{
    public class AppUserPasskeyManager : IAppUserPasskeyManager
    {
        private const int ChallengeTtlMinutes = 5;
        private const int MaxPasskeyNameLength = 128;

        private readonly IAppUserRepo _appUserRepo;
        private readonly IAppUserPasskeyCredentialRepo _credentialRepo;
        private readonly IPasskeyChallengeStore _challengeStore;
        private readonly IAppConfig _appConfig;
        private readonly IAdminLogger _logger;
        private readonly IFido2 _fido2;

        public AppUserPasskeyManager(IAppUserRepo appUserRepo, IAppUserPasskeyCredentialRepo credentialRepo, IPasskeyChallengeStore challengeStore, IAppConfig appConfig, IAdminLogger logger, IFido2 fido2)
        {
            _appUserRepo = appUserRepo ?? throw new ArgumentNullException(nameof(appUserRepo));
            _credentialRepo = credentialRepo ?? throw new ArgumentNullException(nameof(credentialRepo));
            _challengeStore = challengeStore ?? throw new ArgumentNullException(nameof(challengeStore));
            _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fido2 = fido2 ?? throw new ArgumentNullException(nameof(fido2));
        }

        /* Existing user-bound flows (attach/use passkeys for a known user) */

        public async Task<InvokeResult<PasskeyBeginOptionsResponse>> BeginRegistrationOptionsAsync(string userId, string passkeyUrl, EntityHeader org, EntityHeader user)
        {
            if (String.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));

            var appUser = await _appUserRepo.FindByIdAsync(userId);
            if (appUser == null) return InvokeResult<PasskeyBeginOptionsResponse>.FromError("user_not_found");
            if (String.IsNullOrEmpty(appUser.Email)) return InvokeResult<PasskeyBeginOptionsResponse>.FromError("missing_email");

            var (rpId, origin) = GetRpIdAndOrigin();
            var safeUrl = NormalizePasskeyUrl(passkeyUrl);

            var existing = await _credentialRepo.GetByUserAsync(userId, rpId);
            var exclude = existing.Select(c => new PublicKeyCredentialDescriptor(Base64UrlDecode(c.CredentialId))).ToList();

            var fidoUser = new Fido2User()
            {
                DisplayName = appUser.Email,
                Name = appUser.Email,
                Id = System.Text.Encoding.UTF8.GetBytes(userId),
            };

            var options = _fido2.RequestNewCredential(new RequestNewCredentialParams()
            {
                User = fidoUser,
                ExcludeCredentials = exclude,
                AuthenticatorSelection = AuthenticatorSelection.Default,
                AttestationPreference = AttestationConveyancePreference.None,
                Extensions = new AuthenticationExtensionsClientInputs(),
            });

            var challenge = new PasskeyChallenge()
            {
                UserId = userId,
                RpId = rpId,
                Origin = origin,
                PasskeyUrl = safeUrl,
                Purpose = PasskeyChallengePurpose.Register,
                Challenge = Base64UrlEncode(options.Challenge),
                CreatedUtc = DateTime.UtcNow.ToJSONString(),
                ExpiresUtc = DateTime.UtcNow.AddMinutes(ChallengeTtlMinutes).ToJSONString(),
            };

            var storeResult = await _challengeStore.CreateAsync( new PasskeyChallengePacket() 
            { 
                Challenge = challenge, 
                OptionsJson = JsonConvert.SerializeObject(options) 
            });
            if (!storeResult.Successful) return storeResult.ToInvokeResult<PasskeyBeginOptionsResponse>();

            var payload = new PasskeyBeginOptionsResponse() { ChallengeId = storeResult.Result.Challenge.Id, Options = options };
            return InvokeResult<PasskeyBeginOptionsResponse>.Create(payload);
        }

        public async Task<InvokeResult> CompleteRegistrationAsync(string userId, PasskeyRegistrationCompleteRequest payload, EntityHeader org, EntityHeader user)
        {
            if (String.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));

            var (rpId, origin) = GetRpIdAndOrigin();

            string challengeId = payload.ChallengeId;
            if (String.IsNullOrEmpty(challengeId)) return InvokeResult.FromError("missing_challenge_id");

            var challengeResult = await _challengeStore.ConsumeAsync(challengeId);
            var validateChallenge = ValidateChallenge(challengeResult, PasskeyChallengePurpose.Register, rpId, origin);
            if (!validateChallenge.Successful) return validateChallenge;
            if (!String.Equals(challengeResult.Result.Challenge.UserId, userId, StringComparison.Ordinal)) return InvokeResult.FromError("challenge_user_mismatch");

            var options = JsonConvert.DeserializeObject<CredentialCreateOptions>(challengeResult.Result.OptionsJson);
            var optionsUserId = System.Text.Encoding.UTF8.GetString(options.User.Id);
            if (!String.Equals(optionsUserId, userId, StringComparison.Ordinal)) return InvokeResult.FromError("options_user_mismatch");
         
            var attestationResponse = JsonConvert.DeserializeObject<AuthenticatorAttestationRawResponse>(JsonConvert.SerializeObject(payload.AttestationJson));

            IsCredentialIdUniqueToUserAsyncDelegate isUnique = async (args, cancellationToken) =>
            {
                var credentialId = Base64UrlEncode(args.CredentialId);
                var existing = await _credentialRepo.FindByCredentialIdAsync(rpId, credentialId);
                return existing == null;
            };

            var makeResult = await _fido2.MakeNewCredentialAsync(new MakeNewCredentialParams()
            {
                AttestationResponse = attestationResponse,
                OriginalOptions = options,
                IsCredentialIdUniqueToUserCallback = isUnique,
            }, CancellationToken.None);

            var cred = new PasskeyCredential()
            {
                UserId = userId,
                RpId = rpId,
                CredentialId = Base64UrlEncode(makeResult.Id),
                PublicKey = Base64UrlEncode(makeResult.PublicKey),
                SignCount = makeResult.SignCount,
                CreatedUtc = DateTime.UtcNow.ToJSONString(),
                LastUsedUtc = null,
                Name = null,
            };

            var addResult = await _credentialRepo.AddAsync(cred);
            if (!addResult.Successful) return addResult;

            return InvokeResult.SuccessRedirect(NormalizePasskeyUrl(challengeResult.Result.Challenge.PasskeyUrl));
        }

        public async Task<InvokeResult<PasskeyBeginOptionsResponse>> BeginAuthenticationOptionsAsync(string userId, bool isStepUp, string passkeyUrl, EntityHeader org, EntityHeader user)
        {
            if (String.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));

            var (rpId, origin) = GetRpIdAndOrigin();
            var safeUrl = NormalizePasskeyUrl(passkeyUrl);

            var creds = await _credentialRepo.GetByUserAsync(userId, rpId);
            var allow = creds.Select(c => new PublicKeyCredentialDescriptor(Base64UrlDecode(c.CredentialId))).ToList();
            if (allow.Count == 0) return InvokeResult<PasskeyBeginOptionsResponse>.FromError("no_passkeys_registered");

            var uv = isStepUp ? UserVerificationRequirement.Required : UserVerificationRequirement.Preferred;
            var options = _fido2.GetAssertionOptions(new GetAssertionOptionsParams()
            {
                AllowedCredentials = allow,
                UserVerification = uv,
                Extensions = new AuthenticationExtensionsClientInputs(),
            });

            var challenge = new PasskeyChallenge()
            {
                UserId = userId,
                RpId = rpId,
                Origin = origin,
                PasskeyUrl = safeUrl,
                Purpose = PasskeyChallengePurpose.Authenticate,
                Challenge = Base64UrlEncode(options.Challenge),
                CreatedUtc = DateTime.UtcNow.ToJSONString(),
                ExpiresUtc = DateTime.UtcNow.AddMinutes(ChallengeTtlMinutes).ToJSONString(),
            };

            var storeResult = await _challengeStore.CreateAsync(new PasskeyChallengePacket() 
            { 
                Challenge = challenge, 
                OptionsJson = JsonConvert.SerializeObject(options) 
            });
            if (!storeResult.Successful) return storeResult.ToInvokeResult<PasskeyBeginOptionsResponse>();

            var payload = new PasskeyBeginOptionsResponse() { ChallengeId = storeResult.Result.Challenge.Id, Options = options };
            return InvokeResult<PasskeyBeginOptionsResponse>.Create(payload);
        }

        public async Task<InvokeResult> CompleteAuthenticationAsync(string userId, PasskeyAuthenticationCompleteRequest payload, bool isStepUp, EntityHeader org, EntityHeader user)
        {
            if (String.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
            if (payload == null || String.IsNullOrEmpty(payload.AssertionJson)) return InvokeResult.FromError("missing_assertion");

            var (rpId, origin) = GetRpIdAndOrigin();

            string challengeId = payload.ChallengeId;
            if (String.IsNullOrEmpty(challengeId)) return InvokeResult.FromError("missing_challenge_id");

            var challengeResult = await _challengeStore.ConsumeAsync(challengeId);
            var validateChallenge = ValidateChallenge(challengeResult, PasskeyChallengePurpose.Authenticate, rpId, origin);
            if (!validateChallenge.Successful) return validateChallenge;
            if (!String.Equals(challengeResult.Result.Challenge.UserId, userId, StringComparison.Ordinal)) return InvokeResult.FromError("challenge_user_mismatch");

            var options = JsonConvert.DeserializeObject<AssertionOptions>(challengeResult.Result.OptionsJson);
                
            var assertionResponse = JsonConvert.DeserializeObject<AuthenticatorAssertionRawResponse>(JsonConvert.SerializeObject(payload.AssertionJson));

            var credentialId = Base64UrlEncode(assertionResponse.Id);
            var stored = await _credentialRepo.FindByCredentialIdAsync(rpId, credentialId);
            if (stored == null) return InvokeResult.FromError("credential_not_found");
            if (!String.Equals(stored.UserId, userId, StringComparison.Ordinal)) return InvokeResult.FromError("credential_user_mismatch");

            IsUserHandleOwnerOfCredentialIdAsync callback = async (args, cancellationToken) =>
            {
                if (args.UserHandle == null || args.UserHandle.Length == 0) return true;
                var handle = System.Text.Encoding.UTF8.GetString(args.UserHandle);
                return String.Equals(handle, userId, StringComparison.Ordinal);
            };

            var res = await _fido2.MakeAssertionAsync(new MakeAssertionParams() { AssertionResponse = assertionResponse, OriginalOptions = options, StoredPublicKey = Base64UrlDecode(stored.PublicKey), StoredSignatureCounter = stored.SignCount, IsUserHandleOwnerOfCredentialIdCallback = callback }, CancellationToken.None);

            var updateResult = await _credentialRepo.UpdateSignCountAsync(userId, rpId, stored.CredentialId, res.SignCount, DateTime.UtcNow.ToJSONString());
            if (!updateResult.Successful) return updateResult;

            if (isStepUp)
            {
                var appUser = await _appUserRepo.FindByIdAsync(userId);
                if (appUser != null)
                {
                    appUser.LastMfaDateTimeUtc = DateTime.UtcNow.ToJSONString();
                    await _appUserRepo.UpdateAsync(appUser);
                }
            }

            return InvokeResult.SuccessRedirect(NormalizePasskeyUrl(challengeResult.Result.Challenge.PasskeyUrl));
        }

        /* Passwordless (discoverable/resident) flows */

        public async Task<InvokeResult<PasskeyBeginOptionsResponse>> BeginPasswordlessRegistrationOptionsAsync(string passkeyUrl, EntityHeader org, EntityHeader user)
        {
            var (rpId, origin) = GetRpIdAndOrigin();
            var safeUrl = NormalizePasskeyUrl(passkeyUrl);

            var provisional = new AppUser()
            {
                Id = Guid.NewGuid().ToId(),
                Key = Guid.NewGuid().ToId().ToLower(),
                Email = null,
                EmailConfirmed = false,
                FirstName = null,
                LastName = null,
            };

            await _appUserRepo.CreateAsync(provisional);

            var fidoUser = new Fido2User()
            {
                Id = System.Text.Encoding.UTF8.GetBytes(provisional.Id),
                Name = provisional.Id,
                DisplayName = provisional.Id,
            };

            var options = _fido2.RequestNewCredential(new RequestNewCredentialParams()
            {
                User = fidoUser,
                ExcludeCredentials = new List<PublicKeyCredentialDescriptor>(),
                AuthenticatorSelection = AuthenticatorSelection.Default,
                AttestationPreference = AttestationConveyancePreference.None,
                Extensions = new AuthenticationExtensionsClientInputs(),
            });

            var challenge = new PasskeyChallenge()
            {
                UserId = provisional.Id,
                RpId = rpId,
                Origin = origin,
                PasskeyUrl = safeUrl,
                Purpose = PasskeyChallengePurpose.Register,
                Challenge = Base64UrlEncode(options.Challenge),
                CreatedUtc = DateTime.UtcNow.ToJSONString(),
                ExpiresUtc = DateTime.UtcNow.AddMinutes(ChallengeTtlMinutes).ToJSONString(),
            };

            var storeResult = await _challengeStore.CreateAsync(new PasskeyChallengePacket() 
            { 
                Challenge = challenge, 
                OptionsJson = JsonConvert.SerializeObject(options) 
            });
            if (!storeResult.Successful) return storeResult.ToInvokeResult<PasskeyBeginOptionsResponse>();

            var payload = new PasskeyBeginOptionsResponse() { ChallengeId = storeResult.Result.Challenge.Id, Options = options };
            return InvokeResult<PasskeyBeginOptionsResponse>.Create(payload);
        }

        public async Task<InvokeResult<PasskeySignInResult>> CompletePasswordlessRegistrationAsync(PasskeyRegistrationCompleteRequest payload, EntityHeader org, EntityHeader user)
        {
            if (payload == null && String.IsNullOrEmpty(payload.AttestationJson)) return InvokeResult<PasskeySignInResult>.FromError("missing_attestation");

            var (rpId, origin) = GetRpIdAndOrigin();

            string challengeId = payload.ChallengeId;
            if (String.IsNullOrEmpty(challengeId)) return InvokeResult<PasskeySignInResult>.FromError("missing_challenge_id");

            var challengeResult = await _challengeStore.ConsumeAsync(challengeId);
            var validateChallenge = ValidateChallenge(challengeResult, PasskeyChallengePurpose.Register, rpId, origin);
            if (!validateChallenge.Successful) return validateChallenge.ToInvokeResult<PasskeySignInResult>();

            var userId = challengeResult.Result.Challenge.UserId;
            if (String.IsNullOrEmpty(userId)) return InvokeResult<PasskeySignInResult>.FromError("missing_user_id");

            var options = JsonConvert.DeserializeObject<CredentialCreateOptions>(challengeResult.Result.OptionsJson);
           
             var attestationResponse = JsonConvert.DeserializeObject<AuthenticatorAttestationRawResponse>(JsonConvert.SerializeObject(payload.AttestationJson));

            IsCredentialIdUniqueToUserAsyncDelegate isUnique = async (args, cancellationToken) =>
            {
                var credentialId = Base64UrlEncode(args.CredentialId);
                var existing = await _credentialRepo.FindByCredentialIdAsync(rpId, credentialId);
                return existing == null;
            };

            var makeResult = await _fido2.MakeNewCredentialAsync(new MakeNewCredentialParams()
            {
                AttestationResponse = attestationResponse,
                OriginalOptions = options,
                IsCredentialIdUniqueToUserCallback = isUnique,
            }, CancellationToken.None);

            var cred = new PasskeyCredential()
            {
                UserId = userId,
                RpId = rpId,
                CredentialId = Base64UrlEncode(makeResult.Id),
                PublicKey = Base64UrlEncode(makeResult.PublicKey),
                SignCount = makeResult.SignCount,
                CreatedUtc = DateTime.UtcNow.ToJSONString(),
                LastUsedUtc = null,
                Name = null,
            };

            var addResult = await _credentialRepo.AddAsync(cred);
            if (!addResult.Successful) return addResult.ToInvokeResult<PasskeySignInResult>();

            return InvokeResult<PasskeySignInResult>.Create(new PasskeySignInResult()
            {
                UserId = userId,
                RequiresOnboarding = true,
                RedirectUrl = "/auth/onboarding",
                Message = "Onboarding required.",
            });
        }

        public async Task<InvokeResult<PasskeyBeginOptionsResponse>> BeginPasswordlessAuthenticationOptionsAsync(string passkeyUrl, EntityHeader org, EntityHeader user)
        {
            var (rpId, origin) = GetRpIdAndOrigin();
            var safeUrl = NormalizePasskeyUrl(passkeyUrl);

            var options = _fido2.GetAssertionOptions(new GetAssertionOptionsParams()
            {
                AllowedCredentials = new List<PublicKeyCredentialDescriptor>(),
                UserVerification = UserVerificationRequirement.Preferred,
                Extensions = new AuthenticationExtensionsClientInputs(),
            });

            var challenge = new PasskeyChallenge()
            {
                UserId = null,
                RpId = rpId,
                Origin = origin,
                PasskeyUrl = safeUrl,
                Purpose = PasskeyChallengePurpose.Authenticate,
                Challenge = Base64UrlEncode(options.Challenge),
                CreatedUtc = DateTime.UtcNow.ToJSONString(),
                ExpiresUtc = DateTime.UtcNow.AddMinutes(ChallengeTtlMinutes).ToJSONString(),
            };
            
            var storeResult = await _challengeStore.CreateAsync(new PasskeyChallengePacket() 
            { 
                Challenge = challenge, 
                OptionsJson = JsonConvert.SerializeObject(options) 
            });
          
            if (!storeResult.Successful) return storeResult.ToInvokeResult<PasskeyBeginOptionsResponse>();

            var payload = new PasskeyBeginOptionsResponse() { ChallengeId = storeResult.Result.Challenge.Id,  Options = options };
            return InvokeResult<PasskeyBeginOptionsResponse>.Create(payload);
        }

        public async Task<InvokeResult<PasskeySignInResult>> CompletePasswordlessAuthenticationAsync(PasskeyAuthenticationCompleteRequest payload, EntityHeader org, EntityHeader user)
        {
            if (payload == null || String.IsNullOrEmpty(payload.AssertionJson)) return InvokeResult<PasskeySignInResult>.FromError("missing_assertion");

            var (rpId, origin) = GetRpIdAndOrigin();

            string challengeId = payload.ChallengeId;
            if (String.IsNullOrEmpty(challengeId)) return InvokeResult<PasskeySignInResult>.FromError("missing_challenge_id");

            var challengeResult = await _challengeStore.ConsumeAsync(challengeId);
            var validateChallenge = ValidateChallenge(challengeResult, PasskeyChallengePurpose.Authenticate, rpId, origin);
            if (!validateChallenge.Successful) return validateChallenge.ToInvokeResult<PasskeySignInResult>();
    
            var options = JsonConvert.DeserializeObject<AssertionOptions>(challengeResult.Result.OptionsJson);
            
            var assertionResponse = JsonConvert.DeserializeObject<AuthenticatorAssertionRawResponse>(JsonConvert.SerializeObject(payload.AssertionJson));

            var credentialId = Base64UrlEncode(assertionResponse.Id);
            var stored = await _credentialRepo.FindByCredentialIdAsync(rpId, credentialId);
            if (stored == null) return InvokeResult<PasskeySignInResult>.FromError("credential_not_found");

            var userId = stored.UserId;
            if (String.IsNullOrEmpty(userId)) return InvokeResult<PasskeySignInResult>.FromError("missing_user_id");

            IsUserHandleOwnerOfCredentialIdAsync callback = async (args, cancellationToken) =>
            {
                if (args.UserHandle == null || args.UserHandle.Length == 0) return true;
                var handle = System.Text.Encoding.UTF8.GetString(args.UserHandle);
                return String.Equals(handle, userId, StringComparison.Ordinal);
            };

            var res = await _fido2.MakeAssertionAsync(new MakeAssertionParams()
            {
                AssertionResponse = assertionResponse,
                OriginalOptions = options,
                StoredPublicKey = Base64UrlDecode(stored.PublicKey),
                StoredSignatureCounter = stored.SignCount,
                IsUserHandleOwnerOfCredentialIdCallback = callback,
            }, CancellationToken.None);

            var updateResult = await _credentialRepo.UpdateSignCountAsync(userId, rpId, stored.CredentialId, res.SignCount, DateTime.UtcNow.ToJSONString());
            if (!updateResult.Successful) return updateResult.ToInvokeResult<PasskeySignInResult>();

            var appUser = await _appUserRepo.FindByIdAsync(userId);
            var requiresOnboarding = appUser == null || !appUser.EmailConfirmed;

            return InvokeResult<PasskeySignInResult>.Create(new PasskeySignInResult()
            {
                UserId = userId,
                RequiresOnboarding = requiresOnboarding,
                RedirectUrl = requiresOnboarding ? "/auth/onboarding" : NormalizePasskeyUrl(challengeResult.Result.Challenge.PasskeyUrl),
                Message = requiresOnboarding ? "Onboarding required." : null,
            });
        }

        public async Task<InvokeResult<PasskeyCredentialSummary[]>> ListPasskeysAsync(string userId, EntityHeader org, EntityHeader user)
        {
            if (String.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));

            var (rpId, _) = GetRpIdAndOrigin();
            var creds = await _credentialRepo.GetByUserAsync(userId, rpId);
            var summaries = creds.Select(c => new PasskeyCredentialSummary()
            {
                CredentialId = c.CredentialId,
                Name = c.Name,
                CreatedUtc = c.CreatedUtc,
                LastUsedUtc = c.LastUsedUtc,
            }).ToArray();

            return InvokeResult<PasskeyCredentialSummary[]>.Create(summaries);
        }

        public async Task<InvokeResult> RenamePasskeyAsync(string userId, string credentialId, string name, EntityHeader org, EntityHeader user)
        {
            if (String.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
            if (String.IsNullOrEmpty(credentialId)) throw new ArgumentNullException(nameof(credentialId));
            if (String.IsNullOrEmpty(name)) return InvokeResult.FromError("missing_name");
            if (name.Length > MaxPasskeyNameLength) return InvokeResult.FromError("name_too_long");

            var (rpId, _) = GetRpIdAndOrigin();
            var stored = await _credentialRepo.FindByCredentialIdAsync(rpId, credentialId);
            if (stored == null) return InvokeResult.FromError("credential_not_found");
            if (!String.Equals(stored.UserId, userId, StringComparison.Ordinal)) return InvokeResult.FromError("credential_user_mismatch");

            return await _credentialRepo.UpdateNameAsync(userId, rpId, credentialId, name);
        }

        public async Task<InvokeResult> RemovePasskeyAsync(string userId, string credentialId, EntityHeader org, EntityHeader user)
        {
            if (String.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
            if (String.IsNullOrEmpty(credentialId)) throw new ArgumentNullException(nameof(credentialId));

            var (rpId, _) = GetRpIdAndOrigin();
            return await _credentialRepo.RemovePasskeyCredentialAsync(userId, rpId, credentialId);
        }

        private InvokeResult ValidateChallenge(InvokeResult<PasskeyChallengePacket> challengeResult, PasskeyChallengePurpose purpose, string rpId, string origin)
        {
            if (!challengeResult.Successful) return challengeResult.ToInvokeResult();
            if (challengeResult.Result == null) return InvokeResult.FromError("challenge_not_found");
            if (challengeResult.Result.Challenge.IsExpired) return InvokeResult.FromError("challenge_expired");
            if (challengeResult.Result.Challenge.Purpose != purpose) return InvokeResult.FromError("invalid_challenge_purpose");
            if (!String.Equals(challengeResult.Result.Challenge.RpId, rpId, StringComparison.Ordinal)) return InvokeResult.FromError("challenge_rpid_mismatch");
            if (!String.Equals(challengeResult.Result.Challenge.Origin, origin, StringComparison.Ordinal)) return InvokeResult.FromError("challenge_origin_mismatch");
            return InvokeResult.Success;
        }

        private (string rpId, string origin) GetRpIdAndOrigin()
        {
            if (String.IsNullOrEmpty(_appConfig.WebAddress)) throw new InvalidOperationException("WebAddress not configured");
            var uri = new Uri(_appConfig.WebAddress);
            var origin = $"{uri.Scheme}://{uri.Host}{(uri.IsDefaultPort ? String.Empty : ":" + uri.Port)}";
            return (uri.Host, origin);
        }

        private static string NormalizePasskeyUrl(string passkeyUrl)
        {
            if (String.IsNullOrEmpty(passkeyUrl)) return "/auth";
            if (!passkeyUrl.StartsWith("/auth", StringComparison.Ordinal)) return "/auth";
            if (Uri.TryCreate(passkeyUrl, UriKind.Absolute, out _)) return "/auth";
            return passkeyUrl;
        }

        private static string Base64UrlEncode(byte[] bytes)
        {
            var s = Convert.ToBase64String(bytes);
            s = s.TrimEnd('=');
            s = s.Replace('+', '-');
            s = s.Replace('/', '_');
            return s;
        }

        private static byte[] Base64UrlDecode(string base64Url)
        {
            if (String.IsNullOrEmpty(base64Url)) return Array.Empty<byte>();
            var s = base64Url.Replace('-', '+').Replace('_', '/');
            switch (s.Length % 4)
            {
                case 2: s += "=="; break;
                case 3: s += "="; break;
            }
            return Convert.FromBase64String(s);
        }
    }
}
