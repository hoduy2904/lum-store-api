using LumStoreAPI.Application.FeatureQueries;
using LumStoreAPI.Core.Entities.DocumentEngine;
using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.Core.Models.Systems;
using LumStoreAPI.Libraries.Extensions;
using LumStoreAPI.Libraries.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;

namespace LumStoreAPI.Application.DTOs.DocumentPageDTO
{
    public class DocumentPageGetDTO
    {
        [JsonIgnore]
        public IGenericFeatureQuery? FeatureQuery { get; set; }
        public int NodeID { get; set; }
        public string NodeName { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string DocumentName { get; set; } = default!;
        public int? ParentNodeID { get; set; }
        public string? RelativeUrl { get; set; }
        public int NodeOrder { get; set; }
        public string NodeAlias { get; set; } = default!;
        public bool RequireAuthentication { get; set; }
        public DateTimeOffset? PublishedFrom { get; set; }
        public DateTimeOffset? PublishedTo { get; set; }
        public object? SpecialContent { get; set; }
        public WidgetData<object>[] DocumentPageWidgets { get; set; } = [];
        public bool IsPublished => (PublishedFrom == null || PublishedFrom <= DateTime.UtcNow) && (PublishedTo == null || PublishedTo > DateTime.UtcNow);
        public Dictionary<string, object?> Fields { get; set; } = [];

        public DocumentPageGetDTO(DocumentPage documentPage)
        {
            this.NodeID = documentPage.NodeID;
            this.ClassName = documentPage.Node.ClassName;
            this.ParentNodeID = documentPage.Node?.ParentNodeID;
            this.DocumentName = documentPage.DocumentName;
            this.RelativeUrl = documentPage.Node?.RelativeUrl;
            this.NodeOrder = documentPage.Node?.NodeOrder ?? 0;
            this.NodeAlias = documentPage.Node?.NodeAlias ?? string.Empty;
            this.RequireAuthentication = documentPage.RequireAuthentication;
            this.PublishedFrom = documentPage.PublishedFrom;
            this.PublishedTo = documentPage.PublishedTo;
            this.NodeName = documentPage.Node?.NodeName ?? string.Empty;
            this.DocumentPageWidgets = documentPage.DocumentPageWidgets;
            if (DocumentPageTypeHelper.DocumentFeatureQueries.TryGetValue(typeof(DocumentPage), out var query))
            {
                this.FeatureQuery = (IGenericFeatureQuery?)Activator.CreateInstance(query, documentPage);
            }
            var type = documentPage.GetType();
            foreach (var property in type.GetProperties(
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.DeclaredOnly
                ).Where(x => x.DeclaringType != typeof(DocumentPage)))
            {
                if (!this.Fields.ContainsKey(property.Name))
                {
                    this.Fields.Add(property.Name.ToCamelCase(), property.GetValue(documentPage));
                }
            }
        }
    }
}
