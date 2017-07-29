using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Users;
using System.Threading.Tasks;
using LagoVista.Core;
using LagoVista.UserAdmin.Models.Orgs;

namespace LagoVista.UserAdmin.Repos.RDBMS
{
    public class RDBMSManager : IRDBMSManager
    {
        UserAdminDataContext _dataContext;
        public RDBMSManager(UserAdminDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<InvokeResult> AddAppUserAsync(AppUser user)
        {
            var dbUser = new Models.RDBMSAppUser()
            {
                AppUserId = user.Id,
                CreationDate = user.CreationDate.ToDateTime(),
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}",
                LastUpdatedDate = user.LastUpdatedDate.ToDateTime()
            };

            _dataContext.AppUser.Add(dbUser);
            await _dataContext.SaveChangesAsync();

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> AddOrgAsync(Organization org)
        {
            var dbOrg = new Models.RDBMSOrg()
            {
                CreationDate = org.CreationDate.ToDateTime(),
                LastUpdatedDate = org.LastUpdatedDate.ToDateTime(),
                OrgBillingContactId = org.BillingContact.Id,
                OrgId = org.Id,
                OrgName = org.Name,
                Status = org.Status
            };

            _dataContext.Org.Add(dbOrg);
            await _dataContext.SaveChangesAsync();

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> UpdateAppUserAsync(AppUser user)
        {
            var dbUser = new Models.RDBMSAppUser()
            {
                AppUserId = user.Id,
                CreationDate = user.CreationDate.ToDateTime(),
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}",
                LastUpdatedDate = user.LastUpdatedDate.ToDateTime()
            };

            _dataContext.AppUser.Update(dbUser);
            await _dataContext.SaveChangesAsync();

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> UpdateOrgAsync(Organization org)
        {
            var dbOrg = new Models.RDBMSOrg()
            {
                CreationDate = org.CreationDate.ToDateTime(),
                LastUpdatedDate = org.LastUpdatedDate.ToDateTime(),
                OrgBillingContactId = org.BillingContact.Id,
                OrgId = org.Id,
                OrgName = org.Name,
                Status = org.Status
            };

            _dataContext.Org.Update(dbOrg);
            await _dataContext.SaveChangesAsync();

            return InvokeResult.Success;
        }
    }
}
