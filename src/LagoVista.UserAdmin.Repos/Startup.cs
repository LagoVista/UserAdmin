using LagoVista.UserAdmin.Interfaces.Repos;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Repos.Users;
using LagoVista.UserAdmin.Repos.Orgs;
using LagoVista.UserAdmin.Repos.Repos.Users;
using LagoVista.UserAdmin.Repos.Repos.Orgs;
using LagoVista.UserAdmin.Repos.Security;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Repos
{
    public static class Startup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IAppUserRepo, AppUserRepo>();
            services.AddTransient<ILocationUserRepo, LocationUserRepo>();
            services.AddTransient<ITokenRepo, TokenRepo>();
            services.AddTransient<IInviteUserRepo, InviteUserRepo>();
            services.AddTransient<IOrgUserRepo, OrgUserRepo>();
            services.AddTransient<IOrgLocationRepo, OrgLocationRepo>();
            services.AddTransient<IOrganizationRepo, OrganizationRepo>();
            services.AddTransient<ILocationRoleRepo, LocationRoleRepo>();
            services.AddTransient<IOrganizationRoleRepo, OrganizationRoleRepo>();
            services.AddTransient<IRoleRepo, RoleRepo>();
            services.AddTransient<ISubscriptionRepo,SubscriptionRepo>();
            services.AddTransient<ITeamRepo, TeamRepo>();
            services.AddTransient<ITeamUserRepo, TeamUserRepo>();
            services.AddTransient<IAssetSetRepo, AssetSetRepo>();
            services.AddTransient<IManagedAssetRepo, ManagedAssetRepo>();
        }
    }
}
