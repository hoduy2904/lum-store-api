using System;
using LumStoreAPI.SDK.Shiprelay.Models;

namespace LumStoreAPI.SDK.Shiprelay.Interfaces;

internal interface IShiprelayAuthService
{
    Task<AuthResponse?> LoginAsync(string email, string password);
    /// <summary>
    /// Use current config
    /// </summary>
    /// <returns></returns>
    Task<AuthResponse?> LoginAsync();
}
