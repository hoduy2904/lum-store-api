namespace LumStoreAPI.Core.Helpers
{
    public class DataSourceHelper
    {
        public static IEnumerable<string> GetEnumDataSource(Type enumType)
        {
            foreach (var type in Enum.GetValues(enumType))
            {
                yield return $"{Enum.GetName(enumType, type)};{(int)type}";
            }
        }
    }
}
