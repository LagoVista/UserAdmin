using Fido2NetLib;
using Fido2NetLib.Objects;
using LagoVista.AspNetCore.Identity.Utils;
using LagoVista.Core;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Managers.Passkeys;
using LagoVista.UserAdmin.Interfaces.Repos.Security.Passkeys;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Managers;
using LagoVista.UserAdmin.Models.Auth;
using LagoVista.UserAdmin.Models.DTOs;
using LagoVista.UserAdmin.Models.Security.Passkeys;
using LagoVista.UserAdmin.Models.Users;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
        private readonly IAuthenticationLogManager _authLogMgr;
        private readonly IUserRegistrationManager _userRegistrationManager;
      

        public AppUserPasskeyManager(IAppUserRepo appUserRepo, IUserRegistrationManager userRegistrationManager, IAuthenticationLogManager authLogMgr, IAppUserPasskeyCredentialRepo credentialRepo, IPasskeyChallengeStore challengeStore, IAppConfig appConfig, IAdminLogger logger, IFido2 fido2)
        {
            _appUserRepo = appUserRepo ?? throw new ArgumentNullException(nameof(appUserRepo));
            _authLogMgr = authLogMgr ?? throw new ArgumentNullException(nameof(authLogMgr));
            _credentialRepo = credentialRepo ?? throw new ArgumentNullException(nameof(credentialRepo));
            _challengeStore = challengeStore ?? throw new ArgumentNullException(nameof(challengeStore));
            _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fido2 = fido2 ?? throw new ArgumentNullException(nameof(fido2));
            _userRegistrationManager = userRegistrationManager ?? throw new ArgumentNullException(nameof(userRegistrationManager));
        }

        /* Existing user-bound flows (attach/use passkeys for a known user) */

        public async Task<InvokeResult<PasskeyBeginOptionsResponse>> BeginRegistrationOptionsAsync(string userId, string passkeyUrl, EntityHeader org, EntityHeader user)
        {
            if (String.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));

            await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyBeginRegistrationStart, user, org);

            var appUser = await _appUserRepo.FindByIdAsync(userId);
            if (appUser == null)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyBeginRegistrationFailed, user, org, errors: "user_not_found");
                return InvokeResult<PasskeyBeginOptionsResponse>.FromError("user_not_found");
            }

            if (String.IsNullOrEmpty(appUser.Email))
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyBeginRegistrationFailed, user, org, errors: "missing_email");
                return InvokeResult<PasskeyBeginOptionsResponse>.FromError("missing_email");
            }

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

            var storeResult = await _challengeStore.CreateAsync(new PasskeyChallengePacket() { Challenge = challenge, OptionsJson = JsonConvert.SerializeObject(options) });
            if (!storeResult.Successful)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyBeginRegistrationFailed, user, org, errors: "challenge_store_failed");
                return storeResult.ToInvokeResult<PasskeyBeginOptionsResponse>();
            }

            var payload = new PasskeyBeginOptionsResponse() { ChallengeId = storeResult.Result.Challenge.Id, Options = JToken.FromObject(options) };

            await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyBeginRegistrationSuccess, user, org, challengeId: payload.ChallengeId);

            return InvokeResult<PasskeyBeginOptionsResponse>.Create(payload);
        }

        public async Task<InvokeResult> CompleteRegistrationAsync(string userId, PasskeyRegistrationCompleteRequest payload, EntityHeader org, EntityHeader user)
        {
            if (String.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));

            var (rpId, origin) = GetRpIdAndOrigin();

            string challengeId = payload.ChallengeId;
            if (String.IsNullOrEmpty(challengeId))
            { 
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyCompleteRegistrationFailed, user, org, errors:"missing_challenge_id", assertionId: payload.Attestation.Id);
                return InvokeResult.FromError("missing_challenge_id");
            }

            await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyCompleteRegistrationStart, user, org, errors:"missing_challenge_id", challengeId: challengeId, assertionId: payload.Attestation.Id);
       
            var challengeResult = await _challengeStore.ConsumeAsync(challengeId);
            var validateChallenge = ValidateChallenge(challengeResult, PasskeyChallengePurpose.Register, rpId, origin);
            if (!validateChallenge.Successful) 
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyCompleteRegistrationFailed, user, org, errors:"validation_challenge_failed", challengeId: challengeId, assertionId: payload.Attestation.Id);
                return validateChallenge;
            }

            if (!String.Equals(challengeResult.Result.Challenge.UserId, userId, StringComparison.Ordinal)) 
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyCompleteRegistrationFailed, user, org, errors:"challenge_user_mismatch", challengeId: challengeId, assertionId: payload.Attestation.Id);
                return InvokeResult.FromError("challenge_user_mismatch");
            }

            var options = JsonConvert.DeserializeObject<CredentialCreateOptions>(challengeResult.Result.OptionsJson);
            var optionsUserId = System.Text.Encoding.UTF8.GetString(options.User.Id);
            if (!String.Equals(optionsUserId, userId, StringComparison.Ordinal))
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyCompleteRegistrationFailed, user, org, errors:"options_user_mismatch", challengeId: challengeId, assertionId: payload.Attestation.Id);
                return InvokeResult.FromError("options_user_mismatch");
            }

            var attestationResponse = WebAuthnWireMapper.ToAttestationRawResponse(payload.Attestation);

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
            if (!addResult.Successful)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyCompleteRegistrationFailed, user, org, challengeId: challengeId, credentialId: cred.CredentialId, assertionId: payload.Attestation.Id);
                return addResult;   
            }

            await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyCompleteRegistrationSuccess, user, org, challengeId: challengeId, credentialId: cred.CredentialId, assertionId: payload.Attestation.Id);
      
            return InvokeResult.SuccessRedirect(NormalizePasskeyUrl(challengeResult.Result.Challenge.PasskeyUrl));
        }

        public async Task<InvokeResult<PasskeyBeginOptionsResponse>> BeginAuthenticationOptionsAsync(string userId, bool isStepUp, string passkeyUrl, EntityHeader org, EntityHeader user)
        {
            if (String.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));

            await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyAuthenticationOptionsBeginStart, user, org);

            var (rpId, origin) = GetRpIdAndOrigin();
            var safeUrl = NormalizePasskeyUrl(passkeyUrl);

            await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeySetupBegin, user, org);

            var creds = await _credentialRepo.GetByUserAsync(userId, rpId);

            // IMPORTANT: store allow credential IDs as base64url strings (not descriptors) for Redis
            var allowIds = creds?.Select(c => c.CredentialId).Where(id => !String.IsNullOrEmpty(id)).ToArray() ?? Array.Empty<string>();

            if (allowIds.Length == 0)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyAuthenticationOptionsBeginFailed, user, org, errors: "no_passkeys_registered");
                return InvokeResult<PasskeyBeginOptionsResponse>.FromError("no_passkeys_registered");
            }

            // Fido2 options still require descriptors in-memory (fine)
            var allowDescriptors = allowIds.Select(id => new PublicKeyCredentialDescriptor(Base64UrlDecode(id))).ToList();

            var uv = isStepUp ? UserVerificationRequirement.Required : UserVerificationRequirement.Preferred;

            var options = _fido2.GetAssertionOptions(new GetAssertionOptionsParams()
            {
                AllowedCredentials = allowDescriptors,
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

                // NEW: persist primitives we need later
                AllowCredentialIds = allowIds,
                UserVerification = (int)uv,
                TimeoutMs = (int)options.Timeout,
            };

            // Optional: keep OptionsJson for debug/client replay only (do NOT deserialize it into AssertionOptions)
            var optionsWireJson = JsonConvert.SerializeObject(JToken.FromObject(options));

            var storeResult = await _challengeStore.CreateAsync(new PasskeyChallengePacket() { Challenge = challenge, OptionsJson = optionsWireJson });

            if (!storeResult.Successful)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyAuthenticationOptionsBeginFailed, user, org, errors: storeResult.ErrorMessage);
                return storeResult.ToInvokeResult<PasskeyBeginOptionsResponse>();
            }

            var payload = new PasskeyBeginOptionsResponse() { ChallengeId = storeResult.Result.Challenge.Id, Options = JToken.FromObject(options) };

            await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyAuthenticationOptionsBeginSent, user, org, challengeId: storeResult.Result.Challenge.Id);

            return InvokeResult<PasskeyBeginOptionsResponse>.Create(payload);
        }


        public async Task<InvokeResult> CompleteAuthenticationAsync(string userId, PasskeyAuthenticationCompleteRequest payload, bool isStepUp, EntityHeader org, EntityHeader user)
        {
            if (String.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));

            if (payload == null)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyCompleteAuthenticationFailed, user, org, errors: "missing_assertion");
                return InvokeResult.FromError("missing_assertion");
            }

            await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyCompleteAuthenticationStart, user, org, assertionId: payload.Assertion?.Id);

            var (rpId, origin) = GetRpIdAndOrigin();

            string challengeId = payload.ChallengeId;
            if (String.IsNullOrEmpty(challengeId))
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyCompleteAuthenticationFailed, user, org, errors: "missing_challenge_id", assertionId: payload.Assertion?.Id);
                return InvokeResult.FromError("missing_challenge_id");
            }

            var challengeResult = await _challengeStore.ConsumeAsync(challengeId);
            var validateChallenge = ValidateChallenge(challengeResult, PasskeyChallengePurpose.Authenticate, rpId, origin);
            if (!validateChallenge.Successful)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyCompleteAuthenticationFailed, user, org, errors: "validation_challenge_failed", challengeId: challengeId, assertionId: payload.Assertion?.Id);
                return validateChallenge;
            }

            if (!String.Equals(challengeResult.Result.Challenge.UserId, userId, StringComparison.Ordinal))
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyCompleteAuthenticationFailed, user, org, errors: "challenge_user_mismatch", challengeId: challengeId, assertionId: payload.Assertion?.Id);
                return InvokeResult.FromError("challenge_user_mismatch");
            }

            // DO NOT deserialize AssertionOptions from JSON (Fido2 types are not JSON round-trippable)
            var ch = challengeResult.Result.Challenge;
            var uv = ch.UserVerification.HasValue ? (UserVerificationRequirement)ch.UserVerification.Value : (isStepUp ? UserVerificationRequirement.Required : UserVerificationRequirement.Preferred);
            var timeout = (uint)(ch.TimeoutMs ?? 60000);

            var allowDescriptors = (ch.AllowCredentialIds ?? Array.Empty<string>())
                .Select(id => new PublicKeyCredentialDescriptor(Base64UrlDecode(id)))
                .ToList();

            var options = new AssertionOptions()
            {
                Challenge = Base64UrlDecode(ch.Challenge),
                Timeout = timeout,
                RpId = ch.RpId,
                AllowCredentials = allowDescriptors,
                UserVerification = uv,
                Extensions = new AuthenticationExtensionsClientInputs()
            };

            var assertionResponse = WebAuthnWireMapper.ToFido2Assertion(payload.Assertion);

            var credentialId = Base64UrlEncode(assertionResponse.RawId);
            var stored = await _credentialRepo.FindByCredentialIdAsync(rpId, credentialId);
            if (stored == null)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyCompleteAuthenticationFailed, user, org, errors: "credential_not_found", challengeId: challengeId, credentialId: credentialId, assertionId: payload.Assertion?.Id);
                return InvokeResult.FromError("credential_not_found");
            }

            if (!String.Equals(stored.UserId, userId, StringComparison.Ordinal))
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyCompleteAuthenticationFailed, user, org, errors: "credential_user_mismatch", challengeId: challengeId, credentialId: stored.CredentialId, assertionId: payload.Assertion?.Id);
                return InvokeResult.FromError("credential_user_mismatch");
            }

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
                IsUserHandleOwnerOfCredentialIdCallback = callback
            }, CancellationToken.None);

            var updateResult = await _credentialRepo.UpdateSignCountAsync(userId, rpId, stored.CredentialId, res.SignCount, DateTime.UtcNow.ToJSONString());
            if (!updateResult.Successful)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyCompleteAuthenticationFailed, user, org, errors: "update_signcount_failed", challengeId: challengeId, credentialId: stored.CredentialId, assertionId: payload.Assertion?.Id);
                return updateResult;
            }

            if (isStepUp)
            {
                var appUser = await _appUserRepo.FindByIdAsync(userId);
                if (appUser != null)
                {
                    appUser.LastMfaDateTimeUtc = DateTime.UtcNow.ToJSONString();
                    await _appUserRepo.UpdateAsync(appUser);
                }
            }

            await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyCompleteAuthenticationSuccess, user, org, challengeId: challengeId, credentialId: stored.CredentialId, assertionId: payload.Assertion?.Id);

            return InvokeResult.SuccessRedirect(NormalizePasskeyUrl(challengeResult.Result.Challenge.PasskeyUrl));
        }


        /* Passwordless (discoverable/resident) flows */

        public async Task<InvokeResult<PasskeyBeginOptionsResponse>> BeginPasswordlessRegistrationOptionsAsync(string passkeyUrl, EntityHeader org, EntityHeader user)
        {
            await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyBeginPasswordlessRegistrationStart, user, org);

            var (rpId, origin) = GetRpIdAndOrigin();
            var safeUrl = NormalizePasskeyUrl(passkeyUrl);


            var provisionalGuid = Guid.NewGuid();
            var provisionalUserId = provisionalGuid.ToId();

            var fidoUser = new Fido2User()
            {
                Id = System.Text.Encoding.UTF8.GetBytes(provisionalUserId),
                Name = provisionalUserId,
                DisplayName = provisionalUserId,
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
                UserId = provisionalUserId,
                RpId = rpId,
                Origin = origin,
                PasskeyUrl = safeUrl,
                Purpose = PasskeyChallengePurpose.Register,
                Challenge = Base64UrlEncode(options.Challenge),
                CreatedUtc = DateTime.UtcNow.ToJSONString(),
                ExpiresUtc = DateTime.UtcNow.AddMinutes(ChallengeTtlMinutes).ToJSONString(),
            };

            var provisionUser = EntityHeader.Create(provisionalUserId, provisionalUserId);

            var storeResult = await _challengeStore.CreateAsync(new PasskeyChallengePacket() { Challenge = challenge, OptionsJson = JsonConvert.SerializeObject(options) });
            if (!storeResult.Successful)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyBeginPasswordlessRegistrationFailed, provisionUser, org, errors: "challenge_store_failed");
                return storeResult.ToInvokeResult<PasskeyBeginOptionsResponse>();
            }

            var payload = new PasskeyBeginOptionsResponse() { ChallengeId = storeResult.Result.Challenge.Id, Options = JToken.FromObject(options) };

            await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyBeginPasswordlessRegistrationSuccess, provisionUser, org, challengeId: payload.ChallengeId);

            return InvokeResult<PasskeyBeginOptionsResponse>.Create(payload);
        }


        public async Task<InvokeResult<PasskeySignInResult>> CompletePasswordlessRegistrationAsync(PasskeyRegistrationCompleteRequest payload, EntityHeader org, EntityHeader user)
        {
            if (payload == null)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyCompletePasswordlessRegistrationFailed, user, org, errors: "missing_attestation");
                return InvokeResult<PasskeySignInResult>.FromError("missing_attestation");
            }

            await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyCompletePasswordlessRegistrationStart, user, org, assertionId: payload.Attestation?.Id);

            var (rpId, origin) = GetRpIdAndOrigin();

            string challengeId = payload.ChallengeId;
            if (String.IsNullOrEmpty(challengeId))
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyCompletePasswordlessRegistrationFailed, user, org, errors: "missing_challenge_id", assertionId: payload.Attestation?.Id);
                return InvokeResult<PasskeySignInResult>.FromError("missing_challenge_id");
            }

            var challengeResult = await _challengeStore.ConsumeAsync(challengeId);
            var validateChallenge = ValidateChallenge(challengeResult, PasskeyChallengePurpose.Register, rpId, origin);
            if (!validateChallenge.Successful)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyCompletePasswordlessRegistrationFailed, user, org, errors: "validation_challenge_failed", challengeId: challengeId, assertionId: payload.Attestation?.Id);
                return validateChallenge.ToInvokeResult<PasskeySignInResult>();
            }

            var userId = challengeResult.Result.Challenge.UserId;
            if (String.IsNullOrEmpty(userId))
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyCompletePasswordlessRegistrationFailed, user, org, errors: "missing_user_id", challengeId: challengeId, assertionId: payload.Attestation?.Id);
                return InvokeResult<PasskeySignInResult>.FromError("missing_user_id");
            }

            var options = JsonConvert.DeserializeObject<CredentialCreateOptions>(challengeResult.Result.OptionsJson);

            var attestationResponse = WebAuthnWireMapper.ToAttestationRawResponse(payload.Attestation);

            IsCredentialIdUniqueToUserAsyncDelegate isUnique = async (args, cancellationToken) =>
            {
                var credentialId = Base64UrlEncode(args.CredentialId);
                var existing = await _credentialRepo.FindByCredentialIdAsync(rpId, credentialId);
                return existing == null;
            };

            var makeResult = await _fido2.MakeNewCredentialAsync(new MakeNewCredentialParams() { AttestationResponse = attestationResponse, OriginalOptions = options, IsCredentialIdUniqueToUserCallback = isUnique }, CancellationToken.None);
           
            var registration = new RegisterUser()
            {
                AppId = "1844A92CDDDF4B59A3BB294A1524D93A", // The one, the only app id for NuvIoT.
                ClientType = "WEBAPP",
                DeviceId = "BROWSER",
                Source = UserCreationSource.Passkey,
                LoginType = LoginTypes.AppUser
            };

            var credentialid = Base64UrlEncode(makeResult.Id);

            var createUserResponse = await _userRegistrationManager.CreateUserAsync(registration, true);
            if(!createUserResponse.Successful) return createUserResponse.ToInvokeResult<PasskeySignInResult>();
            if (!createUserResponse.Successful)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyCompletePasswordlessRegistrationFailed, user, org, errors: $"credential_add_failedn {createUserResponse.ErrorMessage}", challengeId: challengeId, credentialId: credentialid, assertionId: payload.Attestation?.Id);
                return createUserResponse.ToInvokeResult<PasskeySignInResult>();
            }

            var cred = new PasskeyCredential()
            {
                UserId = createUserResponse.Result.AppUser.Id,
                RpId = rpId,
                CredentialId = credentialid,
                PublicKey = Base64UrlEncode(makeResult.PublicKey),
                SignCount = makeResult.SignCount,
                CreatedUtc = DateTime.UtcNow.ToJSONString(),
                LastUsedUtc = null,
                Name = null,
            };

            // creatweUserResponse.Result.AppUser

            var addResult = await _credentialRepo.AddAsync(cred);
            if (!addResult.Successful)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyCompletePasswordlessRegistrationFailed, user, org, errors: $"credential_add_failed {addResult.ErrorMessage}", challengeId: challengeId, credentialId: cred.CredentialId, assertionId: payload.Attestation?.Id);
                return addResult.ToInvokeResult<PasskeySignInResult>();
            }

            await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyCompletePasswordlessRegistrationSuccess, user, org, createUserResponse.Result.AppUser.Id, createUserResponse.Result.AppUser.UserName, challengeId: challengeId, credentialId: cred.CredentialId, assertionId: payload.Attestation?.Id);

            return InvokeResult<PasskeySignInResult>.Create(new PasskeySignInResult()
            {
                UserId = userId,
                RequiresOnboarding = true,
                RedirectUrl = CommonLinks.CompleteUserRegistration,
                Message = "Onboarding required.",
            });
        }

        public async Task<InvokeResult<PasskeyBeginOptionsResponse>> BeginPasswordlessAuthenticationOptionsAsync(string passkeyUrl, EntityHeader org, EntityHeader user)
        {
            await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyBeginPasswordlessAuthenticationStart, user, org);

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

            var storeResult = await _challengeStore.CreateAsync(new PasskeyChallengePacket() { Challenge = challenge, OptionsJson = JsonConvert.SerializeObject(options) });

            if (!storeResult.Successful)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyBeginPasswordlessAuthenticationFailed, user, org, errors: "challenge_store_failed");
                return storeResult.ToInvokeResult<PasskeyBeginOptionsResponse>();
            }

            var payload = new PasskeyBeginOptionsResponse() { ChallengeId = storeResult.Result.Challenge.Id, Options = JToken.FromObject(options) };

            await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyBeginPasswordlessAuthenticationSuccess, user, org, challengeId: payload.ChallengeId);

            return InvokeResult<PasskeyBeginOptionsResponse>.Create(payload);
        }

        public async Task<InvokeResult<PasskeySignInResult>> CompletePasswordlessAuthenticationAsync(PasskeyAuthenticationCompleteRequest payload, EntityHeader org, EntityHeader user)
        {
            if (payload == null)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyCompletePasswordlessAuthenticationFailed, user, org, errors: "missing_assertion");
                return InvokeResult<PasskeySignInResult>.FromError("missing_assertion");
            }

            await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyCompletePasswordlessAuthenticationStart, user, org, assertionId: payload.Assertion?.Id);

            var (rpId, origin) = GetRpIdAndOrigin();

            string challengeId = payload.ChallengeId;
            if (String.IsNullOrEmpty(challengeId))
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyCompletePasswordlessAuthenticationFailed, user, org, errors: "missing_challenge_id", assertionId: payload.Assertion?.Id);
                return InvokeResult<PasskeySignInResult>.FromError("missing_challenge_id");
            }

            var challengeResult = await _challengeStore.ConsumeAsync(challengeId);
            var validateChallenge = ValidateChallenge(challengeResult, PasskeyChallengePurpose.Authenticate, rpId, origin);
            if (!validateChallenge.Successful)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyCompletePasswordlessAuthenticationFailed, user, org, errors: "validation_challenge_failed", challengeId: challengeId, assertionId: payload.Assertion?.Id);
                return validateChallenge.ToInvokeResult<PasskeySignInResult>();
            }

            var assertionResponse = WebAuthnWireMapper.ToFido2Assertion(payload.Assertion);

            var options = JsonConvert.DeserializeObject<AssertionOptions>(challengeResult.Result.OptionsJson);

            var credentialId = Base64UrlEncode(assertionResponse.RawId);
            var stored = await _credentialRepo.FindByCredentialIdAsync(rpId, credentialId);
            if (stored == null)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyCompletePasswordlessAuthenticationFailed, user, org, errors: "credential_not_found", challengeId: challengeId, credentialId: credentialId, assertionId: payload.Assertion?.Id);
                return InvokeResult<PasskeySignInResult>.FromError("credential_not_found");
            }

            var userId = stored.UserId;
            if (String.IsNullOrEmpty(userId))
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyCompletePasswordlessAuthenticationFailed, user, org, errors: "missing_user_id", challengeId: challengeId, credentialId: stored.CredentialId, assertionId: payload.Assertion?.Id);
                return InvokeResult<PasskeySignInResult>.FromError("missing_user_id");
            }

            IsUserHandleOwnerOfCredentialIdAsync callback = async (args, cancellationToken) =>
            {
                if (args.UserHandle == null || args.UserHandle.Length == 0) return true;
                var handle = System.Text.Encoding.UTF8.GetString(args.UserHandle);
                return String.Equals(handle, userId, StringComparison.Ordinal);
            };

            var res = await _fido2.MakeAssertionAsync(new MakeAssertionParams() { AssertionResponse = assertionResponse, OriginalOptions = options, StoredPublicKey = Base64UrlDecode(stored.PublicKey), StoredSignatureCounter = stored.SignCount, IsUserHandleOwnerOfCredentialIdCallback = callback }, CancellationToken.None);

            var updateResult = await _credentialRepo.UpdateSignCountAsync(userId, rpId, stored.CredentialId, res.SignCount, DateTime.UtcNow.ToJSONString());
            if (!updateResult.Successful)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyCompletePasswordlessAuthenticationFailed, user, org, errors: "update_signcount_failed", challengeId: challengeId, credentialId: stored.CredentialId, assertionId: payload.Assertion?.Id);
                return updateResult.ToInvokeResult<PasskeySignInResult>();
            }

            var appUser = await _appUserRepo.FindByIdAsync(userId);
            var requiresOnboarding = appUser == null || !appUser.EmailConfirmed;

            await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.PasskeyCompletePasswordlessAuthenticationSuccess, user, org, challengeId: challengeId, credentialId: stored.CredentialId, assertionId: payload.Assertion?.Id);

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
