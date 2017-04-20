using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Managers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin
{
    public class Startup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IAppUserManager, AppUserManager>();
            services.AddTransient<IOrganizationManager, OrganizationManager>();
            services.AddTransient<ISubscriptionManager, SubscriptionManager>();
        }
    }
}