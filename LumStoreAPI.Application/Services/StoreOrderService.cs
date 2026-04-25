using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Application.DTOs.StoreOrderDTO;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Entities.Orders;
using LumStoreAPI.Core.Models.Enums;
using LumStoreAPI.Infrastructure;
using LumStoreAPI.Infrastructure.Extensions;
using LumStoreAPI.Libraries.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LumStoreAPI.Application.Services;

internal class StoreOrderService : IStoreOrderService
{
    private readonly ICustomerService _customerService;
    private readonly LumStoreContext _ctx;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public StoreOrderService(
        ICustomerService customerService,
        LumStoreContext ctx,
        IHttpContextAccessor httpContextAccessor)
    {
        _customerService = customerService;
        _ctx = ctx;
        _httpContextAccessor = httpContextAccessor;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private int GetCurrentUserId()
    {
        var claim = _httpContextAccessor.HttpContext?.User
            .Claims.FirstOrDefault(x => x.Type == "id")?.Value;

        if (!int.TryParse(claim, out int userId))
            throw new UnauthorizedAccessException("User identity could not be resolved.");

        return userId;
    }

    private static string GenerateOrderCode()
        => $"ORD-{DateTimeOffset.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

    // ── Commands ──────────────────────────────────────────────────────────────

    public async Task<APIResponse<StoreOrderSummaryDTO>> PlaceOrderAsync(StorePlaceOrderRequest request, CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();

        // Load cart
        var cartItems = (await _ctx.UserCarts
            .Where(c => c.UserId == userId)
            .ToListAsync(ct));

        if (cartItems.Count == 0)
            return APIResponse<StoreOrderSummaryDTO>.Failure("Cart is empty");

        // Verify address ownership
        var address = await _ctx.CustomerAddresses
            .FirstOrDefaultAsync(a => a.ItemID == request.AddressId && a.UserId == userId, ct);

        if (address is null)
            return APIResponse<StoreOrderSummaryDTO>.Failure("Address not found");

        // Load user
        var user = await _ctx.Users.FirstOrDefaultAsync(u => u.ItemID == userId, ct);
        if (user is null)
            return APIResponse<StoreOrderSummaryDTO>.Failure("User not found");

        // Ensure customer profile exists (auto-create if first order)
        var profileDto = await _customerService.GetOrCreateProfileAsync(userId);

        // Load products for snapshot
        var nodeIds = cartItems.Select(c => c.NodeId).Distinct().ToArray();
        var products = await _ctx.Products
            .Where(p => nodeIds.Contains(p.NodeID))
            .ToListAsync(ct);

        // Load variants
        var variantIds = cartItems.Where(c => c.VariantId.HasValue).Select(c => c.VariantId!.Value).ToArray();
        var variants = variantIds.Length > 0
            ? await _ctx.ProductVariants.Where(v => variantIds.Contains(v.ItemID)).ToListAsync(ct)
            : [];

        // Load first image for each product
        var allImageGuids = products.SelectMany(p => p.Images).Distinct().ToArray();
        var mediaByGuid = allImageGuids.Length > 0
            ? await _ctx.MediaLibraries
                .Where(m => allImageGuids.Contains(m.FileID))
                .ToDictionaryAsync(m => m.FileID, ct)
            : new Dictionary<Guid, Core.Entities.Systems.MediaLibrary>();

        // Validate stock for variant items
        foreach (var cartItem in cartItems.Where(c => c.VariantId.HasValue))
        {
            var variant = variants.FirstOrDefault(v => v.ItemID == cartItem.VariantId);
            if (variant is null)
                return APIResponse<StoreOrderSummaryDTO>.Failure($"Variant not found for a cart item");
            if (cartItem.Quantity > variant.Stock)
                return APIResponse<StoreOrderSummaryDTO>.Failure($"Item out of stock: {products.FirstOrDefault(p => p.NodeID == cartItem.NodeId)?.ProductName ?? "Unknown"}");
        }

        // Build order items
        var orderItems = new List<OrderItem>();
        decimal subTotal = 0;

        foreach (var cartItem in cartItems)
        {
            var product = products.FirstOrDefault(p => p.NodeID == cartItem.NodeId);
            if (product is null) continue;

            var variant = cartItem.VariantId.HasValue
                ? variants.FirstOrDefault(v => v.ItemID == cartItem.VariantId)
                : null;

            var unitPrice = product.PriceDiscount > 0 ? product.PriceDiscount : product.Price;
            var lineTotal = unitPrice * cartItem.Quantity;
            subTotal += lineTotal;

            // Resolve first image URL
            string? imageUrl = null;
            if (product.Images.Length > 0 && mediaByGuid.TryGetValue(product.Images[0], out var media))
                imageUrl = MediaLibraryHelper.GetFileURL(media);

            orderItems.Add(new OrderItem
            {
                ProductId = cartItem.NodeId,
                VariantId = cartItem.VariantId,
                ProductName = product.ProductName,
                VariantName = variant?.VariantName,
                SKU = variant?.SKU,
                ImageUrl = imageUrl,
                Quantity = cartItem.Quantity,
                UnitPrice = unitPrice,
                Discount = 0,
                Total = lineTotal
            });
        }

        if (orderItems.Count == 0)
            return APIResponse<StoreOrderSummaryDTO>.Failure("No valid products found in cart");

        // Get tax rate from settings (fallback to 0)
        decimal taxRate = 0;
        var taxSetting = await _ctx.SettingKeyValues
            .FirstOrDefaultAsync(s => s.SettingCode == "Site.TaxRate", ct);
        if (taxSetting?.SettingValue is not null && decimal.TryParse(taxSetting.SettingValue, out var parsedRate))
            taxRate = parsedRate;

        // Get shipping threshold
        decimal shippingFee = 9.99m;
        var shippingSetting = await _ctx.SettingKeyValues
            .FirstOrDefaultAsync(s => s.SettingCode == "Site.FreeShippingThreshold", ct);
        if (shippingSetting?.SettingValue is not null && decimal.TryParse(shippingSetting.SettingValue, out var threshold))
            if (subTotal >= threshold) shippingFee = 0;

        var tax = Math.Round(subTotal * taxRate, 2);
        var total = subTotal + shippingFee + tax;

        // Create order
        var order = new Order
        {
            OrderCode = GenerateOrderCode(),
            CustomerId = profileDto.ProfileId,
            CustomerName = user.FullName,
            CustomerEmail = user.Email,
            CustomerPhone = address.Phone,
            ShippingAddress = address.Address,
            ShippingDetails = address.Details,
            ShippingCity = address.City,
            ShippingState = address.State,
            ShippingZip = string.Empty,
            ShippingCountry = "US",
            CustomerNote = request.Note,
            SubTotal = subTotal,
            ShippingFee = shippingFee,
            Tax = tax,
            Total = total,
            Status = OrderStatus.Pending,
            PaymentStatus = PaymentStatus.Unpaid,
            OrderItems = orderItems
        };

        _ctx.Orders.Add(order);
        await _ctx.SaveChangesAsync(ct);

        // Log initial history
        _ctx.OrderHistories.Add(new OrderHistory
        {
            OrderId = order.ItemID,
            ToStatus = OrderStatus.Pending,
            Comment = "Order placed by customer",
            IsSystemAction = true
        });
        await _ctx.SaveChangesAsync(ct);

        // Update customer stats
        await _ctx.CustomerProfiles
            .Where(p => p.ItemID == profileDto.ProfileId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.TotalOrders, p => p.TotalOrders + 1)
                .SetProperty(p => p.TotalSpent, p => p.TotalSpent + subTotal), ct);

        // Clear cart
        await _ctx.UserCarts.Where(c => c.UserId == userId).ExecuteDeleteAsync(ct);

        return APIResponse<StoreOrderSummaryDTO>.Success(new StoreOrderSummaryDTO
        {
            OrderId = order.ItemID,
            OrderCode = order.OrderCode,
            Status = order.Status.ToString().ToLower(),
            PaymentStatus = order.PaymentStatus.ToString().ToLower(),
            Total = order.Total,
            ItemCount = orderItems.Sum(i => i.Quantity),
            CreatedAt = order.CreatedAt
        }, ["Order placed successfully"]);
    }

    // ── Queries ───────────────────────────────────────────────────────────────

    public async Task<PagedResponse<StoreOrderSummaryDTO>> GetOrdersAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        var profileDto = await _customerService.GetOrCreateProfileAsync(userId);

        var query = _ctx.Orders
            .Where(o => o.CustomerId == profileDto.ProfileId)
            .OrderByDescending(o => o.CreatedAt);

        var total = await query.CountAsync(ct);
        var orders = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(o => o.OrderItems)
            .ToListAsync(ct);

        var dtos = orders.Select(o => new StoreOrderSummaryDTO
        {
            OrderId = o.ItemID,
            OrderCode = o.OrderCode,
            Status = o.Status.ToString().ToLower(),
            PaymentStatus = o.PaymentStatus.ToString().ToLower(),
            Total = o.Total,
            ItemCount = o.OrderItems.Sum(i => i.Quantity),
            CreatedAt = o.CreatedAt,
            PreviewItems = o.OrderItems.Take(3).Select(i => new OrderPreviewItemDTO
            {
                ProductName = i.ProductName,
                Image = i.ImageUrl,
                VariantName = i.VariantName,
                Price = i.UnitPrice,
                Quantity = i.Quantity
            })
        }).AsPagedEnumerable(total);

        return PagedResponse<StoreOrderSummaryDTO>.Success(dtos, page, pageSize);
    }

    public async Task<APIResponse<StoreOrderDetailDTO>> GetOrderAsync(int orderId, CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        var profileDto = await _customerService.GetOrCreateProfileAsync(userId);

        var order = await _ctx.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.ItemID == orderId, ct);

        if (order is null)
            return APIResponse<StoreOrderDetailDTO>.Failure("Order not found");

        if (order.CustomerId != profileDto.ProfileId)
            return APIResponse<StoreOrderDetailDTO>.Failure("Forbidden");

        var detail = new StoreOrderDetailDTO
        {
            OrderId = order.ItemID,
            OrderCode = order.OrderCode,
            Status = order.Status.ToString().ToLower(),
            PaymentStatus = order.PaymentStatus.ToString().ToLower(),
            Total = order.Total,
            ItemCount = order.OrderItems.Sum(i => i.Quantity),
            CreatedAt = order.CreatedAt,
            Note = order.CustomerNote,
            TrackingNumber = order.TrackingNumber,
            TrackingUrl = order.TrackingUrl,
            Address = new StoreOrderAddressDTO
            {
                Address = order.ShippingAddress,
                Details = order.ShippingDetails,
                Phone = order.CustomerPhone,
                City = order.ShippingCity,
                State = order.ShippingState,
                ZipCode = order.ShippingZip,
                Country = order.ShippingCountry
            },
            Items = order.OrderItems.Select(i => new StoreOrderLineItemDTO
            {
                NodeID = i.ProductId,
                ProductName = i.ProductName,
                Image = i.ImageUrl,
                Price = i.UnitPrice,
                Quantity = i.Quantity,
                VariantId = i.VariantId,
                VariantName = i.VariantName,
                SKU = i.SKU,
                LineTotal = i.Total
            }),
            Subtotal = order.SubTotal,
            Shipping = order.ShippingFee,
            Tax = order.Tax
        };

        return APIResponse<StoreOrderDetailDTO>.Success(detail);
    }
}
