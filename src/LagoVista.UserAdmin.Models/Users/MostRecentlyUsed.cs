﻿using LagoVista.Core;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using System;
using System.Collections.Generic;

namespace LagoVista.UserAdmin.Models.Users
{
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
