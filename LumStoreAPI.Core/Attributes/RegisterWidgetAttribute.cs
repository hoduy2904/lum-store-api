namespace LumStoreAPI.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterWidgetAttribute : Attribute
    {
        public string WidgetName { get; set; }
        public Type WidgetQuery { get; set; }

        public RegisterWidgetAttribute(string widgetName, Type widgetType)
        {
            this.WidgetName = widgetName;
            this.WidgetQuery = widgetType;
        }
    }
}
