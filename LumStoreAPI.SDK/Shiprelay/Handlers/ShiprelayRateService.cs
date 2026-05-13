using System;
using System.Net.Http.Json;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.SDK.Extensions;
using LumStoreAPI.SDK.Shiprelay.Interfaces;
using LumStoreAPI.SDK.Shiprelay.Models;
using LumStoreAPI.SDK.Shiprelay.Models.Requests;

namespace LumStoreAPI.SDK.Shiprelay.Handlers;

public class ShiprelayRateService(
    IHttpClientFactory httpClientFactory,
    IIntegrationConfigRepository integrationConfigRepository
) : IShiprelayRateService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IIntegrationConfigRepository _integrationConfigRepository = integrationConfigRepository;
    public async Task<ShiprelayPagedResponse<RateOption>> GetRates(RateRequest rateRequest)
    {
        var config = await _integrationConfigRepository.GetConfigByTypeAsync(Core.Models.Enums.IntegrationType.Shiprelay);
        rateRequest.ResellerId = config?.ResellerId ?? "";

        var shiprelayClient = _httpClientFactory.CreateShiprelayClient();
        var response = await shiprelayClient.PostAsJsonAsync($"rates/calculate", rateRequest);
        if (response.IsSuccessStatusCode) return (await response.Content.ReadFromJsonAsync<ShiprelayPagedResponse<RateOption>>())!;

        var error = await response.Content.ReadAsStringAsync();

        throw new SystemException(error);
    }
}
