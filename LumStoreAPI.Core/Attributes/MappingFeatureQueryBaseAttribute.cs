namespace LumStoreAPI.Core.Attributes
{
    public class MappingFeatureQueryBaseAttribute : Attribute
    {
        public Type Entity { get; protected set; }
        public Type Query { get; protected set; }

    }
}
