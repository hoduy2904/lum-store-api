using LumStoreAPI.Core.Entities.Base;
using LumStoreAPI.Core.Models.Enums;

namespace LumStoreAPI.Core.Entities.Integrations;

/// <summary>Stores connection settings for external integrations (Shiprelay, WMS, etc.).</summary>
public class IntegrationConfig : BaseClassItem
{
    public IntegrationType IntegrationType { get; set; }
    public string Name { get; set; } = default!;
    public string BaseUrl { get; set; } = default!;
    public string? ApiKey { get; set; }       // ShipRelay: login email
    public string? ApiSecret { get; set; }    // ShipRelay: login password
    public string? ResellerId { get; set; }   // ShipRelay: reseller_id UUID (required for shipments & rates)
    public string? WebhookSecret { get; set; }
    public string? AdditionalConfig { get; set; }  // JSON for extra provider-specific settings
    public bool IsEnabled { get; set; } = true;
    public DateTimeOffset? LastSyncAt { get; set; }
}
