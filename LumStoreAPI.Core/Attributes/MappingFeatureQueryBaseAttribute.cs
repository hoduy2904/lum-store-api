namespace LumStoreAPI.Core.Attributes
{
    public class MappingFeatureQueryBaseAttribute : Attribute
    {
        public Type Entity { get; protected set; } = default!;
        public Type Query { get; protected set; } = default!;

    }
}
