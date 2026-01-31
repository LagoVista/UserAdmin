// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 72f5b3306a46a353d10ca160ea61bf6acf378a156ea1d50582b38dfde933f11e
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Resources;
using System;
using System.Collections.Generic;

namespace LagoVista.UserAdmin.Models.Users
{
    [EntityDescription(
        Domains.UserDomain,
        UserAdminResources.Names.MostRecentlyUsed_Name,
        UserAdminResources.Names.MostRecentlyUsed_Help,
        UserAdminResources.Names.MostRecentlyUsed_Description,
        EntityDescriptionAttribute.EntityTypes.OrganizationModel,
        typeof(UserAdminResources))]
    public class MostRecentlyUsed : UserAdminModelBase, IKeyedEntity, INamedEntity, IValidateable, IOwnedEntity
    {
        public MostRecentlyUsed()
        {
            Id = Guid.NewGuid().ToId();
            Key = Guid.NewGuid().ToId().ToLower();
            All = new List<MostRecentlyUsedItem>();
            Modules = new List<MostRecentlyUsedModule>();
        }

        public List<MostRecentlyUsedItem> All { get; set; }
        public List<MostRecentlyUsedModule> Modules { get; set; }

        public static string GenerateId(EntityHeader org, EntityHeader user)
        {
            return $"{nameof(MostRecentlyUsed).ToLower()}-key-{org.Id}-{user.Id}";
        }

        public static string GenerateId(string orgId, string userId)
        {
            return $"{nameof(MostRecentlyUsed).ToLower()}-key-{orgId}-{userId}";
        }
    }

    [EntityDescription(
        Domains.UserDomain,
        UserAdminResources.Names.MostRecentlyUsedModule_Name,
        UserAdminResources.Names.MostRecentlyUsedModule_Help,
        UserAdminResources.Names.MostRecentlyUsedModule_Description,
        EntityDescriptionAttribute.EntityTypes.OrganizationModel,
        typeof(UserAdminResources))]
    public class MostRecentlyUsedModule
    {
        public MostRecentlyUsedModule()
        {
            Id = Guid.NewGuid().ToId();
            Items = new List<MostRecentlyUsedItem>();
        }

        public string Id { get; set; }
        public string ModuleKey { get; set; }
        public List<MostRecentlyUsedItem> Items { get; set; }
    }

    [EntityDescription(
        Domains.UserDomain,
        UserAdminResources.Names.MostRecentlyUsedItem_Name,
        UserAdminResources.Names.MostRecentlyUsedItem_Help,
        UserAdminResources.Names.MostRecentlyUsedItem_Description,
        EntityDescriptionAttribute.EntityTypes.OrganizationModel,
        typeof(UserAdminResources))]
    public class MostRecentlyUsedItem
    {
        public MostRecentlyUsedItem()
        {
            Id = Guid.NewGuid().ToId();
            DateAdded = DateTime.UtcNow.ToJSONString();
            Type = "Module";
        }

        public string Id { get; set; }
        public string ModuleKey { get; set; }
        public string DateAdded { get; set; }
        public string LastAccessed { get; set; }
        public List<string> Route { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Summary { get; set; }
        public string Icon { get; set; }
        public string Link { get; set; }
    }
}
