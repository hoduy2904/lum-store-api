using System;
using System.Net.Http.Json;
using LumStoreAPI.Core.Models.Enums;
using LumStoreAPI.Libraries.Extensions;
using LumStoreAPI.SDK.Extensions;
using LumStoreAPI.SDK.Shiprelay.Interfaces;
using LumStoreAPI.SDK.Shiprelay.Models;
using LumStoreAPI.SDK.Shiprelay.Models.Product;
using LumStoreAPI.SDK.Shiprelay.Models.Requests;

namespace LumStoreAPI.SDK.Shiprelay.Handlers;

internal class ShiprelayProductService(
    IHttpClientFactory httpClientFactory
) : IShiprelayProductService
{
    private const string PRODUCT_URLS = "products";
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    public async Task<ShiprelayPagedResponse<ShiprelayProduct>?> GetShiprelayProductsAsync(ShiprelayProductGetRequest request)
    {
        var client = _httpClientFactory.CreateShiprelayClient();
        var products = await client.GetFromJsonAsync<ShiprelayPagedResponse<ShiprelayProduct>>($"products{request.ToQueryString()}");
        return products;
    }
    public async Task<bool> IsExistsProductAsync(int id)
    {
        var client = _httpClientFactory.CreateShiprelayClient();
        var response = await client.GetAsync($"{PRODUCT_URLS}/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<ShiprelayProduct?> UpdateProductAsync(int id, ShiprelayProductUpdateRequest request, ProductType productType, bool ensureSuccess = true)
    {
        var client = _httpClientFactory.CreateShiprelayClient();
        var response = await client.PutAsJsonAsync($"{PRODUCT_URLS}/{Enum.GetName(productType)?.ToLower() ?? "simple"}/{id}", request);
        if (ensureSuccess)
        {
            return await response.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<ShiprelayProduct>();
        }
        else
        {
            if (response.IsSuccessStatusCode) return await response.Content.ReadFromJsonAsync<ShiprelayProduct>();
            return null;
        }
    }

    public async Task<ShiprelayProduct?> PostProductAsync(ShiprelayProductUpdateRequest request, ProductType productType)
    {
        var client = _httpClientFactory.CreateShiprelayClient();
        var response = await client.PostAsJsonAsync($"{PRODUCT_URLS}/{Enum.GetName(productType)?.ToLower() ?? "simple"}", request);
        var data = await response.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<ShiprelayProduct>();
        return data;
    }

    public async Task<ShiprelayProduct?> ArchiveProductAsync(int id)
    {
        var client = _httpClientFactory.CreateShiprelayClient();
        var response = await client.PatchAsync($"{PRODUCT_URLS}/{id}/archive", null);
        return await response.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<ShiprelayProduct>();
    }
    public async Task<ShiprelayProduct?> RestoreProductAsync(int id)
    {
        var client = _httpClientFactory.CreateShiprelayClient();
        var response = await client.PatchAsync($"{PRODUCT_URLS}/{id}/restore", null);
        return await response.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<ShiprelayProduct>();
    }
}
