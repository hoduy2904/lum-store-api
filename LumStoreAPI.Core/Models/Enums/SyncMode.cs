namespace LumStoreAPI.Core.Models.Enums;

public enum SyncMode
{
    Manual = 0,
    Scheduled = 1,
    Webhook = 2
}

public enum SyncStatus
{
    Success = 0,
    Failed = 1,
    Partial = 2
}

public enum IntegrationType
{
    Shiprelay = 0,
    WMS = 1,
    Payment = 2,
    Other = 3
}
