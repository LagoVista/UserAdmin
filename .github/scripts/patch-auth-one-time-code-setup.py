from pathlib import Path

path = Path('src/LagoVista.UserAdmin/Managers/AppUserTestingManager.cs')
text = path.read_text(encoding='utf-8-sig')


def once(old, new, label):
    global text
    count = text.count(old)
    if count != 1:
        raise SystemExit(f'{label}: expected once, found {count}')
    text = text.replace(old, new, 1)

once(
    'using LagoVista.UserAdmin.Interfaces.Repos.Testing;\n',
    'using LagoVista.UserAdmin.Interfaces.Repos.Security;\nusing LagoVista.UserAdmin.Interfaces.Repos.Testing;\n',
    'security repo using')

once(
    'using System.Linq;\nusing System.Threading.Tasks;\n',
    'using System.Linq;\nusing System.Security.Cryptography;\nusing System.Text;\nusing System.Threading.Tasks;\n',
    'crypto usings')

once(
    '        private readonly IUserRegistrationManager _userRegistrationManager;\n        private readonly IAdminLogger _adminLogger;\n',
    '        private readonly IUserRegistrationManager _userRegistrationManager;\n        private readonly IPasswordResetCodeRepo _passwordResetCodeRepo;\n        private readonly IEmailVerificationCodeRepo _emailVerificationCodeRepo;\n        private readonly IAdminLogger _adminLogger;\n',
    'repo fields')

once(
    '                                   IUserRegistrationManager userRegistrationManager,\n                                   ITestArtifactStorage testArtifactStorage,\n',
    '                                   IUserRegistrationManager userRegistrationManager,\n                                   IPasswordResetCodeRepo passwordResetCodeRepo,\n                                   IEmailVerificationCodeRepo emailVerificationCodeRepo,\n                                   ITestArtifactStorage testArtifactStorage,\n',
    'constructor parameters')

once(
    '            _userRegistrationManager = userRegistrationManager ?? throw new ArgumentNullException(nameof(userRegistrationManager));\n            _magicLinkManager = magicLinkManager ?? throw new ArgumentNullException(nameof(magicLinkManager));\n',
    '            _userRegistrationManager = userRegistrationManager ?? throw new ArgumentNullException(nameof(userRegistrationManager));\n            _passwordResetCodeRepo = passwordResetCodeRepo ?? throw new ArgumentNullException(nameof(passwordResetCodeRepo));\n            _emailVerificationCodeRepo = emailVerificationCodeRepo ?? throw new ArgumentNullException(nameof(emailVerificationCodeRepo));\n            _magicLinkManager = magicLinkManager ?? throw new ArgumentNullException(nameof(magicLinkManager));\n',
    'constructor assignments')

helper = r'''
        private static string ComputeOneTimeCodeHash(AppUser appUser, string code)
        {
            var key = !String.IsNullOrWhiteSpace(appUser.SecurityStamp) ? appUser.SecurityStamp : appUser.PasswordHash;
            if (String.IsNullOrWhiteSpace(key)) throw new InvalidOperationException("The test user does not have security state available for one-time codes.");

            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(code)));
            }
        }

        private async Task ApplyPasswordRecoveryCodeSetupAsync(AppUser appUser, AuthOneTimeCodeState state, TestUserCredentials credentials)
        {
            if (state == null || state.Status == AuthOneTimeCodeStatus.DontCare) return;

            await _passwordResetCodeRepo.ClearAsync(appUser.Id);
            if (state.Status == AuthOneTimeCodeStatus.NotSet) return;

            var code = RandomNumberGenerator.GetInt32(0, 1000000).ToString("D6");
            var now = DateTime.UtcNow;
            await _passwordResetCodeRepo.StoreAsync(new PasswordResetCode
            {
                Id = Guid.NewGuid().ToId(),
                UserId = appUser.Id,
                CodeHash = ComputeOneTimeCodeHash(appUser, code),
                CreatedUtc = now,
                ExpiresUtc = state.Status == AuthOneTimeCodeStatus.Expired ? now.AddMinutes(-1) : now.AddMinutes(10),
                AttemptCount = state.AttemptCount ?? 0,
                ConsumedUtc = state.Status == AuthOneTimeCodeStatus.Consumed ? (DateTime?)now : null
            });

            credentials.PasswordRecoveryCode = code;
        }

        private async Task ApplyEmailVerificationCodeSetupAsync(AppUser appUser, AuthOneTimeCodeState state, TestUserCredentials credentials)
        {
            if (state == null || state.Status == AuthOneTimeCodeStatus.DontCare) return;

            await _emailVerificationCodeRepo.ClearAsync(appUser.Id);
            if (state.Status == AuthOneTimeCodeStatus.NotSet) return;

            var code = RandomNumberGenerator.GetInt32(0, 1000000).ToString("D6");
            var now = DateTime.UtcNow;
            await _emailVerificationCodeRepo.StoreAsync(new EmailVerificationCode
            {
                Id = Guid.NewGuid().ToId(),
                UserId = appUser.Id,
                CodeHash = ComputeOneTimeCodeHash(appUser, code),
                CreatedUtc = now,
                ExpiresUtc = state.Status == AuthOneTimeCodeStatus.Expired ? now.AddMinutes(-1) : now.AddMinutes(10),
                AttemptCount = state.AttemptCount ?? 0,
                ConsumedUtc = state.Status == AuthOneTimeCodeStatus.Consumed ? (DateTime?)now : null
            });

            credentials.EmailVerificationCode = code;
        }

'''

once(
    '        public async Task<InvokeResult<TestUserCredentials>> ApplySetupAsync(string testSceanrioId, EntityHeader org, EntityHeader user)\n',
    helper + '        public async Task<InvokeResult<TestUserCredentials>> ApplySetupAsync(string testSceanrioId, EntityHeader org, EntityHeader user)\n',
    'setup helper insertion')

once(
    '            if (preconditions.HasPassword.Value == SetCondition.Set)\n            {\n                await SetTestUserCredentials(testUser, userCredentials);\n            }\n\n            _adminLogger.Trace($"{this.Tag()} Updated user with preconditions.");',
    '            if (preconditions.HasPassword.Value == SetCondition.Set)\n            {\n                await SetTestUserCredentials(testUser, userCredentials);\n            }\n\n            await ApplyPasswordRecoveryCodeSetupAsync(testUser, preconditions.PasswordRecoveryCode, userCredentials);\n            await ApplyEmailVerificationCodeSetupAsync(testUser, preconditions.EmailVerificationCode, userCredentials);\n\n            _adminLogger.Trace($"{this.Tag()} Updated user with preconditions.");',
    'one time code setup calls')

path.write_text(text, encoding='utf-8')
