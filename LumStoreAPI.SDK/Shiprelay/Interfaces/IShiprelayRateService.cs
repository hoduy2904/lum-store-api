using System;
using LumStoreAPI.SDK.Shiprelay.Models;
using LumStoreAPI.SDK.Shiprelay.Models.Requests;

namespace LumStoreAPI.SDK.Shiprelay.Interfaces;

public interface IShiprelayRateService
{
    Task<ShiprelayPagedResponse<RateOption>> GetRates(RateRequest rateRequest);
}
