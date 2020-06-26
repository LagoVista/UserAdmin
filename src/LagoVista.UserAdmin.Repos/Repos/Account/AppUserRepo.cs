using LagoVista.CloudStorage.DocumentDB;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers;
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
        bool _shouldConsolidateCollections;
        IRDBMSManager _rdbmsUserManager;
        IUserAdminSettings _adminSettings;

        public AppUserRepo(IRDBMSManager rdbmsUserManager, IUserAdminSettings userAdminSettings, IAdminLogger logger) :
            base(userAdminSettings.UserStorage.Uri, userAdminSettings.UserStorage.AccessKey, userAdminSettings.UserStorage.ResourceName, logger)
        {
            _adminSettings = userAdminSettings;
            _shouldConsolidateCollections = userAdminSettings.ShouldConsolidateCollections;
            _rdbmsUserManager = rdbmsUserManager;
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
            await DeleteAsync(user);
        }

        public Task<AppUser> FindByIdAsync(string id)
        {
            return GetDocumentAsync(id, false);
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
            await Client.UpsertDocumentAsync(await GetCollectionDocumentsLinkAsync(), user);
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
            var paramCollection = new SqlParameterCollection();
            foreach (var orgUser in orgUsers)
            {
                if (!String.IsNullOrEmpty(sqlParams))
                {
                    sqlParams += ",";
                }
                var paramName = $"@userId{idx++}";

                sqlParams += paramName;
                paramCollection.Add(new SqlParameter(paramName, orgUser.UserId));
            }

            sqlParams.TrimEnd(',');

            //TODO: This seems kind of ugly...need to put more thought into this, this shouldn't be a query that is hit very often
            var query = $"SELECT * FROM c where c.id in ({sqlParams})";

            /* this sorta sux, but oh well */
            var appUsers = await QueryAsync(query, paramCollection);
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
                Model = results.Model.Select(rec => rec.ToUserInfoSummary()),
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
    }
}
