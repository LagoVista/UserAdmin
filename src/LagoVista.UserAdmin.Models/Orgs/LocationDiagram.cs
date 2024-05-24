using LagoVista.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Resources;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Calendar;
using LagoVista.UserAdmin.Models.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.Orgs
{
    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.LocationDiagram_Title, UserAdminResources.Names.LocationDiagrams_Description, UserAdminResources.Names.LocationDiagrams_Description, EntityDescriptionAttribute.EntityTypes.SimpleModel, typeof(UserAdminResources), Icon: "icon-fo-gallery-1",
               GetListUrl: "/api/org/location/diagrams", GetUrl: "/api/org/location/diagram/{id}", SaveUrl: "/api/org/location/diagram", FactoryUrl: "/api/org/location/diagram")]
    public class LocationDiagram : EntityBase, IValidateable, ISummaryFactory, IFormDescriptor
    {
        public LocationDiagram()
        {
            Icon = "icon-fo-gallery-1";
            Width = 1024;
            Height = 1024;
        }

        [FormField(LabelResource: Resources.UserAdminResources.Names.Common_Icon, FieldType: FieldTypes.Icon, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public string Icon { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Notes, FieldType: FieldTypes.HtmlEditor, ResourceType: typeof(UserAdminResources))]
        public string Notes { get; set; }

        [FormField(LabelResource: Resources.UserAdminResources.Names.LocationDiagram_BaseLocation, HelpResource: Resources.UserAdminResources.Names.LocationDiagram_BaseLocation_Help,
           WaterMark: UserAdminResources.Names.Common_SelectLocation, FieldType: FieldTypes.OrgLocationPicker, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public EntityHeader Location { get; set; }


        public double InitialZoom { get; set; }
        public int InitialPanX { get; set; }
        public int InitialPanY { get; set; }

        [FormField(LabelResource: Resources.UserAdminResources.Names.LocationDiagram_Width, HelpResource: Resources.UserAdminResources.Names.LocationDiagram_BaseLocation_Help,
            FieldType: FieldTypes.Decimal, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public double Width { get; set; }

        [FormField(LabelResource: Resources.UserAdminResources.Names.LocationDiagram_Height, HelpResource: Resources.UserAdminResources.Names.LocationDiagram_BaseLocation_Help,
            FieldType: FieldTypes.Decimal, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public double Height { get; set; }

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
                nameof(Width),
                nameof(Height),
                nameof(Location),
                nameof(Notes),
            };
        }

        ISummaryData ISummaryFactory.CreateSummary()
        {
            return CreateSummary();
        }
    }

    public enum ShapeTypes
    {
        [EnumLabel(LocationDiagramShape.ShapeType_Room, UserAdminResources.Names.ShapeType_Room, typeof(UserAdminResources))]
        Room,
        [EnumLabel(LocationDiagramShape.ShapeType_Door, UserAdminResources.Names.ShapeType_Door, typeof(UserAdminResources))]
        Door,
        [EnumLabel(LocationDiagramShape.ShapeType_Closet, UserAdminResources.Names.ShapeType_Closet, typeof(UserAdminResources))]
        Closet,
        [EnumLabel(LocationDiagramShape.ShapeType_Window, UserAdminResources.Names.ShapeType_Window, typeof(UserAdminResources))]
        Window,
        [EnumLabel(LocationDiagramShape.ShapeType_ExternalEntrance, UserAdminResources.Names.ShapeType_ExternalEntrance, typeof(UserAdminResources))]
        ExternalEntrance,
        [EnumLabel(LocationDiagramShape.ShapeType_ParkingLot, UserAdminResources.Names.ShapeType_ParkingLot, typeof(UserAdminResources))]
        ParkingLot,
        [EnumLabel(LocationDiagramShape.ShapeType_Circle, UserAdminResources.Names.ShapeType_Circle, typeof(UserAdminResources))]
        Circle,
    }


    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.LocationDiagram_Shape, UserAdminResources.Names.LocationDiagram_Shape_Help, UserAdminResources.Names.LocationDiagram_Shape_Help,
        EntityDescriptionAttribute.EntityTypes.SimpleModel, typeof(UserAdminResources), Icon: "icon-ae-ecommerce-1", FactoryUrl: "/api/org/location/diagram/shape/factory")]
    public class LocationDiagramShape : IFormDescriptor
    {
        public const string ShapeType_Room = "room";
        public const string ShapeType_ParkingLot = "parkinglot";
        public const string ShapeType_ExternalEntrance = "exteranlentrance";
        public const string ShapeType_Door = "door";
        public const string ShapeType_Window = "window";
        public const string ShapeType_Closet = "closet";
        public const string ShapeType_Circle = "circle";

        public LocationDiagramShape()
        {
            Id = Guid.NewGuid().ToId();
            Icon = "icon-ae-ecommerce-1";
            X = 200;
            Y = 200;
            Width = 200;
            Height = 200;
            Fill = "#f8f8f8";
            Scale = 1;
            Stroke = "#000000";
            ShapeType = EntityHeader<ShapeTypes>.Create(ShapeTypes.Room);
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
            HelpResource: UserAdminResources.Names.LocationDiagram_Shape_Location_Help, FieldType: FieldTypes.OrgLocationPicker, ResourceType: typeof(UserAdminResources), IsUserEditable: true)]
        public EntityHeader Location { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Common_Notes, FieldType: FieldTypes.HtmlEditor, ResourceType: typeof(UserAdminResources))]
        public string Notes { get; set; }

        [FormField(LabelResource: Resources.UserAdminResources.Names.LocationDiagram_Left, HelpResource: Resources.UserAdminResources.Names.LocationDiagram_BaseLocation_Help,
            FieldType: FieldTypes.Decimal, ResourceType: typeof(UserAdminResources), IsRequired: true, IsUserEditable: true)]
        public double X { get; set; }

        [FormField(LabelResource: Resources.UserAdminResources.Names.LocationDiagram_Top, HelpResource: Resources.UserAdminResources.Names.LocationDiagram_BaseLocation_Help,
            FieldType: FieldTypes.Decimal, ResourceType: typeof(UserAdminResources), IsRequired: true, IsUserEditable: true)]
        public double Y { get; set; }
        [FormField(LabelResource: Resources.UserAdminResources.Names.LocationDiagram_Width, HelpResource: Resources.UserAdminResources.Names.LocationDiagram_BaseLocation_Help,
            FieldType: FieldTypes.Decimal, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public double Width { get; set; }

        [FormField(LabelResource: Resources.UserAdminResources.Names.LocationDiagram_Height, HelpResource: Resources.UserAdminResources.Names.LocationDiagram_BaseLocation_Help,
                FieldType: FieldTypes.Decimal, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public double Height { get; set; }

        [FormField(LabelResource: Resources.UserAdminResources.Names.Shape_FlipX, HelpResource: Resources.UserAdminResources.Names.LocationDiagram_BaseLocation_Help,
                FieldType: FieldTypes.Bool, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public bool FlipX { get; set; }

        [FormField(LabelResource: Resources.UserAdminResources.Names.Shape_FlipY, HelpResource: Resources.UserAdminResources.Names.LocationDiagram_BaseLocation_Help,
                FieldType: FieldTypes.Bool, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public bool FlipY { get; set; }

        [FormField(LabelResource: Resources.UserAdminResources.Names.ShapeType, WaterMark:UserAdminResources.Names.ShapeType_Select, EnumType:typeof(ShapeTypes), 
                FieldType: FieldTypes.Picker, ResourceType: typeof(UserAdminResources), IsRequired: true, IsUserEditable: true)]
        public EntityHeader<ShapeTypes> ShapeType { get; set; }

        [FormField(LabelResource: Resources.UserAdminResources.Names.Shape_Rotation, FieldType: FieldTypes.Decimal, ResourceType: typeof(UserAdminResources), IsRequired: true, IsUserEditable: true)]
        public double Rotation { get; set; }

        [FormField(LabelResource: Resources.UserAdminResources.Names.Shape_Scale, FieldType: FieldTypes.Decimal, ResourceType: typeof(UserAdminResources), IsRequired: true, IsUserEditable: true)]
        public double Scale { get; set; }

        [FormField(LabelResource: Resources.UserAdminResources.Names.Shape_Stroke, FieldType: FieldTypes.Color, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public string Stroke { get; set; }

        [FormField(LabelResource: Resources.UserAdminResources.Names.Shape_Fill, FieldType: FieldTypes.Color, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public string Fill { get; set; }
        public List<int> Points { get; set; } = new List<int>();

        public LocationDiagram Details { get; set; }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(Icon),
                nameof(ShapeType),
                nameof(X),
                nameof(Y),
                nameof(Width),
                nameof(Height),
                nameof(Location),
                nameof(Rotation),
                nameof(Scale),
                nameof(Stroke),   
                nameof(Fill),
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
