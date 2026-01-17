using ExCSS;
using LagoVista.Core;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Testing;
using RingCentral;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    /// <summary>
    /// Server-side manager behind /api/testadmin endpoints.
    /// V1: operations target a fixed test user/org resolved internally (no ids over the wire).
    /// </summary>
    public class AppUserTestingManager : ManagerBase, IAppUserTestingManager
    {
        private readonly IAppUserTestingDslRepo _dslStore;
        private readonly IAppUserTestRunRepo _testRunStore;
        private readonly IAppUserManager _userManager;  
        private readonly ISignInManager _signInManager; 
        private readonly IAppUserRepo _appUserRepo;



        public AppUserTestingManager(IAppUserTestingDslRepo dslStore,
                                   IAppUserTestRunRepo testRunStore,
                                   IDependencyManager depManager,
                                   ISecurity security,
                                   IAppUserRepo appUserRepo,
                                   IAppUserManager userManager,
                                   ISignInManager signInManager,
                                   IAdminLogger logger,
                                   IAppConfig appConfig) : base(logger, appConfig, depManager, security)
        {
            _dslStore = dslStore ?? throw new ArgumentNullException(nameof(dslStore));
            _testRunStore = testRunStore ?? throw new ArgumentNullException(nameof(testRunStore));
            _appUserRepo = appUserRepo ?? throw new ArgumentNullException(nameof(appUserRepo));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager)); 
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        }

        /* Preconditions / Setup */
        public Task<InvokeResult> SignInTestUser()
        {
            return Task.FromResult(InvokeResult.FromError("NotImplemented", "SignInTestUser not implemented yet."));
        }

        public Task<InvokeResult> SignOutTestUser()
        {
            return Task.FromResult(InvokeResult.FromError("NotImplemented", "SignOutTestUser not implemented yet."));
        }

        public Task<InvokeResult> DeleteTestUserAsync()
        {
            return _userManager.DeleteUserAsync(TestUserSeed.User.Id, TestUserSeed.Org1, TestUserSeed.User );
        }

        public async Task<InvokeResult> ApplySetupAsync(AuthTenantStateSnapshot preconditions, bool loginUser)
        {
            var timeStamp = DateTime.UtcNow.ToJSONString();

            var user = await _appUserRepo.FindByIdAsync( TestUserSeed.User.Id );
            var existing = user != null;

            if(preconditions.UserExists == false)
            {
                if(user == null)
                {
                    return InvokeResult.Success;
                }
             
                await _userManager.DeleteUserAsync( TestUserSeed.User.Id, TestUserSeed.Org1, TestUserSeed.User );
                return InvokeResult.Success;
            }
            
            if(user == null)
            {
                user = new Models.Users.AppUser();
                user.Id = TestUserSeed.User.Id;
                user.FirstName = TestUserSeed.FirstName;
                user.LastName = TestUserSeed.LastName;
                user.Email = TestUserSeed.Email;
                user.PhoneNumber = TestUserSeed.PhoneNumber;
                user.UserName = TestUserSeed.Email;
                user.CreatedBy = TestUserSeed.User;
                user.LastUpdatedBy = TestUserSeed.User;
                user.LastUpdatedDate = timeStamp;
                user.CreationDate = timeStamp;
            }

            if(preconditions.EmailConfirmed.HasValue)  user.EmailConfirmed = preconditions.EmailConfirmed.Value;
            if(preconditions.PhoneNumberConfirmed.HasValue)  user.PhoneNumberConfirmed = preconditions.PhoneNumberConfirmed.Value;
            if(preconditions.TwoFactorEnabled.HasValue)  user.TwoFactorEnabled = preconditions.TwoFactorEnabled.Value;
            if(preconditions.IsAccountDisabled.HasValue)  user.IsAccountDisabled = preconditions.IsAccountDisabled.Value;
            if(preconditions.IsAccountDisabled.HasValue)  user.IsAccountDisabled = preconditions.IsAccountDisabled.Value;

            if(!existing)
                await _appUserRepo.CreateAsync(user);
            else 
                await _appUserRepo.UpdateAsync(user);
        
            return InvokeResult.Success;
        }

        public Task<InvokeResult<string>> GetLastEmailTokenAsync()
        {
            return Task.FromResult(InvokeResult<string>.FromError("NotImplemented", "GetLastEmailTokenAsync not implemented yet."));
        }

        public Task<InvokeResult<string>> GetLastSmsTokenAsync()
        {
            return Task.FromResult(InvokeResult<string>.FromError("NotImplemented", "GetLastSmsTokenAsync not implemented yet."));
        }

        /* Snapshot Getter */

        public Task<InvokeResult<AuthTenantStateSnapshot>> GetUserSnapshotAsync(string ceremonyId = null)
        {
            return Task.FromResult(InvokeResult<AuthTenantStateSnapshot>.FromError("NotImplemented", "GetUserSnapshotAsync not implemented yet."));
        }

        public Task<InvokeResult<TestRunVerification>> GetVerificationAsync(string ceremonyId = null)
        {
            return Task.FromResult(InvokeResult<TestRunVerification>.FromError("NotImplemented", "GetVerificationAsync not implemented yet."));
        }

        /* DSL Case Management */

        public async Task<InvokeResult> CreateDslAsync(AppUserTestingDSL dsl)
        {
            if (dsl == null)
            {
                throw new ArgumentNullException(nameof(dsl)); 
            }

            var now = DateTime.UtcNow.ToJSONString();
            dsl.OwnerOrganization = TestUserSeed.Org1;
            dsl.CreatedBy = TestUserSeed.User;
            dsl.LastUpdatedBy = TestUserSeed.User;
            dsl.CreationDate = now;
            dsl.LastUpdatedDate = now;

            await _dslStore.AddDSLAsync(dsl);
            return InvokeResult.Success;
        }

        public async Task<InvokeResult> UpdateDslAsync(AppUserTestingDSL dsl)
        {
            if (dsl == null)
            {
                throw new ArgumentNullException(nameof(dsl)); 
            }

            await _dslStore.UpdateDSLAsync(dsl);
            return InvokeResult.Success;
        }

        public async Task<InvokeResult<AppUserTestingDSL>> GetDslAsync(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return InvokeResult<AppUserTestingDSL>.FromError("ArgumentNull", "id is required.");
            }

            var dsl = await _dslStore.GetByIdAsync(id);
            if (dsl == null)
            {
                return InvokeResult<AppUserTestingDSL>.FromError("NotFound", $"DSL not found: {id}");
            }

            return InvokeResult<AppUserTestingDSL>.Create(dsl);
        }

        public async Task<ListResponse<AppUserTestingDSLSummary>> ListDslAsync(ListRequest request)
        {
            return  await _dslStore.ListAsync(TestUserSeed.Org1.Id, request);
        }

        public async Task<InvokeResult> DeleteDslAsync(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return InvokeResult.FromError("ArgumentNull", "id is required.");
            }

            await _dslStore.DeleteByIdAsync(id);
            return InvokeResult.Success;
        }

        /* Run Persistence */

        public async Task<InvokeResult<AppUserTestRun>> CreateRunAsync(AppUserTestRun run)
        {
            if (run == null)
            {
                throw new ArgumentNullException(nameof(run));
            }

        
            var now = DateTime.UtcNow.ToJSONString();
            run.OwnerOrganization = TestUserSeed.Org1;
            run.CreatedBy = TestUserSeed.User;
            run.LastUpdatedBy = TestUserSeed.User;
            run.CreationDate = now;
            run.LastUpdatedDate = now;

            await _testRunStore.CreateRunAsync(run);
            return InvokeResult<AppUserTestRun>.Create(run);
        }

        public async Task<InvokeResult> AppendRunEventAsync(string runId, AppUserTestRunEvent evt)
        {
            if (String.IsNullOrEmpty(runId))
            {
                return InvokeResult.FromError("ArgumentNull", "runId is required.");
            }

            if (evt == null)
            {
                return InvokeResult.FromError("ArgumentNull", "evt is required.");
            }

            await _testRunStore.AppendEventsAsync(runId, new[] { evt });
            return InvokeResult.Success;
        }

        public async Task<InvokeResult<AppUserTestRun>> FinishRunAsync(string runId, TestRunStatus status, TestRunVerification verification = null)
        {
            if (String.IsNullOrEmpty(runId))
            {
                return InvokeResult<AppUserTestRun>.FromError("ArgumentNull", "runId is required.");
            }

            var finishedUtc = DateTime.UtcNow;
            await _testRunStore.FinishRunAsync(runId, status, finishedUtc, verification);

            var run = await _testRunStore.GetRunAsync(runId);
            return InvokeResult<AppUserTestRun>.Create(run);
        }

        public async Task<InvokeResult<AppUserTestRun>> GetRunAsync(string runId)
        {
            if (String.IsNullOrEmpty(runId))
            {
                return InvokeResult<AppUserTestRun>.FromError("ArgumentNull", "runId is required.");
            }

            var run = await _testRunStore.GetRunAsync(runId);
            if (run == null)
            {
                return InvokeResult<AppUserTestRun>.FromError("NotFound", $"Run not found: {runId}");
            }

            return InvokeResult<AppUserTestRun>.Create(run);
        }

        public Task<ListResponse<AppUserTestRunSummary>> GetTestRunsAsync(ListRequest request)
        {
            return _testRunStore.GetRunsFoOrgAsync(TestUserSeed.Org1.Id, request);
        }

        /* Auth Log Review */

        public Task<InvokeResult<AuthLogReviewSummary>> GetAuthLogReviewAsync(DateTime fromUtc, DateTime toUtc)
        {
            return Task.FromResult(InvokeResult<AuthLogReviewSummary>.FromError("NotImplemented", "GetAuthLogReviewAsync not implemented yet."));
        }

    

    }
}
