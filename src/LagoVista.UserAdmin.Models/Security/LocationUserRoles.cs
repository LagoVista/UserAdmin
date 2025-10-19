// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 31877a9b25595997079ce04c1f51bb616fdcc5e5ccb6b632a82073d59953ba22
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Resources;
using LagoVista.UserAdmin.Resources;

namespace LagoVista.UserAdmin.Models.Security
{
    [EntityDescription(Domains.SecurityDomain, UserAdminResources.Names.LocationUserRole_Title, UserAdminResources.Names.LocationUserRole_Help, UserAdminResources.Names.LocationUserRole_Description, EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources))]
    public class LocationUserRole : TableStorageEntity
    {
        public LocationUserRole(EntityHeader location, EntityHeader user)
        {                        
            LocationId = location.Id;
            LocationName = location.Text;
            UserId = user.Id;
            UserName = user.Text;
            PartitionKey = LocationId;
            RowKey = GetRowKey();
        }

        public string LocationId { get; set; }
        public string LocationName { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }
        public string RoleName { get; set; }
        public string RoleId { get; set; }

        public string GetRowKey(string locationId, string userId)
        {
            return $"{locationId}.{userId}";
        }

        public string GetRowKey()
        {
            return $"{LocationId}.{UserId}";
        }
    }
}
