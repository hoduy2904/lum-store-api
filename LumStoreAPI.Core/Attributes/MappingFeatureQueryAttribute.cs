using LumStoreAPI.Core.Entities.DocumentEngine;
using LumStoreAPI.Core.Interfaces.Sytems;

namespace LumStoreAPI.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class MappingFeatureQueryAttribute<TEntity, TQuery> : MappingFeatureQueryBaseAttribute where TEntity : DocumentPage where TQuery : IGenericFeatureQuery
    {
        public MappingFeatureQueryAttribute()
        {
            Entity = typeof(TEntity);
            Query = typeof(TQuery);
        }
    }
}
