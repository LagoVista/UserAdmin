from pathlib import Path

manager_path = Path('src/LagoVista.UserAdmin/Managers/AppUserTestingManager.cs')
manager = manager_path.read_text(encoding='utf-8-sig')


def once(text, old, new, label):
    count = text.count(old)
    if count != 1:
        raise SystemExit(f'{label}: expected once, found {count}')
    return text.replace(old, new, 1)

manager = once(
    manager,
    '''            credentials.EmailAddress = user.Email;\n            credentials.Password = newPwd;\n\n            return InvokeResult.Success;''',
    '''            credentials.EmailAddress = user.Email;\n            credentials.Password = newPwd;\n            credentials.InvalidPassword = $"invalid!{Guid.NewGuid().ToId()}9876";\n\n            return InvokeResult.Success;''',
    'invalid password credential')

manager = once(
    manager,
    '''            if (preconditions.IsAccountDisabled.Value != SetCondition.DontCare) testUser.IsAccountDisabled = preconditions.IsAccountDisabled.Value == SetCondition.Set;\n            if (preconditions.IsOrgAdmin.Value != SetCondition.DontCare) testUser.IsOrgAdmin = preconditions.IsOrgAdmin.Value == SetCondition.Set;''',
    '''            if (preconditions.IsAccountDisabled.Value != SetCondition.DontCare) testUser.IsAccountDisabled = preconditions.IsAccountDisabled.Value == SetCondition.Set;\n            if (preconditions.IsLockedOut.Value != SetCondition.DontCare)\n            {\n                testUser.LockoutEnabled = preconditions.IsLockedOut.Value == SetCondition.Set;\n                testUser.LockoutDate = preconditions.IsLockedOut.Value == SetCondition.Set ? DateTime.UtcNow.AddHours(1).ToJSONString() : null;\n            }\n            if (preconditions.AccessFailedCount.HasValue) testUser.AccessFailedCount = preconditions.AccessFailedCount.Value;\n            if (preconditions.HasLastLogin.Value != SetCondition.DontCare) testUser.LastLogin = preconditions.HasLastLogin.Value == SetCondition.Set ? DateTime.UtcNow.AddMinutes(-5).ToJSONString() : null;\n            if (preconditions.IsOrgAdmin.Value != SetCondition.DontCare) testUser.IsOrgAdmin = preconditions.IsOrgAdmin.Value == SetCondition.Set;''',
    'sign in state setup')

manager = once(
    manager,
    '''            await _testRunStore.CreateRunAsync(run);\n\n            var scenario = await _testScenarioRepo.GetByIdAsync(run.TestScenario.Id);\n            var platformStatus = GetPlatformStatus(scenario, run.Platform);''',
    '''            var scenario = await _testScenarioRepo.GetByIdAsync(run.TestScenario.Id);\n            await VerifyCompletedRunAsync(run, scenario, org, user);\n            await _testRunStore.CreateRunAsync(run);\n\n            var platformStatus = GetPlatformStatus(scenario, run.Platform);''',
    'run receipt verification')

manager_path.write_text(manager, encoding='utf-8')

plan_path = Path('src/LagoVista.UserAdmin.Models/Testing/TestRunnerPlan.cs')
plan = plan_path.read_text(encoding='utf-8-sig')
plan = once(
    plan,
    '''        /// <summary>Default timeout for waits/actions in ms.</summary>\n        public bool EnableTracing { get; set; } = false;''',
    '''        /// <summary>\n        /// If true, runner enables Playwright tracing and uploads trace path as an artifact reference.\n        /// </summary>\n        public bool EnableTracing { get; set; } = false;''',
    'restore tracing comment')
plan_path.write_text(plan, encoding='utf-8')
