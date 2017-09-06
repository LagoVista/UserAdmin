using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Users;
using System.Threading.Tasks;
using LagoVista.Core;
using System.Linq;
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
            var loadedUser = _dataContext.AppUser.Where(usr => usr.AppUserId == user.Id).FirstOrDefault();
            loadedUser.FullName = user.Name;
            loadedUser.LastUpdatedDate = user.LastUpdatedDate.ToDateTime();

            _dataContext.AppUser.Update(loadedUser);
            await _dataContext.SaveChangesAsync();
            return InvokeResult.Success;
        }

        public async Task<InvokeResult> UpdateOrgAsync(Organization org)
        {
            var loadedOrg = _dataContext.Org.Where(usr => usr.OrgId == org.Id).FirstOrDefault();
            loadedOrg.LastUpdatedDate = org.LastUpdatedDate.ToDateTime();
            loadedOrg.OrgName = org.Name;
            loadedOrg.Status = org.Status;
            loadedOrg.OrgBillingContactId = org.BillingContact.Id;

            _dataContext.Org.Update(loadedOrg);
            await _dataContext.SaveChangesAsync();

            return InvokeResult.Success;
        }
    }
}
