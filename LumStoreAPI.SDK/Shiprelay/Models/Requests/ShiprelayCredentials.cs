using System;

namespace LumStoreAPI.SDK.Shiprelay.Models.Requests;

public sealed record class ShiprelayCredentials(string BaseAddress, string Username, string Password);
