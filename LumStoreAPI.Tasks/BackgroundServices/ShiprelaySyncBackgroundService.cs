using System;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.SDK.Shiprelay.Interfaces;
using Microsoft.Extensions.Hosting;

namespace LumStoreAPI.Tasks.BackgroundServices;

public class ShiprelaySyncBackgroundService : BackgroundService
{
    private readonly IShiprelayProductService _shiprelayProductService;
    private readonly IServiceProvider _serviceProvider;
    public ShiprelaySyncBackgroundService(
        IShiprelayProductService shiprelayProductService,
        IServiceProvider serviceProvider)
    {
        _shiprelayProductService = shiprelayProductService;
        _serviceProvider = serviceProvider;
    }
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}
