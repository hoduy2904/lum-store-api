namespace LumStoreAPI.Libraries.Helpers
{
    public class ValidationHelper
    {
        public static string GetStringValue(string? value, string defaultValue = "")
        {
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;
            return value;
        }
    }
}
