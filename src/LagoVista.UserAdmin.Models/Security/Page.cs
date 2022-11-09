﻿using LagoVista.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.Security
{
    [EntityDescription(Domains.SecurityDomain, UserAdminResources.Names.Page_Title, UserAdminResources.Names.Page_Help, UserAdminResources.Names.Page_Help, EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources))]
    public class Page
    {
        public Page()
        {
            Features = new List<Feature>();
            Status = EntityHeader<ModuleStatus>.Create(ModuleStatus.Development);
            Id = Guid.NewGuid().ToId();
        }

        public int SortOrder { get; set; }

        public string Id { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Common_Name, IsRequired: true, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources))]
        public string Name { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Key, HelpResource: UserAdminResources.Names.Common_Key_Help, FieldType: FieldTypes.Key,
            RegExValidationMessageResource: UserAdminResources.Names.Common_Key_Validation, ResourceType: typeof(UserAdminResources), IsRequired: true)]
        public string Key { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Description, IsRequired: false, FieldType: FieldTypes.MultiLineText, ResourceType: typeof(UserAdminResources))]
        public string Description { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Module_CardIcon, IsRequired: true, FieldType: FieldTypes.Icon, ResourceType: typeof(UserAdminResources))]
        public string CardIcon { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Module_CardTitle, IsRequired: true, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources))]
        public string CardTitle { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Module_CardSummary, IsRequired: true, FieldType: FieldTypes.MultiLineText, ResourceType: typeof(UserAdminResources))]
        public string CardSummary { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Status, IsRequired: true, FieldType: FieldTypes.Picker, EnumType: typeof(ModuleStatus), WaterMark: UserAdminResources.Names.ModuleStatus_Select, ResourceType: typeof(UserAdminResources))]
        public EntityHeader<ModuleStatus> Status { get; set; }


        public List<Feature> Features { get; set; } = new List<Feature>();
    }
}
