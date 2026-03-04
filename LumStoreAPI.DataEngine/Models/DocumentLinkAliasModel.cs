namespace LumStoreAPI.DataEngine.Models
{
    internal record DocumentLinkAliasModel
    {
        public int Descendant { get; set; }
        public int Depth { get; set; }
        public string? NodeAlias { get; set; }
    }
}
