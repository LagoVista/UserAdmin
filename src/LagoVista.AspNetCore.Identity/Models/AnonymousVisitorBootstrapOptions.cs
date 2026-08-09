using LagoVista.AspNetCore.Identity.Interfaces;
using Microsoft.Extensions.Configuration;
using System;

namespace LagoVista.AspNetCore.Identity.Models
{
    public class AnonymousVisitorBootstrapOptions : IAnonymousVisitorBootstrapOptions
    {
        private const int DefaultLifetimeHours = 24;

        public AnonymousVisitorBootstrapOptions(IConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            var section = configuration.GetSection("AnonymousVisitor");
            AppUserId = section["AppUserId"];
            OrganizationId = section["OrganizationId"];

            ActiveLifetime = Int32.TryParse(section["ActiveLifetimeHours"], out var lifetimeHours) && lifetimeHours > 0
                ? TimeSpan.FromHours(lifetimeHours)
                : TimeSpan.FromHours(DefaultLifetimeHours);
        }

        public string AppUserId { get; }
        public string OrganizationId { get; }
        public TimeSpan ActiveLifetime { get; }
    }
}
