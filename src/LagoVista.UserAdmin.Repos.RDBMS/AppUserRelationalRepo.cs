using LagoVista.Core.Interfaces;
using LagoVista.Core.Interfaces.AutoMapper;
using LagoVista.Core.Models;
using LagoVista.Core.Repos;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.Models;
using LagoVista.Relational;
using LagoVista.Relational.DataContexts;
using LagoVista.UserAdmin.Models.Orgs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.RDBMS
{
    public class AppUserRelationalRepo : RelationalBase<UserAdminDataContext>, IAppUserRelationalRepo
    {
        ILagoVistaAutoMapper _autoMapper;


        public AppUserRelationalRepo(UserAdminDataContext context, IAdminLogger adminLogger, ILagoVistaAutoMapper autoMapper, ISecureStorage secureStorage) :
           base(context, adminLogger, secureStorage)
        {
            _autoMapper = autoMapper ?? throw new ArgumentNullException(nameof(autoMapper));

        }

        public Task AddAppUserAsync(AppUserDTO user, EntityHeader org, EntityHeader addedByUser)
        {
            return AddWithContextAsync(org, addedByUser, async ctx =>
            {
                ctx.AppUser.Add(user);
                await CommitAsync();
            });
        }

        public Task DeleteAppUserAsync(string id, EntityHeader org, EntityHeader user)
        {
            return DeleteWithContextAsync(org, user, async ctx =>
            {
                 await ctx.AppUser
                .ReadonlyQuery()
                .Where(usr => usr.AppUserId == id)
                .ExecuteDeleteAsync();
                await CommitAsync();
            });
        }

        public Task<AppUserDTO> GetAppUserAsync(string id, EntityHeader org, EntityHeader user)
        {
            return WithContextAsync(org, user, async ctx =>
            {
                return ctx.AppUser
                .Where(usr => usr.AppUserId == id)
                .SingleOrDefault();
            });
        }

        public Task UpdateAppUserAsync(AppUserDTO user, EntityHeader org, EntityHeader updatedByUser)
        {
            return AddWithContextAsync(org, updatedByUser, async ctx =>
            {
                ctx.AppUser.Update(user);
                await CommitAsync();
            });
        }
    }
}
