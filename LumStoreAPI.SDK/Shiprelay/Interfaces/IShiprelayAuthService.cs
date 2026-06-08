using System;
using LumStoreAPI.SDK.Shiprelay.Models;
using LumStoreAPI.SDK.Shiprelay.Models.Requests;

namespace LumStoreAPI.SDK.Shiprelay.Interfaces;

internal interface IShiprelayAuthService
{
    Task<AuthResponse?> LoginAsync(ShiprelayCredentials shiprelayCredentials);
    Task LogoutAsync(string token, string baseUrl);
}
