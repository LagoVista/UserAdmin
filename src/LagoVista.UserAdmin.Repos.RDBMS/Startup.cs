// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 80810a557a623f3fab6692a5cf21bc98a40da6ad988d6484b850325ea84a0219
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.Repos;
using LagoVista.IoT.Logging.Exceptions;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LagoVista.UserAdmin.Repos.RDBMS
{
    public static class Startup
    {
        public static void ConfigureServices(IConfigurationRoot configurationRoot, Microsoft.Extensions.DependencyInjection.IServiceCollection services, ILogger logger)
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

            services.AddScoped<IAppUserRelationalRepo, AppUserRelationalRepo>();
            services.AddScoped<IOrganizationRelationalRepo, OrganizationRelationalRepo>();
            services.AddScoped<ISubscriptionRepo, SubscriptionRepo>();

            services.AddSingleton<IRDBMSConnectionSettings>(new RDBMSConnectionSettings(connectionSettings));

        }
    }
}
