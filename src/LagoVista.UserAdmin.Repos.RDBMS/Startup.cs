using LagoVista.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using LagoVista.UserAdmin.Interfaces.Managers;

namespace LagoVista.UserAdmin.Repos.RDBMS
{
    public static class Startup
    {
        public static void Init(IServiceCollection services, IConnectionSettings connectionSettings)
        {
            services.AddSingleton<IRDBMSManager, RDBMSManager>();

           var connectionString = $"Server=tcp:{connectionSettings.Uri},1433;Initial Catalog={connectionSettings.ResourceName};Persist Security Info=False;User ID={connectionSettings.UserName};Password={connectionSettings.Password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

            services.AddDbContext<UserAdminDataContext>(options =>
                options.UseSqlServer(connectionString));
        }
    }
}
