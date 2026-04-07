using LumStoreAPI.Core.Attributes;
using LumStoreAPI.Core.Entities.DocumentEngine;
using System.Reflection;
using System.Text.Json.Serialization;

namespace LumStoreAPI.Libraries.Helpers
{
    public class DocumentPageTypeHelper
    {
        public static Dictionary<string, Type> DocumentPageTypes = [];
        public static Dictionary<string, Type> DocumentWidgets = [];
        public static Dictionary<Type, Type> DocumentFeatureQueries = [];
        public static void RegisterPageTypes()
        {
            DocumentPageTypes = [];
            var types = AppDomain.CurrentDomain.GetAssemblies()
                 .SelectMany(a => a.GetTypes())
                 .Where(t => t.GetCustomAttribute<RegisterPageTypeAttribute>() != null);

            foreach (var type in types)
            {
                var attr = type.GetCustomAttribute<RegisterPageTypeAttribute>();
                DocumentPageTypes.Add(attr!.ClassName, attr.Type);
            }
        }

        public static void RegisterFeatureQueries()
        {
            DocumentFeatureQueries = [];
            var types = AppDomain.CurrentDomain.GetAssemblies()
                 .SelectMany(a => a.GetTypes())
                 .Where(t => t.GetCustomAttribute<MappingFeatureQueryBaseAttribute>(true) != null);

            foreach (var type in types)
            {
                var attr = type.GetCustomAttribute<MappingFeatureQueryBaseAttribute>(true);
                DocumentFeatureQueries.Add(attr!.Entity, attr.Query);
            }
        }

        public static void RegisterWidgets()
        {
            DocumentWidgets = [];
            var types = AppDomain.CurrentDomain.GetAssemblies()
                 .SelectMany(a => a.GetTypes())
                 .Where(t => t.GetCustomAttribute<RegisterWidgetAttribute>() != null);

            foreach (var type in types)
            {
                var attr = type.GetCustomAttribute<RegisterWidgetAttribute>();
                if (attr is not null && !DocumentWidgets.ContainsKey(attr!.WidgetName))
                    DocumentWidgets.Add(attr!.WidgetName, attr.WidgetQuery);
            }
        }

        public static bool IsAllowSystemField(string fieldName)
        {
            return typeof(DocumentPage)
                .GetProperties()
                .Where(x => x.PropertyType.GetCustomAttribute<JsonIgnoreAttribute>(true) is not null)
                .Any(x => x.Name.Equals(fieldName));
        }

        public static string GetClassName<T>() where T : DocumentPage
        {
            return typeof(T).GetField("CLASS_NAME", BindingFlags.Public | BindingFlags.Static)?.GetValue(null)?.ToString() ?? "CMS.Folder";
        }
        public static string GetClassName(Type type)
        {
            return type.GetField("CLASS_NAME", BindingFlags.Public | BindingFlags.Static)?.GetValue(null)?.ToString() ?? "CMS.Folder";
        }

    }
}
