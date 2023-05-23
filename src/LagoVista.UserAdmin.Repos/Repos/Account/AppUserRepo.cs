using LagoVista.CloudStorage;
using LagoVista.CloudStorage.DocumentDB;
using LagoVista.Core;
using LagoVista.Core.Exceptions;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Models.Users;
using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Users
{
    public class AppUserRepo : DocumentDBRepoBase<AppUser>, IAppUserRepo
    {
        private readonly bool _shouldConsolidateCollections;
        private readonly IRDBMSManager _rdbmsUserManager;
        private readonly IUserAdminSettings _adminSettings;
        private readonly IUserRoleRepo _userRoleRepo;

        public AppUserRepo(IRDBMSManager rdbmsUserManager, IUserRoleRepo userRoleRepo, IUserAdminSettings userAdminSettings, IAdminLogger logger) :
            base(userAdminSettings.UserStorage.Uri, userAdminSettings.UserStorage.AccessKey, userAdminSettings.UserStorage.ResourceName, logger)
        {
            _adminSettings = userAdminSettings;
            _shouldConsolidateCollections = userAdminSettings.ShouldConsolidateCollections;
            _rdbmsUserManager = rdbmsUserManager;
            _userRoleRepo = userRoleRepo;
        }

        protected override bool ShouldConsolidateCollections
        {
            get { return _shouldConsolidateCollections; }
        }

        public async Task CreateAsync(AppUser user)
        {
            await CreateDocumentAsync(user);
            await _rdbmsUserManager.AddAppUserAsync(user);
        }

        public async Task DeleteAsync(AppUser user)
        {
            await DeleteDocumentAsync(user.Id);
            await _rdbmsUserManager.DeleteAppUserAsync(user.Id);
        }

        public async Task<AppUser> FindByIdAsync(string id)
        {
            var appUser = await GetDocumentAsync(id, false);
            if (appUser == null)
                return null;
            
            if( appUser.CurrentOrganization != null)
            {
                var userRoles = await _userRoleRepo.GetRolesForUserAsync(id, appUser.CurrentOrganization.Id);
                appUser.CurrentOrganizationRoles = userRoles.Select(role => role.ToEntityHeader()).ToList();
            }

            return appUser;
        
        }

        public async Task<AppUser> FindByNameAsync(string userName)
        {
            if (String.IsNullOrEmpty(userName))
            {
                throw new InvalidOperationException("Attempt to find user with null or empty user name.");
            }

            var user = (await QueryAsync(usr => usr.UserName == userName.ToUpper())).FirstOrDefault();
            if (user == null)
            {
                return null;
            }

            //TODO: THIS SUX, when deserializing the query it auto converts to date time, we want the json string
            return await FindByIdAsync(user.Id);
        }

        public async Task<AppUser> FindByEmailAsync(string email)
        {
            if (String.IsNullOrEmpty(email))
            {
                throw new InvalidOperationException("Attempt to find user with null or empty user name.");
            }

            var user = (await QueryAsync(usr => usr.Email == email.ToUpper())).FirstOrDefault();
            if (user == null)
            {
                return null;
            }

            //TODO: THIS SUX, when deserializing the query it auto converts to date time, we want the json string
            return await FindByIdAsync(user.Id);
        }

        public async Task UpdateAsync(AppUser user)
        {
            await UpsertDocumentAsync(user);
            await _rdbmsUserManager.UpdateAppUserAsync(user);
        }

        public Task<AppUser> FindByThirdPartyLogin(string providerId, string providerKey)
        {
            throw new NotImplementedException();
        }

        public async Task<ListResponse<UserInfoSummary>> GetDeviceUsersAsync(string deviceRepoId, ListRequest listRequest)
        {
            var users = await QueryAsync(usr => usr.IsUserDevice == true && usr.DeviceRepo != null && usr.DeviceRepo.Id == deviceRepoId, listRequest);
            return ListResponse<UserInfoSummary>.Create(users.Model.Select(usr => usr.ToUserInfoSummary(false, false)));
        }

        public async Task<IEnumerable<UserInfoSummary>> GetUserSummaryForListAsync(IEnumerable<OrgUser> orgUsers)
        {
            var sqlParams = string.Empty;
            var idx = 0;
            var parameters  = new List<QueryParameter>();
            foreach (var orgUser in orgUsers)
            {
                if (!String.IsNullOrEmpty(sqlParams))
                {
                    sqlParams += ",";
                }
                var paramName = $"@userId{idx++}";

                sqlParams += paramName;
                parameters.Add(new QueryParameter(paramName, orgUser.UserId));
            }

            sqlParams.TrimEnd(',');

            //TODO: This seems kind of ugly...need to put more thought into this, this shouldn't be a query that is hit very often
            var query = $"SELECT * FROM c where c.id in ({sqlParams})";

            /* this sorta sux, but oh well */
            var appUsers = await QueryAsync(query, parameters.ToArray());
            var userSummaries = from appUser
                                in appUsers
                                join orgUser
                                in orgUsers on appUser.Id equals orgUser.UserId
                                select appUser.ToUserInfoSummary(orgUser.IsOrgAdmin, orgUser.IsAppBuilder);

            return userSummaries;
        }

        public async Task<ListResponse<UserInfoSummary>> GetAllUsersAsync(ListRequest listRequest)
        {
            var results = await DescOrderQueryAsync(us => us.IsUserDevice == false, us => us.CreationDate, listRequest);

            return new ListResponse<UserInfoSummary>()
            {
                Model = results.Model.Select(rec => rec.ToUserInfoSummary()).OrderBy(rec=>rec.Name),
                NextPartitionKey = results.NextPartitionKey,
                NextRowKey = results.NextRowKey,
                PageCount = results.PageCount,
                HasMoreRecords = results.HasMoreRecords,
                PageIndex = results.PageIndex,
                PageSize = results.PageSize,
            };
        }

        public async Task<ListResponse<UserInfoSummary>> GetActiveUsersAsync(ListRequest listRequest)
        {
            var results = await DescOrderQueryAsync(us => us.IsUserDevice == false && !us.IsAccountDisabled, us => us.CreationDate, listRequest);

            return new ListResponse<UserInfoSummary>()
            {
                Model = results.Model.Select(rec => rec.ToUserInfoSummary()).OrderBy(rec => rec.Name),
                NextPartitionKey = results.NextPartitionKey,
                NextRowKey = results.NextRowKey,
                PageCount = results.PageCount,
                HasMoreRecords = results.HasMoreRecords,
                PageIndex = results.PageIndex,
                PageSize = results.PageSize,
            };
        }


        public async Task<ListResponse<UserInfoSummary>> GetAllUsersAsync(ListRequest listRequest, bool? emailConfirmed, bool? phoneConfirmed)
        {
            Expression<Func<AppUser, bool>> mthd = (exp => exp.IsUserDevice == false);

            if (emailConfirmed.HasValue)
            {
                Expression<Func<AppUser, bool>> closeExpression = (exp => exp.EmailConfirmed == emailConfirmed.Value);
                var combined = Expression.And(mthd, closeExpression);
                mthd = Expression.Lambda<Func<AppUser, bool>>(combined);
            }

            if (phoneConfirmed.HasValue)
            {
                Expression<Func<AppUser, bool>> closeExpression = (exp => exp.PhoneNumberConfirmed == phoneConfirmed.Value);
                var combined = Expression.And(mthd, closeExpression);
                mthd = Expression.Lambda<Func<AppUser, bool>>(combined);
            }

            var results = await DescOrderQueryAsync(mthd, us => us.CreationDate, listRequest);

            return new ListResponse<UserInfoSummary>()
            {
                Model = results.Model.Select(rec => rec.ToUserInfoSummary()),
                NextPartitionKey = results.NextPartitionKey,
                NextRowKey = results.NextRowKey,
                PageCount = results.PageCount,
                HasMoreRecords = results.HasMoreRecords,
                PageIndex = results.PageIndex,
                PageSize = results.PageSize,
            };
        }

        public async Task DeleteAsync(string userId)
        {
            await this.DeleteDocumentAsync(userId);
            await this._rdbmsUserManager.DeleteAppUserAsync(userId);
        }

        public async Task<ListResponse<UserInfoSummary>> GetUsersWithoutOrgsAsync(ListRequest listRequest)
        {
            var users = (await QueryAsync(usr => (usr.Organizations == null || usr.Organizations.Count == 0), listRequest));
            return ListResponse<UserInfoSummary>.Create(users.Model.Select(usr => usr.ToUserInfoSummary(false, false)));
        }

        public async Task<AppUser> GetUserByExternalLoginAsync(ExternalLoginTypes loginType, string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                throw new InvalidOperationException("Attempt to find user with null or empty user name.");
            }

            var user = (await QueryAsync(usr => usr.ExternalLogins != null && usr.ExternalLogins.Where(ext => ext.Provider.Value == loginType && ext.Id == id).Any())).FirstOrDefault();
            if (user == null)
            {
                return null;
            }

            //TODO: THIS SUX, when deserializing the query it auto converts to date time, we want the json string
            return await FindByIdAsync(user.Id);
        }
        public async Task<AppUser> RemoveExternalLoginAsync(string userId, string externalLoginId)
        {
            if (String.IsNullOrEmpty(userId))
            {
                throw new InvalidOperationException("Attempt to find user with null or empty user name.");
            }

            var appUser = await FindByIdAsync(userId);
            if (appUser == null)
            {
                throw new RecordNotFoundException(nameof(AppUser), userId);
            }

            var existing = appUser.ExternalLogins.Where(exs => exs.Id == externalLoginId).FirstOrDefault();
            if(existing == null)
            {
                throw new RecordNotFoundException(nameof(ExternalLogin), externalLoginId);
            }

            appUser.ExternalLogins.Remove(existing);

            appUser.LastUpdatedBy = appUser.ToEntityHeader();
            appUser.LastUpdatedDate = DateTime.UtcNow.ToJSONString();

            await UpdateAsync(appUser);
            return appUser;
        }


        public async Task<AppUser> AssociateExternalLoginAsync(string userId, ExternalLogin external)
        {
            if (String.IsNullOrEmpty(userId))
            {
                throw new InvalidOperationException("Attempt to find user with null or empty user name.");
            }

            var appUser = await FindByIdAsync(userId);
            if (appUser == null)
            {
                throw new RecordNotFoundException(nameof(AppUser), userId);
            }

            var existing = appUser.ExternalLogins.Where(exs => exs.Provider.Value == external.Provider.Value).FirstOrDefault();
            if (existing != null)
            {
                existing.Email = external.Email ?? existing.Email;
                existing.OAuthToken = external.OAuthToken ?? existing.OAuthToken;
                existing.OAuthTokenSecretId = external.OAuthTokenSecretId ?? existing.OAuthTokenSecretId;
                existing.OAuthTokenVerifierSecretId = external.OAuthTokenVerifierSecretId ?? existing.OAuthTokenVerifierSecretId;
                existing.FirstName = external.FirstName ?? existing.FirstName;
                existing.LastName = external.LastName ?? existing.LastName;
                existing.Organization = external.Organization ?? existing.Organization;
            }
            else
            {
                appUser.ExternalLogins.Add(external);
            }

            appUser.LastUpdatedBy = appUser.ToEntityHeader();
            appUser.LastUpdatedDate = DateTime.UtcNow.ToJSONString();

            await UpdateAsync(appUser);
            return appUser;
        }
    }
}
