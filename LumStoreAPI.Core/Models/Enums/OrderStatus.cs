namespace LumStoreAPI.Core.Models.Enums;

public enum OrderStatus
{
    Pending = 0,       // Chờ xác nhận
    Confirmed = 1,     // Đã xác nhận
    Processing = 2,    // Đang xử lý / chuẩn bị hàng
    Shipped = 3,       // Đang giao
    Delivered = 4,     // Đã giao
    Completed = 5,     // Hoàn tất
    Cancelled = 6,     // Đã hủy
    ReturnRequested = 7, // Yêu cầu trả hàng
    Returned = 8       // Đã hoàn trả
}
