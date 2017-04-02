using LagoVista.Core.Attributes;
using LagoVista.UserAdmin.Resources;
using Newtonsoft.Json;
using System;

namespace LagoVista.UserAdmin.Models
{
    [EntityDescription(Domains.MiscDomain, UserAdminResources.Names.ImageDetails_Title, UserAdminResources.Names.ImageDetails_Description, UserAdminResources.Names.LocationAccountRole_Description, EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources))]
    public class ImageDetails
    {
        [JsonProperty("id")]
        public String Id { get; set; }
        public String ImageUrl { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
