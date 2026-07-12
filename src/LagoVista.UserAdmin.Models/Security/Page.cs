// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: e6489a86dff7cc63eabb90ae2c741736334d9fd2b8966ea0108f5b9dd45936d2
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core;
using LagoVista.Core.AI.Models;
using LagoVista.Core.AI.Models.Rag;
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Utils.Types.Nuviot.RagIndexing;
using LagoVista.UserAdmin.Models.Resources;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static LagoVista.Core.Networking.Models.uPnPDevice;

namespace LagoVista.UserAdmin.Models.Security
{
    [EntityDescription(
        Domains.SecurityDomain, UserAdminResources.Names.Page_Title, UserAdminResources.Names.Page_Help, UserAdminResources.Names.Page_Help,
        EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources),

        FactoryUrl: "/api/module/page/factory",
        Icon:"lago-icon://system/nuvos-semantic-icon/page-default",
        AiIconGuidance: "Represent a Page as a single navigable screen or destination in an application menu. Use a clean page, screen, or content-panel metaphor with one clear primary surface. The icon should feel like one specific place the user can open, not a module, area, document artifact, or dashboard report. Avoid multi-page stacks, folders, charts, browser chrome, complex layouts, or generic document icons with excessive lines. Use a simple rectangular page or screen shape with minimal interior structure.",
        AiIconGuidanceEntityField: "cardSummary",
        ClusterKey: "ui", ModelType: EntityDescriptionAttribute.ModelTypes.Taxonomy, Lifecycle: EntityDescriptionAttribute.Lifecycles.DesignTime,
        Sensitivity: EntityDescriptionAttribute.Sensitivities.Internal, IndexInclude: true, IndexTier: EntityDescriptionAttribute.IndexTiers.Primary,
        IndexPriority: 80, IndexTagsCsv: "securitydomain,ui,taxonomy,page")]
    public class Page : IFormDescriptor, IFormDescriptorCol2
    {
        public Page()
        {
            Features = new List<Feature>();
            Status = EntityHeader<ModuleStatus>.Create(ModuleStatus.Development);
            Id = Guid.NewGuid().ToId();
            DesktopSupport = true;
            CardIcon = "lago-icon://system/nuvos-semantic-icon/page-default";
            PhoneSupport = true;
            TabletSupport = true;
            HelplResources = new List<HelpResource>();
        }

        public int SortOrder { get; set; }

        public string Id { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Common_Name, IsRequired: true, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources))]
        public string Name { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Key, HelpResource: UserAdminResources.Names.Common_Key_Help, FieldType: FieldTypes.Key,
            RegExValidationMessageResource: UserAdminResources.Names.Common_Key_Validation, ResourceType: typeof(UserAdminResources), IsRequired: true)]
        public string Key { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Description, IsRequired: false, FieldType: FieldTypes.MultiLineText, ResourceType: typeof(UserAdminResources))]
        public string Description { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Module_CardIcon, IsRequired: true, FieldType: FieldTypes.Icon, ResourceType: typeof(UserAdminResources))]
        public string CardIcon { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Module_CardTitle, IsRequired: true, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources))]
        public string CardTitle { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Module_CardSummary, 
            AiChatPrompt: @"Generate a concise user-facing summary, 6 to 12 words, that explains what this navigation entity represents in the application. Prefer clear product language over technical implementation language. Use the entity name and existing summary when available. Do not repeat the name verbatim unless needed for clarity. Do not mention code, routing, repositories, controllers, permissions implementation, or database structure. Return only the summary text.
            Summarize this Page as a single navigable screen or destination in the application. The summary should explain what the user can view, manage, or accomplish on the page. Keep it 6 to 12 words, suitable for a UI title or compact menu description. Avoid module-level summaries, area-level grouping language, implementation details, route names, or security mechanics. Return only the summary text.",
            IsRequired: true, FieldType: FieldTypes.MultiLineText, ResourceType: typeof(UserAdminResources))]
        public string CardSummary { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Module_IsLegacyNGX, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool IsLegacyNGX { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Module_Link, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources))]
        public string Link { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_DesktopSupport, IsRequired: false, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool DesktopSupport { get; set; }
        [FormField(LabelResource: UserAdminResources.Names.Common_PhoneSupport, IsRequired: false, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool PhoneSupport { get; set; }
        [FormField(LabelResource: UserAdminResources.Names.Common_TabletSupport, IsRequired: false, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool TabletSupport { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Module_HelpResources, FieldType: FieldTypes.ChildList, ResourceType: typeof(UserAdminResources))]
        public List<HelpResource> HelplResources { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Common_Category, WaterMark: UserAdminResources.Names.Common_Category_Select, IsRequired: false, FieldType: FieldTypes.Picker, ResourceType: typeof(UserAdminResources))]
        public EntityHeader UiCategory { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Common_Status, IsRequired: true, FieldType: FieldTypes.Picker, EnumType: typeof(ModuleStatus), WaterMark: UserAdminResources.Names.ModuleStatus_Select, ResourceType: typeof(UserAdminResources))]
        public EntityHeader<ModuleStatus> Status { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Module_RestrictByDefault, HelpResource: UserAdminResources.Names.Module_RestrictByDefault_Help, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool RestrictByDefault { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Menu_DoNotDisplay, HelpResource: UserAdminResources.Names.Menu_DoNotDisplay_Help, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool DoNotDisplay { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Page_IsForProductLine, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool IsForProductLine { get; set; }

        public List<Feature> Features { get; set; } = new List<Feature>();

        public UserAccess UserAccess { get; set; }

        public Task<EntityRagContent<RagEntityVectorPayload>> GetRagContentAsync(Module parentModule, Area parentArea, RagEntityVectorPayload areaPayload)
        {
            var areaContent = new EntityRagContent<RagEntityVectorPayload>();
            var contentItems = new List<EntityRagContent<RagEntityVectorPayload>>();
            var descriptionBuilder = new StringBuilder();
            var embeddingsBuilder = new StringBuilder();

            var pagePayload = JsonConvert.DeserializeObject<RagEntityVectorPayload>(JsonConvert.SerializeObject(areaPayload));
            pagePayload.Meta.DocId = this.Id;
            pagePayload.Meta.Title = this.Name;
            pagePayload.Meta.SemanticId = $"{areaPayload.Meta.SemanticId}:{nameof(Page)}:{Id}";
            pagePayload.Meta.Subtype = nameof(Page);
            pagePayload.Extra.EditorUrl = $"/admin/module/{parentModule.Id}/area/{parentArea.Id}/page/{Id}";
            pagePayload.Extra.PreviewUrl = $"/{parentModule.Key}/{parentArea.Key}/{Key}";
            areaPayload.Extra.RestPUTUrl = null;
            areaPayload.Extra.RestGETUrl = null;

            descriptionBuilder.AppendLine("# User Interface Page");
            embeddingsBuilder.AppendLine($"{CardTitle}: {CardSummary}");
            descriptionBuilder.AppendLine($"Page Name: {Name}");
            descriptionBuilder.AppendLine($"Launcher Path: /{parentModule.Key}/{parentArea.Key}/{Key}");
            descriptionBuilder.AppendLine($"Descriptioon: {Description}");
            descriptionBuilder.AppendLine($"Launcher Card Title: {CardTitle}");
            descriptionBuilder.AppendLine($"Launcher Card Icon: {CardIcon}");
            descriptionBuilder.AppendLine($"Launcher Card Summary: {CardSummary}");
            descriptionBuilder.AppendLine();

            var pageContent = new EntityRagContent<RagEntityVectorPayload>()
            {
                Payload = areaPayload,
                EmbeddingContent = embeddingsBuilder.ToString(),
                ModelContent = descriptionBuilder.ToString(),
                HumanContent = descriptionBuilder.ToString()
            };

            return Task.FromResult(pageContent);
        }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(UiCategory),
                nameof(Key),
                nameof(RestrictByDefault),
                nameof(CardTitle),
                nameof(CardIcon),
                nameof(CardSummary),
            }; 
        }

        public List<string> GetFormFieldsCol2()
        {
            return new List<string>()
            {
                nameof(Status),
                nameof(DoNotDisplay),
                nameof(IsLegacyNGX),
                nameof(Link),
                nameof(DesktopSupport),
                nameof(TabletSupport),
                nameof(PhoneSupport),
                nameof(IsForProductLine),
                nameof(Description),
            };
        }

        public EntityHeader ToEntityHeader()
        {
            return EntityHeader.Create(Id, Key, Name);
        }
    }
}
