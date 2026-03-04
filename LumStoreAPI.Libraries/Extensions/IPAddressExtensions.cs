using System.Net;

namespace LumStoreAPI.Libraries.Extensions
{
    public static class IPAddressExtensions
    {
        extension(IPAddress? iPAddress)
        {
            public string? IPAddressString
            {
                get
                {
                    string? ip = iPAddress?.ToString();
                    if (string.IsNullOrEmpty(ip) && iPAddress != null && iPAddress.IsIPv4MappedToIPv6)
                    {
                        ip = iPAddress.MapToIPv4().ToString();
                    }
                    return ip;
                }
            }
        }
    }
}
