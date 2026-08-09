using LagoVista.UserAdmin.Interfaces.Managers;
using Microsoft.Extensions.Configuration;
using System;

namespace LagoVista.UserAdmin.Managers
{
    public class AnonymousVisitorPromotionOptions : IAnonymousVisitorPromotionOptions
    {
        public AnonymousVisitorPromotionOptions(IConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            TermsAndConditionsVersion = configuration.GetSection("AnonymousVisitor")["TermsAndConditionsVersion"];
        }

        public string TermsAndConditionsVersion { get; }
    }
}
