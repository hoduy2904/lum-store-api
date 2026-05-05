using LumStoreAPI.Core.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace LumStoreAPI.Application.DTOs.IntegrationDTO;

public class IntegrationConfigGetDTO
{
    public int ConfigId { get; set; }
    public IntegrationType IntegrationType { get; set; }
    public string TypeName => IntegrationType.ToString();
    public string Name { get; set; } = default!;
    public string BaseUrl { get; set; } = default!;
    public string? ApiKey { get; set; }
    public string? SecretKey { get; set; }
    public bool HasApiKey { get; set; }         // mask actual key
    public bool HasWebhookSecret { get; set; }
    public bool IsEnabled { get; set; }
    public DateTimeOffset? LastSyncAt { get; set; }
}

public class IntegrationConfigUpsertDTO
{
    [Required]
    public IntegrationType IntegrationType { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = default!;

    [Required, MaxLength(500)]
    public string BaseUrl { get; set; } = default!;

    [MaxLength(500)]
    public string? ApiKey { get; set; }

    [MaxLength(500)]
    public string? ApiSecret { get; set; }

    [MaxLength(500)]
    public string? WebhookSecret { get; set; }

    public string? AdditionalConfig { get; set; }   // JSON

    public bool IsEnabled { get; set; } = true;
}

public class SyncLogGetDTO
{
    public int LogId { get; set; }
    public IntegrationType IntegrationType { get; set; }
    public string TypeName => IntegrationType.ToString();
    public SyncMode SyncMode { get; set; }
    public string ModeName => SyncMode.ToString();
    public SyncStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public string? Summary { get; set; }
    public string? ErrorDetail { get; set; }
    public int RecordsSynced { get; set; }
    public int RecordsFailed { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
