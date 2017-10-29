using LagoVista.UserAdmin.Interfaces.Repos;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Repos.Users;
using LagoVista.UserAdmin.Repos.Orgs;
using LagoVista.UserAdmin.Repos.Repos.Users;
using LagoVista.UserAdmin.Repos.Security;
using LagoVista.IoT.Logging;
using LagoVista.UserAdmin.Resources;
using LagoVista.UserAdmin.Repos.Repos.Apps;
using LagoVista.UserAdmin.Interfaces.Repos.Apps;
using LagoVista.UserAdmin.Repos.Repos.Security;
using LagoVista.Core.Interfaces;

namespace LagoVista.UserAdmin.Repos
{
    public static class Startup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IAppUserRepo, AppUserRepo>();
            services.AddTransient<ILocationUserRepo, LocationUserRepo>();
            services.AddTransient<IRefreshTokenRepo, RefreshTokenRepo>();
            services.AddTransient<IInviteUserRepo, InviteUserRepo>();
            services.AddTransient<IOrgUserRepo, OrgUserRepo>();
            services.AddTransient<IOrgLocationRepo, OrgLocationRepo>();
            services.AddTransient<IOrganizationRepo, OrganizationRepo>();
            services.AddTransient<ILocationRoleRepo, LocationRoleRepo>();
            services.AddTransient<IOrganizationRoleRepo, OrganizationRoleRepo>();
            services.AddTransient<IRoleRepo, RoleRepo>();
            services.AddTransient<IAccessLogRepo, AccessLogRepo>();
            services.AddTransient<ITeamRepo, TeamRepo>();
            services.AddTransient<ITeamUserRepo, TeamUserRepo>();
            services.AddTransient<IAssetSetRepo, AssetSetRepo>();
            services.AddTransient<IManagedAssetRepo, ManagedAssetRepo>();
            services.AddTransient<IAppInstanceRepo, AppInstanceRepo>();
            
            ErrorCodes.Register(typeof(UserAdminErrorCodes));
        }
    }
}
