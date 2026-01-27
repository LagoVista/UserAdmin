using LagoVista.Core;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Users;
using OtpNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    public class AppUserMfaManager : ManagerBase, IAppUserMfaManager
    {
        private const int RecoveryCodeCount = 10;
        private const int TotpDigits = 6;
        private const int TotpStepSeconds = 30;
        private const int Pbkdf2Iterations = 100000;
        private const string RecoveryCodeLineVersion = "v1";
        private const string RecoveryCodeAlgorithm = "pbkdf2-sha256";
        private const string Issuer = "softwarelogistics";

        private readonly IAppUserRepo _appUserRepo;
        private readonly ISecureStorage _secureStorage;
        private readonly IAdminLogger _logger;

        private readonly IAuthenticationLogManager _authLogMgr;

        public AppUserMfaManager(IAppUserRepo appUserRepo, ISecureStorage secureStorage,IAuthenticationLogManager authLogMgr,  IAdminLogger logger, IAppConfig appConfig, IDependencyManager depManager, ISecurity security) : base(logger, appConfig, depManager, security)
        {
            _appUserRepo = appUserRepo ?? throw new ArgumentNullException(nameof(appUserRepo));
            _secureStorage = secureStorage ?? throw new ArgumentNullException(nameof(secureStorage));
            _authLogMgr = authLogMgr ?? throw new ArgumentNullException(nameof(authLogMgr));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<InvokeResult<AppUserTotpEnrollmentInfo>> BeginTotpEnrollmentAsync(string userId, EntityHeader org, EntityHeader user)
        {
            if (String.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));

            await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpBeginEnrollmentStart, user, org);

            var appUser = await _appUserRepo.FindByIdAsync(userId);
            if (appUser == null)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpBeginEnrollmentFailed, user, org, errors: "user_not_found");
                return InvokeResult<AppUserTotpEnrollmentInfo>.FromError("user_not_found");
            }

            if (String.IsNullOrEmpty(appUser.Email))
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpBeginEnrollmentFailed, user, org, errors: "missing_email");
                return InvokeResult<AppUserTotpEnrollmentInfo>.FromError("missing_email");
            }

            if (!String.IsNullOrEmpty(appUser.AuthenticatorKeySecretId))
            {
                var removeResult = await _secureStorage.RemoveUserSecretAsync(appUser.ToEntityHeader(), appUser.AuthenticatorKeySecretId);
                if (!removeResult.Successful)
                {
                    await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpBeginEnrollmentFailed, user, org, errors: "remove_existing_secret_failed");
                    return removeResult.ToInvokeResult<AppUserTotpEnrollmentInfo>();
                }
            }

            var secretBytes = KeyGeneration.GenerateRandomKey(20);
            var secretBase32 = Base32Encoding.ToString(secretBytes);

            var addSecretResult = await _secureStorage.AddUserSecretAsync(appUser.ToEntityHeader(), secretBase32);
            if (!addSecretResult.Successful)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpBeginEnrollmentFailed, user, org, errors: "add_secret_failed");
                return addSecretResult.ToInvokeResult<AppUserTotpEnrollmentInfo>();
            }

            appUser.AuthenticatorKeySecretId = addSecretResult.Result;
            appUser.AuthenticatorKey = null;
            appUser.TwoFactorEnabled = false;

            await _appUserRepo.UpdateAsync(appUser);

            var label = Uri.EscapeDataString(appUser.Email);
            var issuer = Uri.EscapeDataString(Issuer);
            var otpAuthUri = $"otpauth://totp/{issuer}:{label}?secret={secretBase32}&issuer={issuer}&digits={TotpDigits}&period={TotpStepSeconds}";

            await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpBeginEnrollmentSuccess, user, org);

            return InvokeResult<AppUserTotpEnrollmentInfo>.Create(new AppUserTotpEnrollmentInfo() { Secret = secretBase32, OtpAuthUri = otpAuthUri });
        }


        public async Task<InvokeResult<List<string>>> ConfirmTotpEnrollmentAsync(string userId, string totp, EntityHeader org, EntityHeader user)
        {
            if (String.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));

            await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpConfirmEnrollmentStart, user, org);

            if (String.IsNullOrEmpty(totp))
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpConfirmEnrollmentFailed, user, org, errors: "missing_totp");
                return InvokeResult<List<string>>.FromError("missing_totp");
            }

            var appUser = await _appUserRepo.FindByIdAsync(userId);
            if (appUser == null)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpConfirmEnrollmentFailed, user, org, errors: "user_not_found");
                return InvokeResult<List<string>>.FromError("user_not_found");
            }

            if (String.IsNullOrEmpty(appUser.AuthenticatorKeySecretId))
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpConfirmEnrollmentFailed, user, org, errors: "mfa_not_started");
                return InvokeResult<List<string>>.FromError("mfa_not_started");
            }

            var secretResult = await _secureStorage.GetUserSecretAsync(appUser.ToEntityHeader(), appUser.AuthenticatorKeySecretId);
            if (!secretResult.Successful)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpConfirmEnrollmentFailed, user, org, errors: "get_secret_failed");
                return secretResult.ToInvokeResult<List<string>>();
            }

            if (String.IsNullOrEmpty(secretResult.Result))
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpConfirmEnrollmentFailed, user, org, errors: "mfa_secret_missing");
                return InvokeResult<List<string>>.FromError("mfa_secret_missing");
            }

            var matchResult = TryMatchTotpTimeStep(secretResult.Result, totp);
            if (!matchResult.Successful)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpConfirmEnrollmentFailed, user, org, errors: "totp_mismatch");
                return matchResult.ToInvokeResult<List<string>>();
            }

            var nowUtc = DateTime.UtcNow.ToJSONString();
            var acceptResult = await _appUserRepo.TryAcceptTotpTimeStepAsync(userId, matchResult.Result, true, nowUtc);
            if (!acceptResult.Successful)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpConfirmEnrollmentFailed, user, org, errors: "accept_timestep_failed");
                return acceptResult.ToInvokeResult<List<string>>();
            }

            var codes = GenerateRecoveryCodes();
            var blob = BuildRecoveryCodeBlob(codes);

            if (!String.IsNullOrEmpty(appUser.RecoveryCodesSecretId))
            {
                var removeResult = await _secureStorage.RemoveUserSecretAsync(appUser.ToEntityHeader(), appUser.RecoveryCodesSecretId);
                if (!removeResult.Successful)
                {
                    await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpConfirmEnrollmentFailed, user, org, errors: "remove_existing_recoverycodes_failed");
                    return removeResult.ToInvokeResult<List<string>>();
                }
            }

            var addRecoveryResult = await _secureStorage.AddUserSecretAsync(appUser.ToEntityHeader(), blob);
            if (!addRecoveryResult.Successful)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpConfirmEnrollmentFailed, user, org, errors: "add_recoverycodes_failed");
                return addRecoveryResult.ToInvokeResult<List<string>>();
            }

            appUser.RecoveryCodesSecretId = addRecoveryResult.Result;
            appUser.RecoveryCodes = null;
            appUser.LastMfaDateTimeUtc = nowUtc;
            appUser.TwoFactorEnabled = true;

            try
            {
                await _appUserRepo.UpdateAsync(appUser);
            }
            catch
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpConfirmEnrollmentFailed, user, org, errors: "update_user_failed");
                await _secureStorage.RemoveUserSecretAsync(appUser.ToEntityHeader(), addRecoveryResult.Result);
                throw;
            }

            await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpConfirmEnrollmentSuccess, user, org);

            return InvokeResult<List<string>>.Create(codes);
        }

        public async Task<InvokeResult> VerifyTotpAsync(string userId, string totp, bool isStepUp, EntityHeader org, EntityHeader user)
        {
            if (String.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));

            await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpVerifyStart, user, org);

            if (String.IsNullOrEmpty(totp))
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpVerifyFailed, user, org, errors: "missing_totp");
                return InvokeResult.FromError("missing_totp");
            }

            totp = totp.Trim();
            if (totp.Length < 6 || totp.Length > 8)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpVerifyFailed, user, org, errors: "invalid_totp_format");
                return InvokeResult.FromError("invalid_totp_format");
            }

            var appUser = await _appUserRepo.FindByIdAsync(userId);
            if (appUser == null)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpVerifyFailed, user, org, errors: "user_not_found");
                return InvokeResult.FromError("user_not_found");
            }

            if (!appUser.TwoFactorEnabled)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpVerifyFailed, user, org, errors: "mfa_not_enabled");
                return InvokeResult.FromError("mfa_not_enabled");
            }

            if (String.IsNullOrEmpty(appUser.AuthenticatorKeySecretId))
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpVerifyFailed, user, org, errors: "mfa_not_enrolled");
                return InvokeResult.FromError("mfa_not_enrolled");
            }

            var secretResult = await _secureStorage.GetUserSecretAsync(appUser.ToEntityHeader(), appUser.AuthenticatorKeySecretId);
            if (!secretResult.Successful)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpVerifyFailed, user, org, errors: "get_secret_failed");
                return secretResult.ToInvokeResult();
            }

            if (String.IsNullOrEmpty(secretResult.Result))
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpVerifyFailed, user, org, errors: "mfa_secret_missing");
                return InvokeResult.FromError("mfa_secret_missing");
            }

            var matchResult = TryMatchTotpTimeStep(secretResult.Result, totp);
            if (!matchResult.Successful)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpVerifyFailed, user, org, errors: "totp_mismatch");
                return matchResult.ToInvokeResult();
            }

            var lastMfaUtc = isStepUp ? DateTime.UtcNow.ToJSONString() : null;
            var acceptResult = await _appUserRepo.TryAcceptTotpTimeStepAsync(userId, matchResult.Result, isStepUp, lastMfaUtc);
            if (!acceptResult.Successful)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpVerifyFailed, user, org, errors: "accept_timestep_failed");
                return acceptResult.ToInvokeResult();
            }

            await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpVerifySuccess, user, org);

            return InvokeResult.Success;
        }


        public async Task<InvokeResult<List<string>>> RotateRecoveryCodesAsync(string userId, EntityHeader org, EntityHeader user)
        {
            if (String.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));

            await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpRotateRecoveryCodesStart, user, org);

            var appUser = await _appUserRepo.FindByIdAsync(userId);
            if (appUser == null)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpRotateRecoveryCodesFailed, user, org, errors: "user_not_found");
                return InvokeResult<List<string>>.FromError("user_not_found");
            }

            var codes = GenerateRecoveryCodes();
            var blob = BuildRecoveryCodeBlob(codes);

            if (!String.IsNullOrEmpty(appUser.RecoveryCodesSecretId))
            {
                var removeResult = await _secureStorage.RemoveUserSecretAsync(appUser.ToEntityHeader(), appUser.RecoveryCodesSecretId);
                if (!removeResult.Successful)
                {
                    await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpRotateRecoveryCodesFailed, user, org, errors: "remove_existing_recoverycodes_failed");
                    return removeResult.ToInvokeResult<List<string>>();
                }
            }

            var addRecoveryResult = await _secureStorage.AddUserSecretAsync(appUser.ToEntityHeader(), blob);
            if (!addRecoveryResult.Successful)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpRotateRecoveryCodesFailed, user, org, errors: "add_recoverycodes_failed");
                return addRecoveryResult.ToInvokeResult<List<string>>();
            }

            appUser.RecoveryCodesSecretId = addRecoveryResult.Result;
            appUser.RecoveryCodes = null;

            try
            {
                await _appUserRepo.UpdateAsync(appUser);
            }
            catch
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpRotateRecoveryCodesFailed, user, org, errors: "update_user_failed");
                await _secureStorage.RemoveUserSecretAsync(appUser.ToEntityHeader(), addRecoveryResult.Result);
                throw;
            }

            await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpRotateRecoveryCodesSuccess, user, org);

            return InvokeResult<List<string>>.Create(codes);
        }

        public async Task<InvokeResult> ConsumeRecoveryCodeAsync(string userId, string recoveryCode, bool isStepUp, EntityHeader org, EntityHeader user)
        {
            if (String.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));

            await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpConsumeRecoveryCodeStart, user, org);

            if (String.IsNullOrEmpty(recoveryCode))
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpConsumeRecoveryCodeFailed, user, org, errors: "missing_recovery_code");
                return InvokeResult.FromError("missing_recovery_code");
            }

            var appUser = await _appUserRepo.FindByIdAsync(userId);
            if (appUser == null)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpConsumeRecoveryCodeFailed, user, org, errors: "user_not_found");
                return InvokeResult.FromError("user_not_found");
            }

            if (String.IsNullOrEmpty(appUser.RecoveryCodesSecretId))
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpConsumeRecoveryCodeFailed, user, org, errors: "recovery_codes_not_configured");
                return InvokeResult.FromError("recovery_codes_not_configured");
            }

            var codesResult = await _secureStorage.GetUserSecretAsync(appUser.ToEntityHeader(), appUser.RecoveryCodesSecretId);
            if (!codesResult.Successful)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpConsumeRecoveryCodeFailed, user, org, errors: "get_recoverycodes_failed");
                return codesResult.ToInvokeResult();
            }

            if (String.IsNullOrEmpty(codesResult.Result))
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpConsumeRecoveryCodeFailed, user, org, errors: "recovery_codes_missing");
                return InvokeResult.FromError("recovery_codes_missing");
            }

            var consumeResult = TryConsumeRecoveryCode(codesResult.Result, recoveryCode);
            if (!consumeResult.Successful)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpConsumeRecoveryCodeFailed, user, org, errors: "recovery_code_invalid");
                return consumeResult.ToInvokeResult();
            }

            var removeResult = await _secureStorage.RemoveUserSecretAsync(appUser.ToEntityHeader(), appUser.RecoveryCodesSecretId);
            if (!removeResult.Successful)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpConsumeRecoveryCodeFailed, user, org, errors: "remove_recoverycodes_failed");
                return removeResult.ToInvokeResult();
            }

            var addResult = await _secureStorage.AddUserSecretAsync(appUser.ToEntityHeader(), consumeResult.Result);
            if (!addResult.Successful)
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpConsumeRecoveryCodeFailed, user, org, errors: "add_recoverycodes_failed");
                return addResult.ToInvokeResult();
            }

            appUser.RecoveryCodesSecretId = addResult.Result;
            appUser.RecoveryCodes = null;

            if (isStepUp) appUser.LastMfaDateTimeUtc = DateTime.UtcNow.ToJSONString();

            if (!String.IsNullOrEmpty(appUser.AuthenticatorKeySecretId))
            {
                await _secureStorage.RemoveUserSecretAsync(appUser.ToEntityHeader(), appUser.AuthenticatorKeySecretId);
            }

            appUser.AuthenticatorKeySecretId = null;
            appUser.AuthenticatorKey = null;
            appUser.TwoFactorEnabled = false;

            try
            {
                await _appUserRepo.UpdateAsync(appUser);
            }
            catch
            {
                await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpConsumeRecoveryCodeFailed, user, org, errors: "update_user_failed");
                await _secureStorage.RemoveUserSecretAsync(appUser.ToEntityHeader(), addResult.Result);
                throw;
            }

            await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpConsumeRecoveryCodeSuccess, user, org);

            return InvokeResult.Success;
        }

public async Task<InvokeResult> DisableMfaAsync(string userId, EntityHeader org, EntityHeader user)
{
    if (String.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));

    await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpDisableMfaStart, user, org);

    var appUser = await _appUserRepo.FindByIdAsync(userId);
    if (appUser == null)
    {
        await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpDisableMfaFailed, user, org, errors: "user_not_found");
        return InvokeResult.FromError("user_not_found");
    }

    if (!String.IsNullOrEmpty(appUser.AuthenticatorKeySecretId))
    {
        var removeResult = await _secureStorage.RemoveUserSecretAsync(appUser.ToEntityHeader(), appUser.AuthenticatorKeySecretId);
        if (!removeResult.Successful)
        {
            await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpDisableMfaFailed, user, org, errors: "remove_authenticator_secret_failed");
            return removeResult.ToInvokeResult();
        }
    }

    if (!String.IsNullOrEmpty(appUser.RecoveryCodesSecretId))
    {
        var removeResult = await _secureStorage.RemoveUserSecretAsync(appUser.ToEntityHeader(), appUser.RecoveryCodesSecretId);
        if (!removeResult.Successful)
        {
            await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpDisableMfaFailed, user, org, errors: "remove_recoverycodes_failed");
            return removeResult.ToInvokeResult();
        }
    }

    appUser.AuthenticatorKeySecretId = null;
    appUser.AuthenticatorKey = null;
    appUser.RecoveryCodesSecretId = null;
    appUser.RecoveryCodes = null;
    appUser.TwoFactorEnabled = false;
    appUser.LastMfaDateTimeUtc = null;
    appUser.LastTotpAcceptedTimeStep = 0;

    try
    {
        await _appUserRepo.UpdateAsync(appUser);
    }
    catch
    {
        await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpDisableMfaFailed, user, org, errors: "update_user_failed");
        throw;
    }

    await _authLogMgr.AddAsync(UserAdmin.Models.Security.AuthLogTypes.TotpDisableMfaSuccess, user, org);

    return InvokeResult.Success;
}


        public async Task<InvokeResult> ResetMfaAsync(string userId, EntityHeader org, EntityHeader user)
        {
            return await DisableMfaAsync(userId, org, user);
        }

        private InvokeResult<long> TryMatchTotpTimeStep(string secretBase32, string totp)
        {
            byte[] secretBytes;
            try
            {
                secretBytes = Base32Encoding.ToBytes(secretBase32);
            }
            catch
            {
                return InvokeResult<long>.FromError("mfa_secret_invalid");
            }

            var totpGen = new Totp(secretBytes, step: TotpStepSeconds, totpSize: TotpDigits);
            var nowSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var currentStep = nowSeconds / TotpStepSeconds;

            for (var offset = -1; offset <= 1; offset++)
            {
                var candidateStep = currentStep + offset;
                var candidateTime = DateTimeOffset.FromUnixTimeSeconds(candidateStep * TotpStepSeconds);
                var expected = totpGen.ComputeTotp(candidateTime.UtcDateTime);
                if (String.Equals(expected, totp, StringComparison.Ordinal)) return InvokeResult<long>.Create(candidateStep);
       
                _logger.Trace($"{this.Tag()} - {offset} - Invalid TOP Code - {totp} for secret {expected} - {nowSeconds}");
            }

            return InvokeResult<long>.FromError("Sorry, you have entered an invalid code. Please try again.");
        }

        private List<string> GenerateRecoveryCodes()
        {
            var codes = new List<string>(RecoveryCodeCount);
            for (var i = 0; i < RecoveryCodeCount; i++) codes.Add(GenerateRecoveryCode());
            return codes;
        }

        private string GenerateRecoveryCode()
        {
            const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

            var bytes = new byte[10];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(bytes);

            var chars = new char[10];
            for (var i = 0; i < chars.Length; i++) chars[i] = alphabet[bytes[i] % alphabet.Length];

            return new string(chars, 0, 5) + "-" + new string(chars, 5, 5);
        }

        private string BuildRecoveryCodeBlob(List<string> plaintextCodes)
        {
            var lines = new List<string>(plaintextCodes.Count);
            foreach (var code in plaintextCodes) lines.Add(HashRecoveryCodeLine(code));
            return String.Join("\n", lines);
        }

        private string HashRecoveryCodeLine(string plaintextCode)
        {
            var salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(salt);

            byte[] hash;
            using (var pbkdf2 = new Rfc2898DeriveBytes(Encoding.UTF8.GetBytes(plaintextCode), salt, Pbkdf2Iterations, HashAlgorithmName.SHA256)) hash = pbkdf2.GetBytes(32);
            {
                var saltB64 = Convert.ToBase64String(salt);
                var hashB64 = Convert.ToBase64String(hash);
                return $"{RecoveryCodeLineVersion}|{RecoveryCodeAlgorithm}|{Pbkdf2Iterations}|{saltB64}|{hashB64}";
            }
        }

        private InvokeResult<string> TryConsumeRecoveryCode(string blob, string plaintextCode)
        {
            var lines = blob.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i].Trim();
                if (String.IsNullOrEmpty(line)) continue;

                var parts = line.Split('|');
                if (parts.Length != 5) continue;
                if (!String.Equals(parts[0], RecoveryCodeLineVersion, StringComparison.Ordinal)) continue;
                if (!String.Equals(parts[1], RecoveryCodeAlgorithm, StringComparison.Ordinal)) continue;
                if (!Int32.TryParse(parts[2], out var iterations)) continue;

                byte[] salt;
                byte[] expectedHash;
                try
                {
                    salt = Convert.FromBase64String(parts[3]);
                    expectedHash = Convert.FromBase64String(parts[4]);
                }
                catch
                {
                    continue;
                }
                byte[] actualHash;
                using (var pbkdf2 = new Rfc2898DeriveBytes(Encoding.UTF8.GetBytes(plaintextCode), salt, iterations, HashAlgorithmName.SHA256)) actualHash = pbkdf2.GetBytes(expectedHash.Length);
                    if (!CryptographicOperations.FixedTimeEquals(actualHash, expectedHash)) continue;

                lines.RemoveAt(i);
                return InvokeResult<string>.Create(String.Join("\n", lines));
            }

            return InvokeResult<string>.FromError("invalid_recovery_code");
        }
    }
}
