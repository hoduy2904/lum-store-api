using System.Text;

namespace LumStoreAPI.Core.Models.Systems
{
    public class AdminConfiguration
    {
        public string Key { get; set; } = default!;
        public byte[] EncodingKey => Encoding.UTF8.GetBytes(Key);
        public string AdminDomain { get; set; } = string.Empty;
    }
}
