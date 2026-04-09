using LagoVista.UserAdmin.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Services
{
    public class Startup
    {
        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IDxfLocationDiagramConverter, DxfLocationDiagramConverter>();
            services.AddScoped<IDxfLocationDiagramDescriptorConverter, DxfLocationDiagramDescriptorConverter>();
        }
    }
}
