// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: c683947355673de99b3f213ad1a5b845b8d9ae4a097586fc6442f3057acd94e0
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
        Domains.OrganizationDomain, UserAdminResources.Names.UserFavorites_Name, UserAdminResources.Names.UserFavorites_Help,
        UserAdminResources.Names.UserFavorites_Description, EntityDescriptionAttribute.EntityTypes.OrganizationModel, typeof(UserAdminResources),

        ClusterKey: "favorites", ModelType: EntityDescriptionAttribute.ModelTypes.Configuration, Lifecycle: EntityDescriptionAttribute.Lifecycles.DesignTime,
        Sensitivity: EntityDescriptionAttribute.Sensitivities.Internal, IndexInclude: true, IndexTier: EntityDescriptionAttribute.IndexTiers.Secondary,
        IndexPriority: 60, IndexTagsCsv: "organizationdomain,favorites,configuration")]
    public class UserFavorites : UserAdminModelBase, IKeyedEntity, INamedEntity, IValidateable, IOwnedEntity
    {
        public UserFavorites()
        {
            Id = Guid.NewGuid().ToId();
            Favorites = new List<UserFavorite>();
            Modules = new List<FavoritesByModule>();
            Key = Id.ToLower();
        }

        public List<UserFavorite> Favorites { get; set; }

        public List<FavoritesByModule> Modules { get; set; }

        public static string GenerateId(EntityHeader org, EntityHeader user)
        {
            return $"{nameof(UserFavorites).ToLower()}-{org.Id}-{user.Id}";
        }

        public static string GenerateId(string orgId, string userId)
        {
            return $"{nameof(UserFavorites).ToLower()}-{orgId}-{userId}";
        }
    }

    [EntityDescription(
        Domains.OrganizationDomain,
        UserAdminResources.Names.FavoritesByModule_Name,
        UserAdminResources.Names.FavoritesByModule_Help,
        UserAdminResources.Names.FavoritesByModule_Description,
        EntityDescriptionAttribute.EntityTypes.OrganizationModel,
        typeof(UserAdminResources))]
    public class FavoritesByModule
    {
        public FavoritesByModule()
        {
            Id = Guid.NewGuid().ToId();
            Items = new List<UserFavorite>();
        }

        public string Id { get; set; }
        public string ModuleKey { get; set; }
        public List<UserFavorite> Items { get; set; }
    }

    [EntityDescription(
        Domains.OrganizationDomain,
        UserAdminResources.Names.UserFavorite_Name,
        UserAdminResources.Names.UserFavorite_Help,
        UserAdminResources.Names.UserFavorite_Description,
        EntityDescriptionAttribute.EntityTypes.OrganizationModel,
        typeof(UserAdminResources))]
    public class UserFavorite
    {
        public UserFavorite()
        {
            Id = Guid.NewGuid().ToId();
            DateAdded = DateTime.UtcNow.ToJSONString();
            LastAccessed = DateTime.UtcNow.ToJSONString();
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
