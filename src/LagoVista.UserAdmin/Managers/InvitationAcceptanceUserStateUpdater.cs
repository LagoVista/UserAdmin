using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;

namespace LagoVista.UserAdmin.Managers
{
    public static class InvitationAcceptanceUserStateUpdater
    {
        public static void ApplyAcceptedMembership(AppUser acceptedUser, EntityHeader organization)
        {
            if (acceptedUser == null) throw new ArgumentNullException(nameof(acceptedUser));
            if (EntityHeader.IsNullOrEmpty(organization)) throw new ArgumentNullException(nameof(organization));

            if (acceptedUser.Organizations == null)
                acceptedUser.Organizations = new List<EntityHeader>();

            acceptedUser.Organizations.RemoveAll(existing => existing.Id == organization.Id);
            acceptedUser.Organizations.Add(organization);
        }
    }
}
