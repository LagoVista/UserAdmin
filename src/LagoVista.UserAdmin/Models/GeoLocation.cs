using LagoVista.Core.Attributes;
using LagoVista.Core.Geo;
using LagoVista.UserAdmin.Resources;

namespace LagoVista.UserAdmin.Models
{
    [EntityDescription(Domains.MiscDomain, UserAdminResources.Names.GeoLocation_Title, UserAdminResources.Names.GeoLocation_Help, UserAdminResources.Names.GeoLocation_Description, EntityDescriptionAttribute.EntityTypes.SimpleModel, typeof(UserAdminResources))]
    public class GeoLocation : IGeoLocation
    {
        public double Altitude { get; set; }

        public string LastUpdated { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
