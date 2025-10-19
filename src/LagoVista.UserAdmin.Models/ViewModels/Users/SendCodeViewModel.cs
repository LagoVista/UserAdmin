// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: b06dc6b652a5dbffc97f799d3db472cce1ce238c333892d675a6eede78c91c2e
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.UserAdmin.Models;
using LagoVista.UserAdmin.Models.Resources;
using LagoVista.UserAdmin.Resources;
using System.Collections.Generic;


namespace LagoVista.UserAdmin.ViewModels.Users
{
    [EntityDescription(Domains.SecurityViewModels, UserAdminResources.Names.SendCodeVM_Title, UserAdminResources.Names.SetPasswordVM_Help, UserAdminResources.Names.SendCodeVM_Description, EntityDescriptionAttribute.EntityTypes.ViewModel, typeof(UserAdminResources))]
    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }

        public ICollection<SelectListItem> Providers { get; set; }

        public string ReturnUrl { get; set; }

        public bool RememberMe { get; set; }
    }
}
