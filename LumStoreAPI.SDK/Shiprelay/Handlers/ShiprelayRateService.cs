using System;
using System.Net.Http.Json;
using LumStoreAPI.SDK.Extensions;
using LumStoreAPI.SDK.Shiprelay.Interfaces;
using LumStoreAPI.SDK.Shiprelay.Models;
using LumStoreAPI.SDK.Shiprelay.Models.Requests;

namespace LumStoreAPI.SDK.Shiprelay.Handlers;

public class ShiprelayRateService(
    IHttpClientFactory httpClientFactory
) : IShiprelayRateService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    public async Task<ShiprelayPagedResponse<RateOption>> GetRates(RateRequest rateRequest)
    {
        rateRequest.ResellerId = "f8c2d4d7-1cef-4b7a-a7bd-697f59673722";

        var shiprelayClient = _httpClientFactory.CreateShiprelayClient();
        var response = await shiprelayClient.PostAsJsonAsync($"rates/calculate", rateRequest);
        if (response.IsSuccessStatusCode) return (await response.Content.ReadFromJsonAsync<ShiprelayPagedResponse<RateOption>>())!;

        var error = await response.Content.ReadAsStringAsync();

        throw new SystemException(error);
    }
}
