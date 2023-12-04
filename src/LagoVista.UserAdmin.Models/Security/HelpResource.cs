﻿using LagoVista.Core.Attributes;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Resources;

namespace LagoVista.UserAdmin.Models.Security
{
    [EntityDescription(Domains.SecurityDomain, UserAdminResources.Names.HelpResource_Title, UserAdminResources.Names.HelpResource_Description, UserAdminResources.Names.HelpResource_Description, EntityDescriptionAttribute.EntityTypes.SimpleModel, typeof(UserAdminResources))]
    public class HelpResource
    {
        [FormField(LabelResource: UserAdminResources.Names.Common_Name, IsRequired: true, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources))]
        public string Name { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.HelpResource_Guide, FieldType: FieldTypes.EntityHeaderPicker, WaterMark: UserAdminResources.Names.HelpResource_SelectGuide, ResourceType: typeof(UserAdminResources))]
        public EntityHeader HelpResourceGuide { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.HelpResource_Link, FieldType: FieldTypes.WebLink, ResourceType: typeof(UserAdminResources))]
        public string Link { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.HelpResource_Summary, FieldType: FieldTypes.MultiLineText, ResourceType: typeof(UserAdminResources))]
        public string Summary { get; set; }
    }
}
