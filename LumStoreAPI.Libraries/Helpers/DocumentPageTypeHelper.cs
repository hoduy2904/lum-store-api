using LumStoreAPI.Core.Attributes;
using LumStoreAPI.Core.Entities.DocumentEngine;
using System.Reflection;
using System.Text.Json.Serialization;

namespace LumStoreAPI.Libraries.Helpers
{
    public class DocumentPageTypeHelper
    {
        public static Dictionary<string, Type> DocumentPageTypes = [];
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
