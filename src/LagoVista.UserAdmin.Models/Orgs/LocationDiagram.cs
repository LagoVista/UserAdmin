// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 79c8b31fb5643adda293fe954f39b35b921c57fd3adcc2e10ba104c6f2cd0ffd
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Models.Geo;
using LagoVista.Core.Resources;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Calendar;
using LagoVista.UserAdmin.Models.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace LagoVista.UserAdmin.Models.Orgs
{
    public enum DiagramUnits
    {
        [EnumLabel(LocationDiagram.DiagramUnits_Feet,  Resources.UserAdminResources.Names.DiagramUnits_Feet, typeof(Resources.UserAdminResources))]
        Feet,

        [EnumLabel(LocationDiagram.DiagramUnits_Meters, Resources.UserAdminResources.Names.DiagramUnits_Meters, typeof(Resources.UserAdminResources))]
        Meters
    }

    public enum VerticalAlign
    {
        [EnumLabel(LocationDiagram.VerticalAlign_Top, Resources.UserAdminResources.Names.VerticalAlign_Top, typeof(Resources.UserAdminResources))]
        Top,
        [EnumLabel(LocationDiagram.VerticalAlign_Middle, Resources.UserAdminResources.Names.VerticalAlign_Middle, typeof(Resources.UserAdminResources))]
        Middle,
        [EnumLabel(LocationDiagram.VerticalAlign_Bottom, Resources.UserAdminResources.Names.VerticalAlign_Bottom, typeof(Resources.UserAdminResources))]
        Bottom
    }

    public enum HorizontalAlign
    {
        [EnumLabel(LocationDiagram.HorizontalAlign_Left, Resources.UserAdminResources.Names.HorizontalAlign_Left, typeof(Resources.UserAdminResources))]
        Top,
        [EnumLabel(LocationDiagram.HorizontalAlign_Center, Resources.UserAdminResources.Names.HorizontalAlign_Center, typeof(Resources.UserAdminResources))]
        Center,
        [EnumLabel(LocationDiagram.HorizontalAlign_Right, Resources.UserAdminResources.Names.HorizontalAlign_Right, typeof(Resources.UserAdminResources))]
        Bottom
    }

    [EntityDescription(Domains.OrgLocations, UserAdminResources.Names.LocationDiagram_Title, UserAdminResources.Names.LocationDiagrams_Description, UserAdminResources.Names.LocationDiagrams_Description,
        EntityDescriptionAttribute.EntityTypes.SimpleModel, typeof(UserAdminResources), Icon: "icon-pz-worldwide-1",
        GetListUrl: "/api/org/location/diagrams", DeleteUrl: "/api/org/location/diagram/{id}", 
        GetUrl: "/api/org/location/diagram/{id}", SaveUrl: "/api/org/location/diagram", FactoryUrl: "/api/org/location/diagram")]
    public class LocationDiagram : EntityBase, IValidateable, ISummaryFactory, IFormDescriptor
    {
        public const string DiagramUnits_Feet = "feet";
        public const string DiagramUnits_Meters = "meters";

        public const string VerticalAlign_Top = "top";
        public const string VerticalAlign_Middle = "middle";
        public const string VerticalAlign_Bottom = "bottom";

        public const string HorizontalAlign_Left = "left";
        public const string HorizontalAlign_Center = "center";
        public const string HorizontalAlign_Right = "right";


        public LocationDiagram()
        {
            Icon = "icon-pz-worldwide-1";
            Width = 100*12;
            Height = 100*12;
            Fill = "#f8f8f8";
            Stroke = "#000000";
            TextColor = "#000000";
            Units = EntityHeader<DiagramUnits>.Create(DiagramUnits.Feet);
        }

        [FormField(LabelResource: Resources.UserAdminResources.Names.Common_Icon, FieldType: FieldTypes.Icon, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public string Icon { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Notes, FieldType: FieldTypes.HtmlEditor, ResourceType: typeof(UserAdminResources))]
        public string Notes { get; set; }

        [FormField(LabelResource: Resources.UserAdminResources.Names.LocationDiagram_BaseLocation, HelpResource: Resources.UserAdminResources.Names.LocationDiagram_BaseLocation_Help, EntityHeaderPickerUrl: "/api/org/locations",
           WaterMark: UserAdminResources.Names.Common_SelectLocation, FieldType: FieldTypes.EntityHeaderPicker, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public EntityHeader Location { get; set; }

        public GeoLocation GeoLocationCenter { get; set; }

        public double GeoMetersPerUnit { get; set; }

        public List<GeoLocation> GeoPoints { get; set; } = new List<GeoLocation>();

        public decimal DefaultGeoZoomLevel { get; set; } = 10;

        public decimal DefaultShapeZoomLevel { get; set; } = 1;

        public double ViewPortX { get; set; } = 20;
        public double ViewPortY { get; set; } = 20;


        [FormField(LabelResource: Resources.UserAdminResources.Names.Diagram_Units, FieldType: FieldTypes.Picker, ResourceType: typeof(UserAdminResources), EnumType: typeof(DiagramUnits), WaterMark: UserAdminResources.Names.Diagram_Units_Select, IsRequired: true, IsUserEditable: true)]
        public EntityHeader<DiagramUnits> Units { get; set; }



        [FormField(LabelResource: Resources.UserAdminResources.Names.LocationDiagram_Width, FieldType: FieldTypes.Decimal, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public double Width { get; set; }

        [FormField(LabelResource: Resources.UserAdminResources.Names.LocationDiagram_Height, FieldType: FieldTypes.Decimal, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public double Height { get; set; }

        public double Scale { get; set; }

        public double Rotation { get; set; }


        [FormField(LabelResource: Resources.UserAdminResources.Names.Common_Customer, FieldType: FieldTypes.Color, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public EntityHeader Customer { get; set; }


        [FormField(LabelResource: Resources.UserAdminResources.Names.Common_TextColor, FieldType: FieldTypes.Color, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public string TextColor { get; set; }

        [FormField(LabelResource: Resources.UserAdminResources.Names.Common_StrokeColor, FieldType: FieldTypes.Color, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public string Stroke { get; set; }

        [FormField(LabelResource: Resources.UserAdminResources.Names.Common_FillColor, FieldType: FieldTypes.Color, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public string Fill { get; set; }



        [FormField(LabelResource: Resources.UserAdminResources.Names.LocationDiagram_Layers,
            FieldType: FieldTypes.ChildList, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public List<LocationDiagramLayer> Layers { get; set; } = new List<LocationDiagramLayer>();


        public LocationDiagramSummary CreateSummary()
        {
            var summary = new LocationDiagramSummary();
            summary.Populate(this);
            return summary;
        }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(Units),
                nameof(Stroke),
                nameof(TextColor),
                nameof(Fill),
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
        [EnumLabel(LocationDiagramShape.ShapeType_Polygon, UserAdminResources.Names.ShapeType_Polygon, typeof(UserAdminResources))]
        Polygon,
    }


    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.LocationDiagram_Shape, UserAdminResources.Names.LocationDiagram_Shape_Help, UserAdminResources.Names.LocationDiagram_Shape_Help,
        EntityDescriptionAttribute.EntityTypes.SimpleModel, typeof(UserAdminResources), Icon: "icon-ae-ecommerce-1", FactoryUrl: "/api/org/location/diagram/shape/factory")]
    public class LocationDiagramShape : IValidateable, IFormDescriptor, IFormDescriptorCol2
    {
        public const string ShapeType_Room = "room";
        public const string ShapeType_ParkingLot = "parkinglot";
        public const string ShapeType_ExternalEntrance = "exteranlentrance";
        public const string ShapeType_Door = "door";
        public const string ShapeType_Window = "window";
        public const string ShapeType_Closet = "closet";
        public const string ShapeType_Circle = "circle";
        public const string ShapeType_Polygon = "polygon";

        public LocationDiagramShape()
        {
            Id = Guid.NewGuid().ToId();
            Icon = "icon-ae-ecommerce-1";
            X = 50 * 12;
            Y = 50 * 12;
            Width = 10 * 12;
            Height = 10 * 12;
            Scale = 1;
            Fill = "#f8f8f8";            
            Stroke = "#000000";
            TextColor = "#000000";
            ShapeType = EntityHeader<ShapeTypes>.Create(ShapeTypes.Room);
            HorizontalAlign = EntityHeader<HorizontalAlign>.Create(Orgs.HorizontalAlign.Center);
            VerticalAlign = EntityHeader<VerticalAlign>.Create(Orgs.VerticalAlign.Middle);
        }

        [FormField(LabelResource: Resources.UserAdminResources.Names.Common_VerticalAlign, FieldType: FieldTypes.Picker, ResourceType: typeof(UserAdminResources), EnumType: typeof(VerticalAlign), WaterMark: UserAdminResources.Names.Common_VerticalAlign_Select, IsRequired: true, IsUserEditable: true)]
        public EntityHeader<VerticalAlign> VerticalAlign { get; set; }


        [FormField(LabelResource: Resources.UserAdminResources.Names.Common_HorizontalAlign, FieldType: FieldTypes.Picker, ResourceType: typeof(UserAdminResources), EnumType: typeof(HorizontalAlign), WaterMark: UserAdminResources.Names.Common_HorizontalAlign_Select, IsRequired: true, IsUserEditable: true)]
        public EntityHeader<HorizontalAlign> HorizontalAlign { get; set; }


        public string Id { get; set; }

        [FormField(LabelResource: LagoVistaCommonStrings.Names.Common_Name, FieldType: FieldTypes.Text, ResourceType: typeof(LagoVistaCommonStrings), IsRequired: true, IsUserEditable: true)]
        public string Name { get; set; }

        [FormField(LabelResource: LagoVistaCommonStrings.Names.Common_Key, HelpResource: LagoVistaCommonStrings.Names.Common_Key_Help, FieldType: FieldTypes.Key,
            RegExValidationMessageResource: LagoVistaCommonStrings.Names.Common_Key_Validation, ResourceType: typeof(LagoVistaCommonStrings), IsRequired: true)]
        public string Key { get; set; }


        [FormField(LabelResource: Resources.UserAdminResources.Names.Common_Icon, FieldType: FieldTypes.Icon, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public string Icon { get; set; }

        /*[FormField(LabelResource: UserAdminResources.Names.LocationDiagram_Shape_OrgLocation, WaterMark: UserAdminResources.Names.Common_SelectLocation, EntityHeaderPickerUrl: "/api/org/locations",
            HelpResource: UserAdminResources.Names.LocationDiagram_Shape_OrgLocation_Help, FieldType: FieldTypes.EntityHeaderPicker, ResourceType: typeof(UserAdminResources), IsUserEditable: true)]
        public EntityHeader Location { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.LocationDiagram_Shape_CustomerLocation, WaterMark: UserAdminResources.Names.Common_SelectLocation, EntityHeaderPickerUrl: "/api/customer/{customerid}/locations",
            HelpResource: UserAdminResources.Names.LocationDiagram_Shape_CustomerLocation_Help, FieldType: FieldTypes.EntityHeaderPicker, ResourceType: typeof(UserAdminResources), IsUserEditable: true)]
        public EntityHeader CustomerLocation { get; set; }*/


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


        [FormField(LabelResource: Resources.UserAdminResources.Names.Shape_TextRotation, FieldType: FieldTypes.Decimal, ResourceType: typeof(UserAdminResources), IsRequired: true, IsUserEditable: true)]
        public double TextRotation { get; set; }

        [FormField(LabelResource: Resources.UserAdminResources.Names.Shape_Scale, FieldType: FieldTypes.Decimal, ResourceType: typeof(UserAdminResources), IsRequired: true, IsUserEditable: true)]
        public double Scale { get; set; }

        [FormField(LabelResource: Resources.UserAdminResources.Names.Shape_FontSize, FieldType: FieldTypes.Decimal, ResourceType: typeof(UserAdminResources), IsRequired: true, IsUserEditable: true)]
        public double FontSize { get; set; } = 24;

        [FormField(LabelResource: Resources.UserAdminResources.Names.Shape_TextOffsetX, FieldType: FieldTypes.Decimal, ResourceType: typeof(UserAdminResources), IsRequired: true, IsUserEditable: true)]
        public double TextOffsetX { get; set; }
       
        [FormField(LabelResource: Resources.UserAdminResources.Names.Shape_TextOffsetY, FieldType: FieldTypes.Decimal, ResourceType: typeof(UserAdminResources), IsRequired: true, IsUserEditable: true)]
        public double TextOffsetY { get; set; }


        [FormField(LabelResource: Resources.UserAdminResources.Names.Common_TextColor, FieldType: FieldTypes.Color, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public string TextColor { get; set; }

        [FormField(LabelResource: Resources.UserAdminResources.Names.Common_StrokeColor, FieldType: FieldTypes.Color, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public string Stroke { get; set; }

        [FormField(LabelResource: Resources.UserAdminResources.Names.Common_FillColor, FieldType: FieldTypes.Color, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public string Fill { get; set; }


        public List<DeviceReference> Devices { get; set; } = new List<DeviceReference>();



        [FormField(LabelResource: Resources.UserAdminResources.Names.Shape_Locked, FieldType: FieldTypes.Icon, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public bool Locked { get; set; } = false;


        public GeoLocation GeoLocationCenter { get; set; }


        public List<GeoLocation> GeoPoints { get; set; } = new List<GeoLocation>();

        public List<ShapePoint> Points { get; set; } = new List<ShapePoint>();

        public EntityHeader ToEntityHeader()
        {
            return new EntityHeader()
            {
                Id = Id,
                Key = Key,
                Text = Name,
            };
        }


        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(Icon),
                nameof(ShapeType),
///                nameof(Location),
///                nameof(CustomerLocation),
                nameof(Rotation),
                nameof(Notes),
            };
        }

        public List<string> GetFormFieldsCol2()
        {
            return new List<string>()
            {
                nameof(Stroke),
                nameof(Fill),
                nameof(TextColor),
                nameof(FontSize),
                nameof(HorizontalAlign),
                nameof(VerticalAlign),
                nameof(TextRotation),
                nameof(TextOffsetX),
                nameof(TextOffsetY),
            };
        }
    }

    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.LocationDiagramLayer_Title, UserAdminResources.Names.LocationDiagramLayer_Description,
            UserAdminResources.Names.LocationDiagramLayer_Description,
            EntityDescriptionAttribute.EntityTypes.SimpleModel, typeof(UserAdminResources), Icon: "icon-fo-layer-3",
             FactoryUrl: "/api/org/location/diagram/layer")]
    public class LocationDiagramLayer : IValidateable, IFormDescriptor
    {
        public LocationDiagramLayer()
        {
            Id = Guid.NewGuid().ToId();
            Icon = "icon-fo-layer-3";
        }

        public string Id { get; set; }

        [FormField(LabelResource: LagoVistaCommonStrings.Names.Common_Name, FieldType: FieldTypes.Text, ResourceType: typeof(LagoVistaCommonStrings), IsRequired: true, IsUserEditable: true)]
        public string Name { get; set; }

        [FormField(LabelResource: LagoVistaCommonStrings.Names.Common_Key, HelpResource: LagoVistaCommonStrings.Names.Common_Key_Help, FieldType: FieldTypes.Key,
            RegExValidationMessageResource: LagoVistaCommonStrings.Names.Common_Key_Validation, ResourceType: typeof(LagoVistaCommonStrings), IsRequired: true)]
        public string Key { get; set; }

        [FormField(LabelResource: Resources.UserAdminResources.Names.Common_Icon, FieldType: FieldTypes.Icon, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public string Icon { get; set; }

        [FormField(LabelResource: Resources.UserAdminResources.Names.Common_Description, FieldType: FieldTypes.HtmlEditor, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public string Description { get; set; }

        [FormField(LabelResource: Resources.UserAdminResources.Names.LocationDiagramLayer_Locked, FieldType: FieldTypes.Icon, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public bool Locked { get; set; } = false;

        [FormField(LabelResource: Resources.UserAdminResources.Names.LocationDiagramLayer_Visible, FieldType: FieldTypes.Icon, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public bool Visible { get; set; } = true;

        [FormField(LabelResource: Resources.UserAdminResources.Names.LocationDiagramLayer_Shapes, FieldType: FieldTypes.ChildList, ResourceType: typeof(UserAdminResources))]
        public List<LocationDiagramShape> Shapes { get; set; } = new List<LocationDiagramShape>();
     
        [FormField(LabelResource: Resources.UserAdminResources.Names.LocationDiagramLayer_Groups, FieldType: FieldTypes.ChildList, ResourceType: typeof(UserAdminResources))]
        public List<LocationDiagramShapeGroup> Groups { get; set; } = new List<LocationDiagramShapeGroup>();

        public EntityHeader ToEntityHeader()
        {
            return new EntityHeader()
            {
                Id = Id,
                Key = Key,
                Text = Name,
            };
        }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                 nameof(Name),
                 nameof(Key),
                 nameof(Icon),
                 nameof(Locked),
                 nameof(Visible),
                 nameof(Description)
            };
        }
    }


    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.LocationDiagramShapeGroup_Title, UserAdminResources.Names.LocationDiagramShapeGroup_Help, 
            UserAdminResources.Names.LocationDiagramShapeGroup_Help, 
            EntityDescriptionAttribute.EntityTypes.SimpleModel, typeof(UserAdminResources), Icon: "ico-pz-worldwide-1",
             FactoryUrl: "/api/org/location/diagram/group")]
    public class LocationDiagramShapeGroup : IFormDescriptor, IValidateable
    {

        public LocationDiagramShapeGroup()
        {
            Id = Guid.NewGuid().ToId();
            Icon = "ico-pz-worldwide-1";
        }

        public string Id { get; set; }

        [FormField(LabelResource: LagoVistaCommonStrings.Names.Common_Name, FieldType: FieldTypes.Text, ResourceType: typeof(LagoVistaCommonStrings), IsRequired: true, IsUserEditable: true)]
        public string Name { get; set; }

        [FormField(LabelResource: LagoVistaCommonStrings.Names.Common_Key, HelpResource: LagoVistaCommonStrings.Names.Common_Key_Help, FieldType: FieldTypes.Key,
            RegExValidationMessageResource: LagoVistaCommonStrings.Names.Common_Key_Validation, ResourceType: typeof(LagoVistaCommonStrings), IsRequired: true)]
        public string Key { get; set; }

        [FormField(LabelResource: Resources.UserAdminResources.Names.Common_Icon, FieldType: FieldTypes.Icon, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public string Icon { get; set; }

        [FormField(LabelResource: Resources.UserAdminResources.Names.Common_Description, FieldType: FieldTypes.HtmlEditor, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public string Description { get; set; }


        [FormField(LabelResource: Resources.UserAdminResources.Names.LocationDiagramShapeGroup_Shapes, FieldType: FieldTypes.ChildList, ResourceType: typeof(UserAdminResources))]
        public List<EntityHeader> Shapes { get; set; } = new List<EntityHeader>();

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(Icon),
                nameof(Description)
            };
        }
    }

    public class ShapePoint
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.LocationDiagrams_Title, UserAdminResources.Names.LocationDiagrams_Description, UserAdminResources.Names.LocationDiagrams_Description, EntityDescriptionAttribute.EntityTypes.SimpleModel, typeof(UserAdminResources), Icon: "icon-pz-worldwide-1",
               GetListUrl: "/api/org/location/diagrams", GetUrl: "/api/org/location/diagram/{id}", DeleteUrl: "/api/org/location/diagram/{id}", SaveUrl: "/api/org/location/diagram", FactoryUrl: "/api/org/location/diagram")]
    public class LocationDiagramSummary : SummaryData
    {
        public EntityHeader Location { get; set; }
    }
}
