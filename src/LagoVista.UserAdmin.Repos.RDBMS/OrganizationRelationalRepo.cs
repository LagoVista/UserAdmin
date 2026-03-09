using LagoVista.Core.Interfaces;
using LagoVista.Core.Interfaces.AutoMapper;
using LagoVista.Core.Models;
using LagoVista.Core.Repos;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.Models;
using LagoVista.Relational;
using LagoVista.Relational.DataContexts;
using Microsoft.EntityFrameworkCore;
using RingCentral;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.RDBMS
{
    [CriticalCoverage]
    public class OrganizationRelationalRepo : RelationalBase<BillingDataContext>, IOrganizationRelationalRepo
    {
        ILagoVistaAutoMapper _autoMapper;
     
        public OrganizationRelationalRepo(BillingDataContext context, IAdminLogger adminLogger, ILagoVistaAutoMapper autoMapper, ISecureStorage secureStorage) :
            base(context, adminLogger, secureStorage)
        {
            _autoMapper = autoMapper ?? throw new ArgumentNullException(nameof(autoMapper));
          
        }

        public Task AddOrganizationAsync(OrganizationDTO organization, EntityHeader org, EntityHeader user)
        {
            return AddWithContextAsync(org, user, async ctx =>
            {
                ctx.Org.Add(organization);
                await CommitAsync();
                ctx.ChangeTracker.Clear();
            });
        }

        public Task DeleteOrganizationAsync(string id, EntityHeader org, EntityHeader user)
        {
            return DeleteWithContextAsync(org, user, async ctx =>
            {
                await ctx.Org
                .ReadonlyQuery()
                .Where(usr => usr.OrgId == id)
                .ExecuteDeleteAsync();
                 await CommitAsync();
            });
        }

        public Task<OrganizationDTO> GetOrganizationAsync(string id, EntityHeader org, EntityHeader user)
        {
            return WithContextAsync(org, user, async ctx =>
            {
                return await ctx.Org
                .ReadonlyQuery()
                .Include(org => org.BillingContact)
                .Where(org => org.OrgId == id)
                .SingleOrDefaultAsync();
            });
        }

        public Task UpdateOrganizationAsync(OrganizationDTO organization, EntityHeader org, EntityHeader user)
        {
            return UpdateWithContextAsync(org, user, async ctx =>
            {
                ctx.Org.Update(organization);
                await CommitAsync();
                ctx.ChangeTracker.Clear();
            });
        }
    }
}
