using LagoVista.Core.PlatformSupport;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using LagoVista.Core.Models;
using System;
using LagoVista.CloudStorage.Storage;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.IoT.Logging.Loggers;

namespace LagoVista.UserAdmin.Repos.Orgs
{
    public class OrganizationAccountRepo : TableStorageBase<OrganizationAccount>, IOrganizationAccountRepo
    {
        public OrganizationAccountRepo(IUserAdminSettings settings, IAdminLogger logger) : base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {

        }

        public Task<IEnumerable<OrganizationAccount>> GetUsersForAccount(string accountId)
        {
            return GetByParitionIdAsync(accountId);
        }

        public async Task<bool> QueryOrganizationHasAccountAsync(string orgId, string accountId)
        {
            return (await base.GetAsync(orgId, OrganizationAccount.CreateRowKey(orgId, accountId),false)) != null;
        }

        public async Task<OrganizationAccount> AddAccountUserAsync(OrganizationAccount accountUser)
        {
            await InsertAsync(accountUser);

            return accountUser;
        }

        public Task<IEnumerable<OrganizationAccount>> GetOrganizationsForAccountAsync(string accountId)
        {
            return GetByFilterAsync(FilterOptions.Create("AccountId", FilterOptions.Operators.Equals, accountId));
        }

        public Task<IEnumerable<OrganizationAccount>> GetAccountsForOrganizationAsync(string organizationId)
        {
            return GetByParitionIdAsync(organizationId);
        }

        public Task RemoveAccountFromOrgAsync(EntityHeader account, EntityHeader org, EntityHeader removedBy)
        {
            var rowKey = OrganizationAccount.CreateRowKey(org.Id, account.Id);
            return RemoveAsync(account.Id, rowKey);
        }

        public async Task<bool> QueryOrganizationHasAccountByEmailAsync(string organizationId, string email)
        {
            return (await GetByFilterAsync(
                FilterOptions.Create("Email", FilterOptions.Operators.Equals, email.ToUpper()),
                FilterOptions.Create("OrganizationId", FilterOptions.Operators.Equals, organizationId)
                )).ToList().Any();
        }
    }
}
