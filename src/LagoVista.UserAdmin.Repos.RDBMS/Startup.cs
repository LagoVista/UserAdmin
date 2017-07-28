using LagoVista.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.Core.PlatformSupport;
using Microsoft.Extensions.Configuration;
using LagoVista.IoT.Logging.Exceptions;
using LagoVista.Core.Models;

namespace LagoVista.UserAdmin.Repos.RDBMS
{
    public static class Startup
    {
        public static void ConfigureServices(IConfigurationRoot configurationRoot, IServiceCollection services, ILogger logger)
        {
            var billingDbSection = configurationRoot.GetSection("BillingDb");
            if (billingDbSection == null)
            {
                logger.AddCustomEvent(LogLevel.ConfigurationError, "BillingManager_Startup", "Missing Section BillingDb");
                throw new InvalidConfigurationException(new IoT.Logging.Error() { ErrorCode = "CFG9991", Message = "Missing Section BillingDb" });
            }

            var connectionSettings = new ConnectionSettings()
            {
                Uri = billingDbSection["ServerURL"],
                ResourceName = billingDbSection["InitialCatalog"],
                UserName = billingDbSection["UserName"],
                Password = billingDbSection["Password"],
            };

            if (string.IsNullOrEmpty(connectionSettings.Uri))
            {
                logger.AddCustomEvent(LogLevel.ConfigurationError, "BillingManager_Startup", "Missing BillingDb__ServerURL");
                throw new InvalidConfigurationException(new IoT.Logging.Error() { ErrorCode = "CFG9999", Message = "Missing BillingDb__ServerURL" });
            }

            if (string.IsNullOrEmpty(connectionSettings.ResourceName))
            {
                logger.AddCustomEvent(LogLevel.ConfigurationError, "BillingManager_Startup", "Missing BillingDb__InitialCatalog");
                throw new InvalidConfigurationException(new IoT.Logging.Error() { ErrorCode = "CFG9999", Message = "Missing BillingDb__InitialCatalog" });
            }

            if (string.IsNullOrEmpty(connectionSettings.UserName))
            {
                logger.AddCustomEvent(LogLevel.ConfigurationError, "BillingManager_Startup", "Missing BillingDb__UserName");
                throw new InvalidConfigurationException(new IoT.Logging.Error() { ErrorCode = "CFG9999", Message = "Missing BillingDb__UserName" });
            }

            if (string.IsNullOrEmpty(connectionSettings.Password))
            {
                logger.AddCustomEvent(LogLevel.ConfigurationError, "BillingManager_Startup", "Missing BillingDb__Password");
                throw new InvalidConfigurationException(new IoT.Logging.Error() { ErrorCode = "CFG9999", Message = "Missing BillingDb__Password" });
            }

            var connectionString = $"Server=tcp:{connectionSettings.Uri},1433;Initial Catalog={connectionSettings.ResourceName};Persist Security Info=False;User ID={connectionSettings.UserName};Password={connectionSettings.Password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

            services.AddTransient<IRDBMSManager, RDBMSManager>();

            services.AddDbContext<UserAdminDataContext>(options =>
                options.UseSqlServer(connectionString));
        }
    }
}
