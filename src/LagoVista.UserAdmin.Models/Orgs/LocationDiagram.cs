using LagoVista.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Resources;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.Orgs
{
    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.LocationDiagram_Title, UserAdminResources.Names.LocationDiagrams_Description, UserAdminResources.Names.LocationDiagrams_Description, EntityDescriptionAttribute.EntityTypes.SimpleModel, typeof(UserAdminResources), Icon:"ico-pz-worldwide-1",
               GetListUrl:"/api/org/location/diagrams", GetUrl:"/api/org/location/diagram/{id}", SaveUrl: "/api/org/location/diagram", FactoryUrl:"/api/org/location/diagram")]
    public class LocationDiagram : EntityBase, IValidateable, ISummaryFactory, IFormDescriptor
    {


        [FormField(LabelResource: Resources.UserAdminResources.Names.Common_Icon, FieldType: FieldTypes.Icon, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public string Icon { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Notes, FieldType: FieldTypes.HtmlEditor, ResourceType: typeof(UserAdminResources))]
        public string Notes { get; set; }

        [FormField(LabelResource: Resources.UserAdminResources.Names.LocationDiagram_BaseLocation, HelpResource: Resources.UserAdminResources.Names.LocationDiagram_BaseLocation_Help, 
           WaterMark:UserAdminResources.Names.Common_SelectLocation,  FieldType: FieldTypes.OrgLocationPicker, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public EntityHeader Location { get; set; }


        public int InitialZoom { get; set; }
        public int InitialPanX { get; set; }
        public int InitialPanY { get; set; }

        public List<LocationDiagramShape> Shapes { get; set; } = new List<LocationDiagramShape>();

        public LocationDiagramSummary CreateSummary()
        {
            return new LocationDiagramSummary()
            {
                Id = Id,
                Name = Name,
                Key = Key,
                Location = Location,
                IsPublic = false
            };
        }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(Icon),
                nameof(Location),                
                nameof(Notes),                
            };
        }

        ISummaryData ISummaryFactory.CreateSummary()
        {
            return CreateSummary();
        }
    }

    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.LocationDiagram_Shape, UserAdminResources.Names.LocationDiagram_Shape_Help, UserAdminResources.Names.LocationDiagram_Shape_Help,
        EntityDescriptionAttribute.EntityTypes.SimpleModel, typeof(UserAdminResources), Icon: "ico-pz-worldwide-1",FactoryUrl: "/api/org/location/diagram/shape/factory")]
    public class LocationDiagramShape : IFormDescriptor
    {
        public LocationDiagramShape()
        {
            Id = Guid.NewGuid().ToId();
        }

        public string Id { get; set; }

        [FormField(LabelResource: LagoVistaCommonStrings.Names.Common_Name, FieldType: FieldTypes.Text, ResourceType: typeof(LagoVistaCommonStrings), IsRequired: true, IsUserEditable: true)]
        public string Name { get; set; }

        [FormField(LabelResource: LagoVistaCommonStrings.Names.Common_Key, HelpResource: LagoVistaCommonStrings.Names.Common_Key_Help, FieldType: FieldTypes.Key,
            RegExValidationMessageResource: LagoVistaCommonStrings.Names.Common_Key_Validation, ResourceType: typeof(LagoVistaCommonStrings), IsRequired: true)]
        public string Key { get; set; }


        [FormField(LabelResource: Resources.UserAdminResources.Names.Common_Icon, FieldType: FieldTypes.Icon, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public string Icon { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.LocationDiagram_Shape_Location, WaterMark: UserAdminResources.Names.Common_SelectLocation,
            HelpResource:UserAdminResources.Names.LocationDiagram_Shape_Location_Help, FieldType: FieldTypes.OrgLocationPicker, ResourceType: typeof(UserAdminResources), IsUserEditable: true)]
        public EntityHeader Location { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Common_Notes, FieldType: FieldTypes.HtmlEditor, ResourceType: typeof(UserAdminResources))]
        public string Notes { get; set; }

        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public List<int> Points { get; set; } = new List<int>();

        public LocationDiagram Details { get; set; }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(Icon),
                nameof(Location),
                nameof(Notes),
            };
        }
    }

    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.LocationDiagrams_Title, UserAdminResources.Names.LocationDiagrams_Description, UserAdminResources.Names.LocationDiagrams_Description, EntityDescriptionAttribute.EntityTypes.SimpleModel, typeof(UserAdminResources), Icon: "ico-pz-worldwide-1",
               GetListUrl: "/api/org/location/diagrams", GetUrl: "/api/org/location/diagram/{id}", SaveUrl: "/api/org/location/diagram", FactoryUrl: "/api/org/location/diagram")]
    public class LocationDiagramSummary : SummaryData
    {
        public EntityHeader Location { get; set; }
    }
}
