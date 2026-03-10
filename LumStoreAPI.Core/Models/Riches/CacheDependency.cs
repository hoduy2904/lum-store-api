namespace LumStoreAPI.Core.Models.Riches
{
    public class CacheDependency
    {
        private readonly ICollection<string> _dependencies = [];
        private bool _isNodeOrder = false;

        public CacheDependency CustomKey(string key)
        {
            if (!_dependencies.Contains(key))
            {
                _dependencies.Add(key);
            }
            return this;
        }

        public CacheDependency ClassName(string className)
        {
            return CustomKey($"node|{className}|all");
        }
        public CacheDependency Nodes()
        {
            return CustomKey("nodes");
        }

        public CacheDependency NodeID(int nodeID)
        {
            return CustomKey($"node|byid|{nodeID}");
        }

        public CacheDependency Children(int parentNodeID)
        {
            return CustomKey($"node|{parentNodeID}|children");
        }

        public CacheDependency SettingKey(string keyCode)
        {
            return CustomKey($"settingkey|bykeycode|{keyCode}");
        }

        public CacheDependency NodeOrder()
        {
            _isNodeOrder = true;
            return this;
        }

        public IEnumerable<string> GetDependencies()
        {
            if (_isNodeOrder)
                return _dependencies.Union(_dependencies.Select(x => $"{x}|nodeorder"));
            return _dependencies;
        }
    }
}
