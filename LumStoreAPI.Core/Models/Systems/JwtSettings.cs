using System.Text;

namespace LumStoreAPI.Core.Models.Systems
{
    public class JwtSettings
    {
        public string Key { get; set; } = default!;
        public byte[] EncodingKey => Encoding.UTF8.GetBytes(Key);
    }
}
