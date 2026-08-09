from pathlib import Path

path = Path('src/LagoVista.UserAdmin/Managers/AppUserTestingManager.cs')
text = path.read_text(encoding='utf-8-sig')

old = '''            if (preconditions.HasPassword.Value == SetCondition.Set)\n            {\n                await SetTestUserCredentials(testUser, userCredentials);\n            }\n\n            await ApplyPasswordRecoveryCodeSetupAsync(testUser, preconditions.PasswordRecoveryCode, userCredentials);\n'''
new = '''            if (preconditions.HasPassword.Value == SetCondition.Set)\n            {\n                await SetTestUserCredentials(testUser, userCredentials);\n            }\n\n            if (preconditions.PasswordResetAuthority.Value == SetCondition.Set)\n            {\n                userCredentials.PasswordResetToken = await _userManager.GeneratePasswordResetTokenAsync(testUser);\n            }\n\n            await ApplyPasswordRecoveryCodeSetupAsync(testUser, preconditions.PasswordRecoveryCode, userCredentials);\n'''

count = text.count(old)
if count != 1:
    raise SystemExit(f'password reset authority insertion: expected once, found {count}')

path.write_text(text.replace(old, new, 1), encoding='utf-8')
