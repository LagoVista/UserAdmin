using LagoVista.CloudStorage.DocumentDB;
using LagoVista.Core.PlatformSupport;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Linq;
using System.Threading.Tasks;
using LagoVista.Core.Models;
using System.Collections.Generic;
using Microsoft.Azure.Documents;
using LagoVista.UserAdmin.Models.Orgs;

namespace LagoVista.UserAdmin.Repos.Users
{
    public class AppUserRepo : DocumentDBRepoBase<AppUser>, IAppUserRepo
    {
        bool _shouldConsolidateCollections;
        IRDBMSManager _rdbmsUserManager;

        public AppUserRepo(IRDBMSManager rdbmsUserManager, IUserAdminSettings userAdminSettings, IAdminLogger logger) : 
            base(userAdminSettings.UserStorage.Uri, userAdminSettings.UserStorage.AccessKey, userAdminSettings.UserStorage.ResourceName, logger)
        {
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
            return GetDocumentAsync(id,false);
        }

        public async Task<AppUser> FindByNameAsync(string userName)
        {
            return (await QueryAsync(usr => usr.UserName == userName.ToLower())).FirstOrDefault();
        }

        public async Task<AppUser> FindByEmailAsync(string email)
        {
            return (await QueryAsync(usr => usr.Email == email.ToUpper())).FirstOrDefault();
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

        public async Task<IEnumerable<UserInfoSummary>> GetUserSummaryForListAsync(IEnumerable<OrgUser> orgUsers)
        {
            var sqlParams = string.Empty;
            var idx = 0;
            var paramCollection = new SqlParameterCollection();
            foreach (var orgUser in orgUsers)
            {
                if(!String.IsNullOrEmpty(sqlParams))
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
                                select appUser.ToUserInfoSummary(orgUser.IsOrgAdmin);            

            return userSummaries;
        }
    }
}
