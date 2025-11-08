// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: a4bfd4e0c0a54eb0f328ef27b5acbb1f4110b05f71ca3d7fdeac92c229104563
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Resources;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace LagoVista.UserAdmin.Models.Orgs
{
    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.Organization_Title, UserAdminResources.Names.Organization_Help,
    UserAdminResources.Names.Organization_Description, EntityDescriptionAttribute.EntityTypes.OrganizationModel, typeof(UserAdminResources),
        FactoryUrl: "/api/org/location/sublocation/factory", Icon: "icon-fo-internet-2")]
    public class SubLocation : IIDEntity, IKeyedEntity, INamedEntity, IValidateable, IIconEntity, IFormDescriptor
    {
        public string Id { get; set; }


        [FormField(LabelResource: LagoVistaCommonStrings.Names.Common_Name, FieldType: FieldTypes.Text, ResourceType: typeof(LagoVistaCommonStrings), IsRequired: true, IsUserEditable: true)]
        public string Name { get; set; }

        [FormField(LabelResource: LagoVistaCommonStrings.Names.Common_Key, HelpResource: LagoVistaCommonStrings.Names.Common_Key_Help, FieldType: FieldTypes.Key,
            RegExValidationMessageResource: LagoVistaCommonStrings.Names.Common_Key_Validation, ResourceType: typeof(LagoVistaCommonStrings), IsRequired: true)]
        public string Key { get; set; }

        [FormField(LabelResource: Resources.UserAdminResources.Names.Common_Icon, FieldType: FieldTypes.Icon, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public string Icon { get; set; } = "icon-fo-internet-2";

        [FormField(LabelResource: UserAdminResources.Names.Common_Description, IsRequired: false, FieldType: FieldTypes.MultiLineText, ResourceType: typeof(UserAdminResources))]
        public string Description { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Organization_Details, IsRequired: false, FieldType: FieldTypes.HtmlEditor, ResourceType: typeof(UserAdminResources))]
        public string Details { get; set; }


        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(Icon),
                nameof(Description),
                nameof(Details),
            };
        }
    }
}
