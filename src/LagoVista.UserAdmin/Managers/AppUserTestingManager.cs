using LagoVista.Core;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Testing;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Testing;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    /// <summary>
    /// Server-side manager behind /api/sys/testing endpoints.
    /// </summary>
    public partial class AppUserTestingManager : ManagerBase, IAppUserTestingManager
    {
        private readonly IAppUserTestingDslRepo _testScenarioRepo;
        private readonly IAppUserTestRunRepo _testRunStore;
        private readonly IAppUserManager _userManager;  
        private readonly ISignInManager _signInManager; 
        private readonly IAppUserRepo _appUserRepo;
        private readonly IAuthViewRepo _authViewRepo;



        public AppUserTestingManager(IAppUserTestingDslRepo dslStore,
                                   IAppUserTestRunRepo testRunStore,
                                   IDependencyManager depManager,
                                   ISecurity security,
                                   IAppUserRepo appUserRepo,
                                   IAppUserManager userManager,
                                   ISignInManager signInManager,
                                   IAdminLogger logger,
                                   IAuthViewRepo authViewRepo,
                                   IAppConfig appConfig) : base(logger, appConfig, depManager, security)
        {
            _testScenarioRepo = dslStore ?? throw new ArgumentNullException(nameof(dslStore));
            _testRunStore = testRunStore ?? throw new ArgumentNullException(nameof(testRunStore));
            _appUserRepo = appUserRepo ?? throw new ArgumentNullException(nameof(appUserRepo));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager)); 
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _authViewRepo = authViewRepo ?? throw new ArgumentNullException(nameof(authViewRepo));
        }

        public async Task<InvokeResult> DeleteTestUserAsync(EntityHeader org, EntityHeader user)
        {
            return await _userManager.DeleteUserAsync(TestUserSeed.User.Id, org, user);
        }

        public async Task<InvokeResult> ApplySetupAsync(AuthTenantStateSnapshot preconditions, EntityHeader org, EntityHeader user)
        {
            ValidationCheck(preconditions, Actions.Any);

            var timeStamp = DateTime.UtcNow.ToJSONString();

            var testUser = await _appUserRepo.FindByIdAsync( TestUserSeed.User.Id );
            var existing = testUser != null;

            if(preconditions.EnsureUserExists.Value == SetCondition.NotSet)
            {
                if(testUser == null)
                {
                    return InvokeResult.Success;
                }
             
                // User Manager enforces Authorization
                await _userManager.DeleteUserAsync( TestUserSeed.User.Id, org, user );
                return InvokeResult.Success;
            }

            if (testUser == null)
            {
                testUser = new Models.Users.AppUser();
                testUser.Id = TestUserSeed.User.Id;
                testUser.FirstName = TestUserSeed.FirstName;
                testUser.LastName = TestUserSeed.LastName;
                testUser.Email = TestUserSeed.Email;
                testUser.PhoneNumber = TestUserSeed.PhoneNumber;
                testUser.UserName = TestUserSeed.Email;
                testUser.CreatedBy = TestUserSeed.User;
                testUser.LastUpdatedBy = TestUserSeed.User;
                testUser.LastUpdatedDate = timeStamp;
                testUser.CreationDate = timeStamp;
            }

            if (preconditions.EmailConfirmed.Value != SetCondition.DontCare)  testUser.EmailConfirmed = preconditions.EmailConfirmed.Value == SetCondition.Set;
            if(preconditions.PhoneNumberConfirmed.Value != SetCondition.DontCare)  testUser.PhoneNumberConfirmed = preconditions.PhoneNumberConfirmed.Value == SetCondition.Set;
            if(preconditions.TwoFactorEnabled.Value != SetCondition.DontCare)  testUser.TwoFactorEnabled = preconditions.TwoFactorEnabled.Value == SetCondition.Set;
            if(preconditions.IsAccountDisabled.Value != SetCondition.DontCare)  testUser.IsAccountDisabled = preconditions.IsAccountDisabled.Value == SetCondition.Set;

            if (!existing)
            {
                await AuthorizeAsync(testUser, AuthorizeResult.AuthorizeActions.Create, user, org);
                await _appUserRepo.CreateAsync(testUser);
            }
            else
            {
                await AuthorizeAsync(testUser, AuthorizeResult.AuthorizeActions.Update, user, org);
                await _appUserRepo.UpdateAsync(testUser);
            }
            return InvokeResult.Success;
        }

        public async Task<AppUser> GetTestUserAsync(EntityHeader org, EntityHeader user)
        {
            var appUser = await _appUserRepo.FindByIdAsync(TestUserSeed.User.Id);
            await AuthorizeAsync(appUser, AuthorizeResult.AuthorizeActions.Read, user, org);
            return appUser;
        }

        public Task<InvokeResult<string>> GetLastEmailTokenAsync(EntityHeader org, EntityHeader user)
        {
            return Task.FromResult(InvokeResult<string>.FromError("NotImplemented", "GetLastEmailTokenAsync not implemented yet."));
        }

        public Task<InvokeResult<string>> GetLastSmsTokenAsync(EntityHeader org, EntityHeader user)
        {
            return Task.FromResult(InvokeResult<string>.FromError("NotImplemented", "GetLastSmsTokenAsync not implemented yet."));
        }

        public async Task<InvokeResult> AddAuthViewAsync(AuthView authView, EntityHeader org, EntityHeader user)
        {
            ValidationCheck(authView, Actions.Create);
            await AuthorizeAsync(user, org, typeof(AuthView), Actions.Create);
            await _authViewRepo.AddAuthViewAsync(authView);
            return InvokeResult.Success;
        }

        public async Task<InvokeResult> UpdateAuthViewAsync(AuthView authView, EntityHeader org, EntityHeader user)
        {
            ValidationCheck(authView, Actions.Update);
            await AuthorizeAsync(user, org, typeof(AuthView), Actions.Update);
            await _authViewRepo.UpdateAuthViewAsync(authView);
            return InvokeResult.Success;
        }

        public async Task<AuthView> GetAuthViewAsync(string id, EntityHeader org, EntityHeader user)
        {
            var authView = await _authViewRepo.GetByIdAsync(id);
            await AuthorizeAsync(authView, AuthorizeResult.AuthorizeActions.Read, user, org);
            return authView;
        }

        public async Task<ListResponse<AuthViewSummary>> GetAuthViewsForOrgAsync(ListRequest request, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(AuthView), Actions.Read);
            return await _authViewRepo.ListAsync(org.Id, request);
        }

        public async Task<InvokeResult> DeleteAuthViewAsync(string id, EntityHeader org, EntityHeader user)
        {
            var view = await _authViewRepo.GetByIdAsync(id);
            await AuthorizeAsync(view, AuthorizeResult.AuthorizeActions.Delete, user, org);
            await _authViewRepo.DeleteByIdAsync(id);
            return InvokeResult.Success;
        }

        /* Test Scenario Management */

        public async Task<InvokeResult> AddTestScenarioAsync(AppUserTestScenario testScenario, EntityHeader org, EntityHeader user)
        {
            ValidationCheck(testScenario, Actions.Create);
            await AuthorizeAsync(user, org, typeof(AppUserTestScenario), Actions.Create);
            await _testScenarioRepo.AddDSLAsync(testScenario);
            return InvokeResult.Success;
        }

        public async Task<InvokeResult> UpdateTestScenarioAsync(AppUserTestScenario testScenario, EntityHeader org, EntityHeader user)
        {
            ValidationCheck(testScenario, Actions.Update);
            await AuthorizeAsync(user, org, typeof(AppUserTestScenario), Actions.Update);
            await _testScenarioRepo.UpdateDSLAsync(testScenario);
            return InvokeResult.Success;
        }

        public async Task<AppUserTestScenario> GetTestScenarioAsync(string id, EntityHeader org, EntityHeader user)
        {
            var scenario = await _testScenarioRepo.GetByIdAsync(id);
            await AuthorizeAsync(scenario, AuthorizeResult.AuthorizeActions.Read, user, org);
            return scenario;
        }

        public async Task<ListResponse<AppUserTestScenarioSummary>> GetTestScenariosForOrganizationAsync(ListRequest request, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(AppUserTestScenario), Actions.Read);
            return await _testScenarioRepo.ListAsync(org.Id, request);
        }

        public async Task<InvokeResult> DeleteTestScenarioAsync(string id, EntityHeader org, EntityHeader user)
        {
            var scenario = await _testScenarioRepo.GetByIdAsync(id);
            await AuthorizeAsync(scenario, AuthorizeResult.AuthorizeActions.Delete, user, org);
            await _testScenarioRepo.DeleteByIdAsync(id);
            return InvokeResult.Success;
        }

        /* Run Persistence */

        public async Task<InvokeResult> AddTestRunAsync(AppUserTestRun run, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(user, org, typeof(AppUserTestRun), Actions.Create);

            var now = DateTime.UtcNow.ToJSONString();
            run.OwnerOrganization = org;
            run.CreatedBy = user;
            run.LastUpdatedBy = user;
            run.CreationDate = now;
            run.LastUpdatedDate = now;

            await _testRunStore.CreateRunAsync(run);
            return InvokeResult.Success;
        }

        public async Task<InvokeResult> AppendRunEventAsync(string runId, AppUserTestRunEvent evt, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(user, org, typeof(AppUserTestRun), Actions.Update);

            await _testRunStore.AppendEventsAsync(runId, new[] { evt });
            return InvokeResult.Success;
        }

        public async Task<InvokeResult> FinishRunAsync(string runId, TestRunStatus status, EntityHeader org, EntityHeader user, TestRunVerification verification = null)
        {
            await AuthorizeAsync(user, org, typeof(AppUserTestRun), Actions.Update);

            var finishedUtc = DateTime.UtcNow;
            await _testRunStore.FinishRunAsync(runId, status, finishedUtc, verification);

            return InvokeResult.Success;
        }

        public async Task<AppUserTestRun> GetTestRunAsync(string runId, EntityHeader org, EntityHeader user)
        {
            var testRun = await _testRunStore.GetRunAsync(runId);
            await AuthorizeAsync(testRun, AuthorizeResult.AuthorizeActions.Read, user, org);
            return testRun;
        }

        public async Task<ListResponse<AppUserTestRunSummary>> GetTestRunsAsync(ListRequest request, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(AppUserTestRun), Actions.Read);
            return await _testRunStore.GetRunsFoOrgAsync(org.Id, request);
        }

        /* Auth Log Review */

        public async Task<InvokeResult<AuthLogReviewSummary>> GetAuthLogReviewAsync(DateTime fromUtc, DateTime toUtc, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(AuthLogReviewSummary), Actions.Read);
            return InvokeResult<AuthLogReviewSummary>.FromError("NotImplemented", "GetAuthLogReviewAsync not implemented yet.");
        }
    }
}
