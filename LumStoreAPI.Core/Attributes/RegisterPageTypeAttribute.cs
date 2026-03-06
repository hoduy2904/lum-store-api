namespace LumStoreAPI.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterPageTypeAttribute : Attribute
    {
        public string ClassName { get; set; }
        public Type Type { get; set; }
        public RegisterPageTypeAttribute(string className, Type type)
        {
            this.Type = type;
            this.ClassName = className;
        }
    }
}
