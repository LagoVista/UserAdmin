using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.DTOs;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Models.Users;
using System;

namespace LagoVista.UserAdmin.Managers
{
    internal static class AppEndUserRegistrationGuard
    {
        public static void ValidateRequest(RegisterUser request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            if (request.LoginType != LoginTypes.AppEndUser)
            {
                return;
            }

            if (EntityHeader.IsNullOrEmpty(request.EndUserAppOrg))
            {
                throw new InvalidOperationException("EndUserAppOrg is required when creating an AppEndUser account.");
            }

            if (EntityHeader.IsNullOrEmpty(request.Customer))
            {
                throw new InvalidOperationException("Customer is required when creating an AppEndUser account.");
            }

            if (EntityHeader.IsNullOrEmpty(request.CustomerContact))
            {
                throw new InvalidOperationException("CustomerContact is required when creating an AppEndUser account.");
            }

            if (!String.IsNullOrWhiteSpace(request.OrgId) && !String.Equals(request.OrgId, request.EndUserAppOrg.Id, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("OrgId must match EndUserAppOrg.Id when creating an AppEndUser account.");
            }
        }

        public static void ApplyOrganizationDefaults(RegisterUser request, Organization organization, AppUser appUser)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (organization == null) throw new ArgumentNullException(nameof(organization));
            if (appUser == null) throw new ArgumentNullException(nameof(appUser));

            if (request.LoginType != LoginTypes.AppEndUser)
            {
                return;
            }

            ValidateRequest(request);

            if (!String.Equals(organization.Id, request.EndUserAppOrg.Id, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Loaded organization [{organization.Id}] does not match EndUserAppOrg [{request.EndUserAppOrg.Id}].");
            }

            if (EntityHeader.IsNullOrEmpty(organization.DefaultDeviceRepository))
            {
                throw new InvalidOperationException($"Cannot create AppEndUser because organization [{organization.Name}] does not have a default device repository configured.");
            }

            if (EntityHeader.IsNullOrEmpty(organization.DefaultInstance))
            {
                throw new InvalidOperationException($"Cannot create AppEndUser because organization [{organization.Name}] does not have a default deployment instance configured.");
            }

            appUser.EndUserAppOrg = request.EndUserAppOrg;
            appUser.Customer = request.Customer;
            appUser.EmailConfirmed = true;
            appUser.CustomerContact = request.CustomerContact;
            appUser.CurrentOrganization = organization.CreateSummary();
            appUser.CurrentRepo = organization.DefaultDeviceRepository;
            appUser.CurrentInstance = organization.DefaultInstance;
        }
    }
}
