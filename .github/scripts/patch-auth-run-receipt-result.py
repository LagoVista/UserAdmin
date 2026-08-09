from pathlib import Path

path = Path('src/LagoVista.UserAdmin/Managers/AppUserTestingManager.cs')
text = path.read_text(encoding='utf-8-sig')
old = '''            var scenario = await _testScenarioRepo.GetByIdAsync(run.TestScenario.Id);\n            await VerifyCompletedRunAsync(run, scenario, org, user);\n            await _testRunStore.CreateRunAsync(run);\n            return InvokeResult.Success;'''
new = '''            var scenario = await _testScenarioRepo.GetByIdAsync(run.TestScenario.Id);\n            await VerifyCompletedRunAsync(run, scenario, org, user);\n            await _testRunStore.CreateRunAsync(run);\n\n            if (run.Status == TestRunStatus.Failed && !String.IsNullOrWhiteSpace(run.ErrorMessage))\n                return InvokeResult.FromError("AuthTestReceiptFailed", run.ErrorMessage);\n\n            return InvokeResult.Success;'''
count = text.count(old)
if count != 1:
    raise SystemExit(f'run receipt result: expected once, found {count}')
path.write_text(text.replace(old, new, 1), encoding='utf-8')
