using LumStoreAPI.Core.Entities.Base;
using LumStoreAPI.Core.Models.Enums;

namespace LumStoreAPI.Core.Entities.Integrations;

/// <summary>Log record for every sync operation with external systems.</summary>
public class SyncLog : BaseClassItem
{
    public IntegrationType IntegrationType { get; set; }
    public SyncMode SyncMode { get; set; }
    public SyncStatus Status { get; set; }
    public string? Summary { get; set; }
    public string? ErrorDetail { get; set; }
    public int RecordsSynced { get; set; }
    public int RecordsFailed { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}
