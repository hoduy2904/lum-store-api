using LumStoreAPI.Core.Entities.Base;
using LumStoreAPI.Core.Models.Enums;

namespace LumStoreAPI.Core.Entities.Integrations
{
    public class ShiprelayDataSync : BaseClassItem
    {
        public int VariantID { get; set; }
        public int? ShiprelayId { get; set; }
        public EmailStatus Status { get; set; } = EmailStatus.Waiting;
        public string? Message { get; set; }
        public EntryActionStatus EntryActionStatus { get; set; } = EntryActionStatus.INSERT;
        public DateTimeOffset RunnedAt { get; set; }
    }
}
