// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 96fa1b340cf8c42c73925f6a544ff4c45b27a08dd1c94e8a088c005658d00168
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.DTOs;

namespace LagoVista.UserAdmin.Models.Security
{
    public enum AccessType
    {
        Module,
        Area,
        Page,
        Feature
    }

    public class RoleAccess
    {
        public string Id { get; set; }
        public bool IsPublic { get; set; }
        public EntityHeader Role { get; set; }
        public EntityHeader Organization { get; set; }
        public string CreationDate { get; set; }
        public EntityHeader CreatedBy { get; set; }

        public EntityHeader Module { get; set; }
        public EntityHeader Area { get; set; }
        public EntityHeader Page { get; set; }
        public EntityHeader Feature { get; set; }

        public EntityHeader FunctionMap { get; set; }
        public EntityHeader FunctionMapFunction { get; set; }

        public int Create { get; set; }
        public int Read { get; set; }
        public int Update { get; set; }
        public int Delete { get; set; }
        public string Other { get; set; }


        public RoleAccessDTO ToDTO()
        {
            return new RoleAccessDTO()
            {
                RowKey = Id,
                PartitionKey = Organization.Id,

                RoleId = Role.Id,
                RoleKey = Role.Key,
                RoleName = Role.Text,

                OrganizationId = Organization.Id,
                OrganizationName = Organization.Text,

                ModuleId = Module?.Id,
                ModuleKey = Module?.Key,
                ModuleName = Module?.Text,

                AreaId = Area?.Id,
                AreaKey = Area?.Key,
                AreaName = Area?.Text,

                PageId = Page?.Id,
                PageKey = Page?.Key,
                PageName = Page?.Text,

                FeatureId = Feature?.Id,
                FeatureKey = Feature?.Key,
                FeatureName = Feature?.Text,

                FunctionMapId = FunctionMap?.Id,
                FunctionMapKey = FunctionMap?.Key,
                FunctionMapName = FunctionMap?.Text,

                FunctionMapFunctionId = FunctionMapFunction?.Id,
                FunctionMapFunctionKey = FunctionMapFunction?.Key,
                FunctionMapFunctionName = FunctionMapFunction?.Text,

                IsPublic = IsPublic,

                CreatedById = CreatedBy.Id,
                CreatedByName = CreatedBy.Text,

                Create = Create,
                Read = Read,
                Update = Update,
                Delete = Delete,
                Other = Other
            };
        }

        public UserAccess ToUserAccess()
        {
            return new UserAccess()
            {
                Create = Create,
                Read = Read,
                Update = Update,
                Delete = Delete,
            };
        }


        private int? Calculate(int? existingLevel, int? newLevel)
        {
            if (!existingLevel.HasValue || existingLevel.Value == UserAccess.NotSpecified)
                return newLevel.Value;


            return newLevel.Value;
        }

        public UserAccess Merge(UserAccess access)
        {
            access.Create = Calculate(access.Create, Create);
            access.Read = Calculate(access.Read, Read);
            access.Update = Calculate(access.Update, Update);
            access.Delete = Calculate(access.Delete, Delete);
            return access;
        }
    }
}
