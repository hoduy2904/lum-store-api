using System;
using System.Net.Http.Json;
using System.Text.Json;
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
    public async Task<ShiprelayProduct?> GetProductByIdAsync(int id)
    {
        var client = _httpClientFactory.CreateShiprelayClient();
        var response = await client.GetAsync($"{PRODUCT_URLS}/{id}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        // ShipRelay API v2 wraps single-item GET responses in {"data": {...}}
        var element = root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Object
            ? data
            : root;

        return JsonSerializer.Deserialize<ShiprelayProduct>(element.GetRawText());
    }

    public async Task<bool> IsExistsProductAsync(int id)
        => (await GetProductByIdAsync(id)) != null;

    public async Task<ShiprelayProduct?> UpdateProductAsync(int id, ShiprelayProductUpdateRequest request, ProductType productType, bool ensureSuccess = true)
    {
        if (productType == ProductType.BUNDLE)
            throw new NotSupportedException("BUNDLE products are not supported by ShipRelay. Map the variant to SIMPLE, PACKING, or CASEPACK before syncing.");

        var client = _httpClientFactory.CreateShiprelayClient();
        var response = await client.PutAsJsonAsync($"{PRODUCT_URLS}/{Enum.GetName(productType)?.ToLower() ?? "simple"}/{id}", request);
        ShiprelayProduct? shiprelayProduct = null;
        if (ensureSuccess)
        {
            shiprelayProduct = await response.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<ShiprelayProduct>();
        }
        else
        {
            if (response.IsSuccessStatusCode) shiprelayProduct = await response.Content.ReadFromJsonAsync<ShiprelayProduct>();
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception(error);
            }
        }
        if (shiprelayProduct is not null && shiprelayProduct.ArchivedAt is not null)
        {
            return await this.RestoreProductAsync(shiprelayProduct.Id);
        }
        return shiprelayProduct;
    }

    public async Task<ShiprelayProduct?> PostProductAsync(ShiprelayProductUpdateRequest request, ProductType productType)
    {
        if (productType == ProductType.BUNDLE)
            throw new NotSupportedException("BUNDLE products are not supported by ShipRelay. Map the variant to SIMPLE, PACKING, or CASEPACK before syncing.");

        var client = _httpClientFactory.CreateShiprelayClient();
        var response = await client.PostAsJsonAsync($"{PRODUCT_URLS}/{Enum.GetName(productType)?.ToLower() ?? "simple"}", request);
        if (response.StatusCode == System.Net.HttpStatusCode.UnprocessableContent)
        {
            var error = await response.Content.ReadAsStringAsync();
            if (ShiprelayErrorConstants.ExistsProduct.Any(x => error.Contains(x, StringComparison.OrdinalIgnoreCase)))
            {
                var product = await this.GetShiprelayProductsAsync(new ShiprelayProductGetRequest { Page = 1, PerPage = 1, SKU = request.SKU });
                if (product is null || !product.Data.Any()) throw new Exception(error);
                var id = product.Data.First().Id;
                request.Settings.RemoveUnescessaryUpdate();
                return await this.UpdateProductAsync(id, request, productType, false);
            }
            else
            {
                throw new Exception(error);
            }
        }
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

    public async Task<bool> IsExistsProductAsync(string sku)
    {
        var data = await this.GetShiprelayProductsAsync(new ShiprelayProductGetRequest { Page = 1, PerPage = 1, SKU = sku });
        return data is not null && data.Meta.Total > 0;
    }
}
