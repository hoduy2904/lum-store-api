using System;
using System.Net.Http.Json;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.SDK.Extensions;
using LumStoreAPI.SDK.Shiprelay.Interfaces;
using LumStoreAPI.SDK.Shiprelay.Models;
using LumStoreAPI.SDK.Shiprelay.Models.Requests;
using Microsoft.Extensions.DependencyInjection;

namespace LumStoreAPI.SDK.Shiprelay.Handlers;

public class ShiprelayRateService(
    IHttpClientFactory httpClientFactory,
    IServiceProvider serviceProvider
) : IShiprelayRateService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    public async Task<ShiprelayPagedResponse<RateOption>> GetRates(RateRequest rateRequest)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var integrationConfigRepository = scope.ServiceProvider.GetRequiredService<IIntegrationConfigRepository>();
        var config = await integrationConfigRepository.GetConfigByTypeAsync(Core.Models.Enums.IntegrationType.Shiprelay);
        rateRequest.ResellerId = config?.ResellerId ?? "";

        var shiprelayClient = _httpClientFactory.CreateShiprelayClient();
        var response = await shiprelayClient.PostAsJsonAsync($"rates/calculate", rateRequest);
        if (response.IsSuccessStatusCode) return (await response.Content.ReadFromJsonAsync<ShiprelayPagedResponse<RateOption>>())!;

        var error = await response.Content.ReadAsStringAsync();

        throw new SystemException(error);
    }
}
