using System;
using System.Collections.Generic;
using LagoVista.Core.Models;
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using Newtonsoft.Json;
using LagoVista.Core.Validation;
using LagoVista.Core;
using LagoVista.Core.Authentication.Models;
using LagoVista.UserAdmin.Resources;

namespace LagoVista.UserAdmin.Models.Users
{
    [EntityDescription(Domains.UserDomain, UserAdminResources.Names.AppUser_Title, UserAdminResources.Names.AppUser_Help, UserAdminResources.Names.AppUser_Description, EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources))]
    public class AppUser : UserAdminModelBase, INamedEntity, IValidateable, IOwnedEntity
    {
        public AppUser(String email, String createdBy)
        {
            Id = Guid.NewGuid().ToId();
            Email = email;
            UserName = email;
            CreatedBy = new EntityHeader()
            {
                Id = Id,
                Text = createdBy
            };
            CreationDate = DateTime.Now.ToJSONString();

            LastUpdatedBy = new EntityHeader()
            {
                Id = Id,
                Text = createdBy
            };

            LastUpdatedDate = DateTime.Now.ToJSONString();

            ProfileImageUrl = new ImageDetails()
            {
                Width = 128,
                Height = 128,
                ImageUrl = "https://bytemaster.blob.core.windows.net/userprofileimages/watermark.png",
                Id = "b78ca749a1e64ce59df4aa100050dcc7"
            };

            Organizations = new List<EntityHeader>();
            CurrentOrganizationRoles = new List<EntityHeader>();
        }

        public AppUser()
        {

        }

        public List<EntityHeader> Organizations { get; set; }
        public EntityHeader CurrentOrganization { get; set; }
        public List<EntityHeader> CurrentOrganizationRoles { get; set; }

        public ImageDetails ProfileImageUrl { get; set; }

        private string _email;
        [FormField(LabelResource: Resources.UserAdminResources.Names.AppUser_Email, IsRequired: true, FieldType: FieldTypes.Email, ResourceType: typeof(Resources.UserAdminResources))]
        public string Email
        {
            get { return _email; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    _email = null;
                }
                else
                {
                    _email = value.ToUpper();
                }
            }
        }
        public bool EmailConfirmed { get; set; }

        [FormField(LabelResource: Resources.UserAdminResources.Names.AppUser_FirstName, IsRequired: true, ResourceType: typeof(Resources.UserAdminResources))]
        public string FirstName { get; set; }
        [FormField(LabelResource: Resources.UserAdminResources.Names.AppUser_LastName, IsRequired: true, ResourceType: typeof(Resources.UserAdminResources))]
        public string LastName { get; set; }

        [FormField(LabelResource: Resources.UserAdminResources.Names.AppUser_LastName, IsRequired: true, ResourceType: typeof(Resources.UserAdminResources))]
        public bool IsSystemAdmin { get; set; }

        [FormField(LabelResource: Resources.UserAdminResources.Names.AppUser_PhoneNumber, FieldType: FieldTypes.Phone, ResourceType: typeof(Resources.UserAdminResources))]
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }

        [JsonIgnore()]
        public String Name
        {
            get { return $"{FirstName} {LastName}"; }
        }


        public string UserName { get; set; }

        public EntityHeader ToEntityHeader()
        {
            return EntityHeader.Create(Id, $"{FirstName} ${LastName}");
        }

        public int AccessFailedCount { get; set; }
        public string LockoutDate { get; set; }
        public bool LockoutEnabled { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public bool TwoFactorEnabled { get; set; }

        public IList<ThirdPartyLoginInfo> Logins { get; set; }
        public bool IsPublic { get; set; }
        public EntityHeader OwnerOrganization { get; set; }
        public EntityHeader OwnerUser { get; set; }

        public UserInfo ToUserInfo()
        {
            return new UserInfo()
            {
                IsSystemAdmin = IsSystemAdmin,
                FirstName = FirstName,
                LastName = LastName,
                CreatedBy = CreatedBy,
                CreationDate = CreationDate,
                CurrentOrganization = CurrentOrganization,
                Email = Email,
                EmailConfirmed = EmailConfirmed,
                Id = Id,
                LastUpdatedBy = LastUpdatedBy,
                LastUpdatedDate = LastUpdatedDate,
                PhoneNumber = PhoneNumber,
                PhoneNumberConfirmed = PhoneNumberConfirmed,
                ProfileImageUrl = ProfileImageUrl
            };
        }

        public UserInfoSummary ToUserInfoSummary()
        {
            return new UserInfoSummary()
            {
                Email = Email,
                EmailConfirmed = EmailConfirmed,
                Id = Id,
                IsSystemAdmin = IsSystemAdmin,
                Name = Name,
                PhoneNumberConfirmed = PhoneNumberConfirmed,
                ProfileImageUrl = ProfileImageUrl,
                Key = Id,
            };
        }
    }
}
