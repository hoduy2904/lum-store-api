using System;
using Microsoft.Extensions.Configuration;

namespace LumStoreAPI.SDK.Shiprelay;

internal class ShiprelayConfig
{
    public static string? ShiprelayUrl { get; set; }
    public static string? Email { get; set; }
    public static string? Password { get; set; }

    public static void Configure(IConfiguration configuration)
    {
        ShiprelayUrl = configuration.GetValue<string>(nameof(ShiprelayUrl));
        Email = configuration.GetValue<string>(nameof(Email));
        Password = configuration.GetValue<string>(nameof(Password));
    }
}
