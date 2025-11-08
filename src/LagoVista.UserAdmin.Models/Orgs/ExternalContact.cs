// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 8fe00d472356578f623a1e4fe5bfaf266e52d536472c93378d29367dd4537763
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Resources;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;

namespace LagoVista.UserAdmin.Models.Orgs
{

    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.ExternalContact_Title, UserAdminResources.Names.ExternalContact_Description,
    UserAdminResources.Names.Organization_Description, EntityDescriptionAttribute.EntityTypes.OrganizationModel, typeof(UserAdminResources),
    FactoryUrl: "/api/distro/externalcontact/factory", Icon: "icon-pz-skill")]
    public class ExternalContact : IIDEntity, IValidateable, IFormDescriptor, IFormConditionalFields
    {
        public ExternalContact()
        {
            Id = Guid.NewGuid().ToId();
        }

        public string Id { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUser_FirstName, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string FirstName { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUser_LastName, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string LastName { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.ExternalContact_SendEmail, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool SendEmail { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.ExternalContact_SendSMS, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool SendSMS { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUser_Email, ResourceType: typeof(UserAdminResources))]
        public string Email { get; set; } 

        [FormField(LabelResource: UserAdminResources.Names.AppUser_PhoneNumber, ResourceType: typeof(UserAdminResources))]
        public string Phone { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Common_Notes, FieldType:FieldTypes.MultiLineText, ResourceType: typeof(UserAdminResources))]
        public string Notes { get; set; }

        public string EmailConfirmedTimeStamp { get; set; }

        public string SmsConfirmedTimeStamp { get; set; }

        public FormConditionals GetConditionalFields()
        {
            return new FormConditionals()
            {
                ConditionalFields = new List<string>() { nameof(Email), nameof(Phone) },
                Conditionals = new List<FormConditional>()
                {
                    new FormConditional()
                    {
                         Field = nameof(SendSMS),
                         Value = "true",
                         RequiredFields = new List<string>() { nameof(Phone) },
                         VisibleFields = new List<string>() { nameof(Phone) },
                    },
                    new FormConditional()
                    {
                         Field = nameof(SendEmail),
                         Value = "true",
                         RequiredFields = new List<string>() { nameof(Email) },
                         VisibleFields = new List<string>() { nameof(Email) },
                    },

                }
            };
        }

        [CustomValidator]
        public void Validate(ValidationResult result, Actions action)
        {
            if (SendEmail && String.IsNullOrEmpty(Email))
            {
                result.AddUserError("To send an email to this contact you must provide their email address.");
            }

            if(SendSMS && String.IsNullOrEmpty(Phone))
            {
                result.AddUserError("To send an SMS message to this contact you must provide their phone number.");
            }            
        }



        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(FirstName),
                nameof(LastName),
                nameof(SendEmail),
                nameof(Email),
                nameof(SendSMS),
                nameof(Phone),
                nameof(Notes)
            };
        }
    }
}
