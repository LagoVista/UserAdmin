using LagoVista.UserAdmin.Interfaces.Repos;
using LagoVista.UserAdmin.Interfaces.Repos.Account;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Repos.Account;
using LagoVista.UserAdmin.Repos.Orgs;
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
            services.AddTransient<ILocationAccountRepo, LocationAccountRepo>();
            services.AddTransient<ITokenRepo, TokenRepo>();
            services.AddTransient<IInviteUserRepo, InviteUserRepo>();
            services.AddTransient<IOrganizationAccountRepo, OrganizationAccountRepo>();
            services.AddTransient<IOrganizationLocationRepo, OrganizationLocationRepo>();
            services.AddTransient<IOrganizationRepo, OrganizationRepo>();
            services.AddTransient<ILocationRoleRepo, LocationRoleRepo>();
            services.AddTransient<IOrganizationRoleRepo, OrganizationRoleRepo>();
            services.AddTransient<IRoleRepo, RoleRepo>();
            services.AddTransient<ISubscriptionRepo,SubscriptionRepo>();
        }
    }
}
