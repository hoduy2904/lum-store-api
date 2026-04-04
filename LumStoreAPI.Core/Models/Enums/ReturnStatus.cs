namespace LumStoreAPI.Core.Models.Enums;

public enum ReturnStatus
{
    Pending = 0,    // Chờ duyệt
    Approved = 1,   // Đã duyệt
    Rejected = 2,   // Từ chối
    Refunded = 3    // Đã hoàn tiền
}
