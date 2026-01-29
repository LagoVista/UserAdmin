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
using LagoVista.UserAdmin.Models.Security;
using LagoVista.UserAdmin.Models.Testing;
using LagoVista.UserAdmin.Models.Users;
using RingCentral;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IAppUserManager _appUserManager;
        private readonly IUserManager _userManager;
        private readonly ISignInManager _signInManager;
        private readonly IAuthenticationLogManager _authLogMgr;
        private readonly IOrganizationManager _orgManager;
        private readonly IAppUserRepo _appUserRepo;
        private readonly IAuthViewRepo _authViewRepo;
        private readonly ITestArtifactStorage _testArtifactStorage;
        private readonly IUserRegistrationManager _userRegistrationManager;
        private readonly IAdminLogger _adminLogger;

        public AppUserTestingManager(IAppUserTestingDslRepo dslStore,
                                   IAppUserTestRunRepo testRunStore,
                                   IDependencyManager depManager,
                                   ISecurity security,
                                   IAdminLogger adminLogger,
                                   IOrganizationManager orgManager,
                                   IAppUserRepo appUserRepo,
                                   IAppUserManager appuUserManager,
                                   ISignInManager signInManager,
                                   IAdminLogger logger,
                                   IAuthViewRepo authViewRepo,
                                   IAuthenticationLogManager authLogMgr,
                                   IUserManager userManager,
                                   IUserRegistrationManager userRegistrationManager,
                                   ITestArtifactStorage testArtifactStorage,
                                   IAppConfig appConfig) : base(logger, appConfig, depManager, security)
        {
            _authLogMgr = authLogMgr ?? throw new ArgumentNullException(nameof(authLogMgr));
            _adminLogger = adminLogger ?? throw new ArgumentNullException(nameof(adminLogger));
            _orgManager = orgManager ?? throw new ArgumentNullException(nameof(orgManager));
            _testScenarioRepo = dslStore ?? throw new ArgumentNullException(nameof(dslStore));
            _testRunStore = testRunStore ?? throw new ArgumentNullException(nameof(testRunStore));
            _appUserRepo = appUserRepo ?? throw new ArgumentNullException(nameof(appUserRepo));
            _appUserManager = appuUserManager ?? throw new ArgumentNullException(nameof(appUserRepo));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _authViewRepo = authViewRepo ?? throw new ArgumentNullException(nameof(authViewRepo));
            _testArtifactStorage = testArtifactStorage ?? throw new ArgumentNullException(nameof(testArtifactStorage));
            _userRegistrationManager = userRegistrationManager ?? throw new ArgumentNullException(nameof(userRegistrationManager));
        }

        public async Task<InvokeResult> DeleteTestUserAsync(EntityHeader org, EntityHeader user)
        {
            return await _appUserManager.DeleteUserAsync(TestUserSeed.User.Id, org, user);
        }

        public async Task<InvokeResult<TestUserCredentials>> GetTestUserCredentials(EntityHeader user, EntityHeader pwd)
        {
            var testUser = await _appUserRepo.FindByIdAsync(TestUserSeed.User.Id);
            var newPwd = $"test!{Guid.NewGuid().ToId()}1234";
            var token = await _userManager.GeneratePasswordResetTokenAsync(testUser);
            await _userManager.ResetPasswordAsync(testUser, token, newPwd);

            var credentials = new TestUserCredentials()
            {
                EmailAddress = testUser.Email,
                Password = newPwd
            };

            return InvokeResult<TestUserCredentials>.Create(credentials);
        }

        public async Task<InvokeResult<TestUserCredentials>> ApplySetupAsync(string testSceanrioId, EntityHeader org, EntityHeader user)
        {
            var scanario = await _testScenarioRepo.GetByIdAsync(testSceanrioId);
            _adminLogger.Trace($"{this.Tag()} Applying test user setup for scenario: {scanario.Name}");

            var preconditions = scanario.PreConditions;

            ValidationCheck(preconditions, Actions.Any);

            var timeStamp = DateTime.UtcNow.ToJSONString();

            var devTestUser = await _appUserRepo.FindByEmailAsync(TestUserSeed.Email);
            if(devTestUser != null && devTestUser.Id != TestUserSeed.User.Id)
            {
                _adminLogger.Trace($"{this.Tag()} Test user already exists but has a different ID, likely created by OAuth deleting before applying setup.");
                await _appUserManager.DeleteUserAsync(devTestUser.Id, org, user);
                _adminLogger.Trace($"{this.Tag()} Test user deleted.");
            }

            var gitHubUser = await _appUserRepo.GetUserByExternalLoginAsync(ExternalLoginTypes.GitHub, "SLOAUTH");
            

            var testUser = await _appUserRepo.FindByIdAsync(TestUserSeed.User.Id);

            if (preconditions.EnsureUserExists.Value == SetCondition.NotSet)
            {
                _adminLogger.Trace($"{this.Tag()} User Should Not Exist");

                if(gitHubUser != null)
                {
                    _adminLogger.Trace($"{this.Tag()} User Should Not Exist - GitHub Login Found - Deleting");
                    await _appUserManager.DeleteUserAsync(gitHubUser.Id, org, user);
                    _adminLogger.Trace($"{this.Tag()} User Should Not Exist - GitHub Login Found - Deleted");
                }

                if (testUser == null)
                {
                    _adminLogger.Trace($"{this.Tag()} User Should Not Exist - It Didn't");
                    return InvokeResult<TestUserCredentials>.Create(new TestUserCredentials());
                }

                // User Manager enforces Authorization
                await _appUserManager.DeleteUserAsync(TestUserSeed.User.Id, org, user);
                _adminLogger.Trace($"{this.Tag()} User Should Not Exist - Deleted");

                return InvokeResult<TestUserCredentials>.Create(new TestUserCredentials());
            }

            _adminLogger.Trace($"{this.Tag()} User Should Exist");


            var generatedPassword = $"a55{Guid.NewGuid().ToId()}!1234";

            if (testUser == null)
            {
                _adminLogger.Trace($"{this.Tag()} User Should Exist - It Didn't");
                var result = await _userRegistrationManager.CreateUserAsync(new Models.DTOs.RegisterUser()
                {
                    AppId = "WPF-AUTHTESTING",
                    ClientType = "WEBAPP",
                    DeviceId = "WPF-AUTHTESTING",
                    LoginType = LoginTypes.AppUser,
                    Email = TestUserSeed.Email,
                    FirstName = TestUserSeed.FirstName,
                    LastName = TestUserSeed.LastName,
                    Password = generatedPassword
                }, userId: TestUserSeed.User.Id);

                if (!result.Successful)
                {
                    _adminLogger.AddError(this.Tag(), result.ErrorMessage);
                    return result.ToInvokeResult<TestUserCredentials>();
                }

                _adminLogger.Trace($"{this.Tag()} User Should Exist - Created.");

                testUser = result.Result.AppUser;
            }

            if (preconditions.EmailConfirmed.Value != SetCondition.DontCare) testUser.EmailConfirmed = preconditions.EmailConfirmed.Value == SetCondition.Set;
            if (preconditions.PhoneNumberConfirmed.Value != SetCondition.DontCare) testUser.PhoneNumberConfirmed = preconditions.PhoneNumberConfirmed.Value == SetCondition.Set;
            if (preconditions.TwoFactorEnabled.Value != SetCondition.DontCare) testUser.TwoFactorEnabled = preconditions.TwoFactorEnabled.Value == SetCondition.Set;
            if (preconditions.IsAccountDisabled.Value != SetCondition.DontCare) testUser.IsAccountDisabled = preconditions.IsAccountDisabled.Value == SetCondition.Set;
            if (preconditions.IsOrgAdmin.Value != SetCondition.DontCare) testUser.IsOrgAdmin = preconditions.IsOrgAdmin.Value == SetCondition.Set;
            if (preconditions.ShowWelcome.Value != SetCondition.DontCare) testUser.ShowWelcome = preconditions.ShowWelcome.Value == SetCondition.Set;

            _adminLogger.Trace($"{this.Tag()} Set Pre Conditions on User");

            if (preconditions.BelongsToOrg.Value == SetCondition.Set)
            {
                _adminLogger.Trace($"{this.Tag()} Use Should belong to an Org");

                var existingOrg = await _orgManager.QueryOrgNamespaceInUseAsync(TestUserSeed.TEST_ORG_NS1);
                if (!existingOrg)
                {
                    _adminLogger.Trace($"{this.Tag()} Use Should belong to an Org - Does Not Exist - Creating - {TestUserSeed.TEST_ORG_NS1}");

                    var orgCreateResult = await _orgManager.CreateNewOrganizationAsync(new ViewModels.Organization.CreateOrganizationViewModel()
                    {
                        CreateGettingStartedData = false,
                        Namespace = TestUserSeed.TEST_ORG_NS1,
                        Name = TestUserSeed.Org1.Text
                    }, user, TestUserSeed.Org1.Id);


                    if (!orgCreateResult.Successful)
                    {
                        _adminLogger.AddError(this.Tag(), orgCreateResult.ErrorMessage);
                        return orgCreateResult.ToInvokeResult<TestUserCredentials>();
                    }

                    _adminLogger.Trace($"{this.Tag()} Use Should belong to an Org - Created {TestUserSeed.TEST_ORG_NS1}");

                    var addUserResult = await _orgManager.AddUserToOrgAsync(TestUserSeed.Org2.Id, TestUserSeed.User.Id, org, user);
                    if (!addUserResult.Successful)
                    {
                        _adminLogger.AddError(this.Tag(), addUserResult.ErrorMessage);
                        return addUserResult.ToInvokeResult<TestUserCredentials>();
                    }

                    _adminLogger.Trace($"{this.Tag()} Use Should belong to an Org - Added to Org {TestUserSeed.TEST_ORG_NS1}");
                }
                else
                {
                    _adminLogger.Trace($"{this.Tag()} Use Should belong to an Org - Already Exists {TestUserSeed.TEST_ORG_NS1}");

                    var result = await _orgManager.QueryOrganizationHasUserAsync(TestUserSeed.Org1.Id, TestUserSeed.User.Id, org, user);
                    if (!result)
                    {
                        var addUserToOrgresult = await _orgManager.AddUserToOrgAsync(TestUserSeed.Org1.Id, TestUserSeed.User.Id, org, user);
                        if (!addUserToOrgresult.Successful)
                        {
                            _adminLogger.AddError(this.Tag(), $"Could not add to org: {addUserToOrgresult.ErrorMessage}");
                            return addUserToOrgresult.ToInvokeResult<TestUserCredentials>();
                        }
                    }
                }
            }
            if (preconditions.BelongsToOrg.Value == SetCondition.NotSet)
            {
                _adminLogger.Trace($"{this.Tag()} Use should not belong to an org");

                if (testUser.Organizations.Any())
                {
                    testUser.Organizations.Clear();
                    testUser.CurrentOrganizationRoles.Clear();
                    testUser.CurrentOrganization = null;
                    testUser.IsOrgAdmin = false;
                    testUser.OwnerOrganization = null;
                }

                _adminLogger.Trace($"{this.Tag()} Use should not belong to an org -- Cleared");
            }

            await AuthorizeAsync(testUser, AuthorizeResult.AuthorizeActions.Update, user, org);
            await _appUserRepo.UpdateAsync(testUser);

            if (preconditions.BelongsToOrg.Value == SetCondition.NotSet)
            {
                _adminLogger.Trace($"{this.Tag()} Use should not belong to an org -- Should we remove org?");

                testUser.OwnerOrganization = null;
                var existingOrg = await _orgManager.QueryOrgNamespaceInUseAsync(TestUserSeed.TEST_ORG_NS1);
                if (existingOrg)
                {
                    _adminLogger.Trace($"{this.Tag()} Use should not belong to an org - exists deleteing");

                    var deleteOrgresult = await _orgManager.DeleteOrgAsync(TestUserSeed.Org1.Id, org, user);
                    if (!deleteOrgresult.Successful)
                    {
                        _adminLogger.AddError(this.Tag(), $"Could not delete org: {deleteOrgresult.ErrorMessage}");
                        return deleteOrgresult.ToInvokeResult<TestUserCredentials>();
                    }

                    _adminLogger.Trace($"{this.Tag()} Use should not belong to an org - deleted");
                }
                else
                    _adminLogger.Trace($"{this.Tag()} Use should not belong to an org - did not exist, nothing to do.");
            }

            if (preconditions.HasPassword.Value == SetCondition.Set)
            {
                var credentials = await GetTestUserCredentials(org, user);
                return InvokeResult<TestUserCredentials>.Create(credentials.Result);
            }
            else
            {
                return InvokeResult<TestUserCredentials>.Create(new TestUserCredentials() { EmailAddress = TestUserSeed.Email });
            }
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
            await _testScenarioRepo.UpdateTestScenarioAsync(testScenario);
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

        public async Task<InvokeResult> AddTestRunAsync(AppUserTestRun run, List<ArtifactFlie> files, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(user, org, typeof(AppUserTestRun), Actions.Create);

            var now = DateTime.UtcNow.ToJSONString();
            run.OwnerOrganization = org;
            run.CreatedBy = user;
            run.LastUpdatedBy = user;
            run.CreationDate = now;
            run.LastUpdatedDate = now;

            foreach (var artififact in run.Artifacts)
            {
                var file = files.Where(fl => fl.FileName == artififact.FileName).FirstOrDefault();
                if (file != null)
                {
                    // Now assign the name to retreive the file from the server.
                    artififact.FileName = await _testArtifactStorage.SaveArtifactAsync(org.Id, run.Id, artififact.FileName, file.ContentType, file.Buffer);
                }
            }

            await _testRunStore.CreateRunAsync(run);

            var scenario = await _testScenarioRepo.GetByIdAsync(run.TestScenario.Id);
            scenario.LastRun = run.Finished;
            scenario.LastStatus = run.Status.ToString();
            scenario.LastUpdatedDate = now;
            scenario.LastUpdatedBy = user;
            scenario.LastError = run.ErrorMesage;
            await _testScenarioRepo.UpdateTestScenarioAsync(scenario);
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

        public async Task<ListResponse<AuthenticationLog>> GetAuthLogReviewAsync(ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(AuthLogReviewSummary), Actions.Read);
            return await _authLogMgr.GetForUserIdAsync(TestUserSeed.User.Id, listRequest, org, user);
        }
    }

}
