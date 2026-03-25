using LumStoreAPI.Core.Attributes;
using LumStoreAPI.Core.Entities.DocumentEngine;
using System.Reflection;

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

    }
}
