using LumStoreAPI.Application.DTOs.OrderDTO;
using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Application.DTOs.ShiprelayDTO;
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
    private readonly IShiprelayService _shiprelayService;
    private readonly IOrderService _orderService;
    private readonly LumStoreContext _ctx;
    private readonly IPaymentService _paymentService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IDiscountRuleService _discountRuleService;

    public StoreOrderService(
        ICustomerService customerService,
        IShiprelayService shiprelayService,
        IOrderService orderService,
        LumStoreContext ctx,
        IHttpContextAccessor httpContextAccessor,
        IPaymentService paymentService,
        IDiscountRuleService discountRuleService)
    {
        _customerService = customerService;
        _shiprelayService = shiprelayService;
        _orderService = orderService;
        _ctx = ctx;
        _httpContextAccessor = httpContextAccessor;
        _paymentService = paymentService;
        _discountRuleService = discountRuleService;
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

            decimal basePrice;
            decimal unitPrice;
            if (product.IsCombo)
            {
                var comboResult = await _discountRuleService.CalculateComboPriceAsync(product.NodeID);
                basePrice = comboResult.TotalPrice;
                unitPrice = comboResult.TotalPrice;
            }
            else
            {
                basePrice = product.PriceDiscount > 0 ? product.PriceDiscount : product.Price;
                unitPrice = await _discountRuleService.CalculateDiscountedPriceAsync(product.NodeID, basePrice, cartItem.Quantity);
            }

            var discountPerUnit = basePrice - unitPrice;
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
                UnitPrice = basePrice,
                Discount = discountPerUnit,
                Total = lineTotal
            });
        }

        if (orderItems.Count == 0)
            return APIResponse<StoreOrderSummaryDTO>.Failure("No valid products found in cart");

        // Get tax rate from settings (fallback to 0)
        decimal taxRate = 0;
        var taxSetting = await _ctx.SettingKeyValues
            .FirstOrDefaultAsync(s => s.SettingCode == "TAX_RATE", ct);
        if (taxSetting?.SettingValue is not null && decimal.TryParse(taxSetting.SettingValue, out var parsedRate))
            taxRate = parsedRate;

        // Get shipping threshold
        decimal shippingFee = 9.99m;
        var shippingSetting = await _ctx.SettingKeyValues
            .FirstOrDefaultAsync(s => s.SettingCode == "FREE_SHIPPING", ct);
        if (shippingSetting?.SettingValue is not null && decimal.TryParse(shippingSetting.SettingValue, out var threshold))
            if (subTotal >= threshold) shippingFee = 0;

        var tax = Math.Round(subTotal * taxRate, 2);
        var total = subTotal + shippingFee + tax;

        // Generate order code before calling ShipRelay so it can be used as order_ref
        var orderCode = GenerateOrderCode();

        // Call ShipRelay BEFORE saving — if it fails we do not create the order
        var shipmentDto = new ShiprelayCreateShipmentDTO
        {
            OrderId = 0,              // no DB ID yet
            OrderRef = orderCode,
            ShipmentTotalCost = total,
            PackageRef = 1,
            ShipmentCreatedAt = DateTimeOffset.UtcNow,
            ShippingSelectedRef = request.ShippingServiceCode,
            RecipientName = user.FullName,
            Email = user.Email,
            Phone = address.Phone,
            Address1 = address.Address,
            Address2 = address.Details,
            City = address.City,
            State = address.State,
            Zip = address.ZipCode,
            Country = "US",
            Notes = request.Note,
            Items = orderItems.Select(i => new ShiprelayItemDTO
            {
                ProductId = variants.FirstOrDefault(v => v.ItemID == i.VariantId)?.ShiprelayId ?? 0,
                Quantity = i.Quantity,
                Price = i.UnitPrice
            }).ToList()
        };

        var shipResult = await _shiprelayService.CreateShipmentAsync(shipmentDto);
        if (!shipResult.Success)
            return APIResponse<StoreOrderSummaryDTO>.Failure(
                shipResult.ErrorMessage ?? "Unable to create shipment. Please try again.");

        // Create order
        var order = new Order
        {
            OrderCode = orderCode,
            CustomerId = profileDto.ProfileId,
            CustomerName = user.FullName,
            CustomerEmail = user.Email,
            CustomerPhone = address.Phone,
            ShippingAddress = address.Address,
            ShippingDetails = address.Details,
            ShippingCity = address.City,
            ShippingState = address.State,
            ShippingZip = address.ZipCode,
            ShippingCountry = "US",
            CustomerNote = request.Note,
            SubTotal = subTotal,
            ShippingFee = shippingFee,
            Tax = tax,
            Total = total,
            Status = OrderStatus.Confirmed,
            PaymentStatus = PaymentStatus.Unpaid,
            ShiprelayShipmentId = shipResult.ShipmentId,
            TrackingNumber = shipResult.TrackingNumber,
            TrackingUrl = shipResult.TrackingUrl,
            ShippingCarrier = shipResult.Carrier,
            OrderItems = orderItems
        };

        _ctx.Orders.Add(order);
        await _ctx.SaveChangesAsync(ct);

        var rateResults = await _shiprelayService.GetRatesAsync(new ShiprelayRateRequestDTO
        {
            OrderId = 0,
            RecipientName = user.FullName,
            Address1 = address.Address,
            City = address.City,
            Region = address.State ?? string.Empty,
            Country = "US",
            Zip = address.ZipCode,
            Phone = address.Phone,
            Email = user.Email,
            Items = orderItems.Select(x => new ShiprelayItemDTO
            {
                ProductId = variants.FirstOrDefault(v => v.ItemID == x.VariantId)?.ShiprelayId ?? 0,
                Quantity = x.Quantity,
                Price = x.UnitPrice
            }).ToList()
        });

        shippingFee = rateResults.FirstOrDefault(r => r.ServiceCode == request.ShippingServiceCode)?.TotalPrice ?? 0;

        // Sync ShippingFee + Total in DB with the actual ShipRelay rate
        order.ShippingFee = shippingFee;
        order.Total = order.SubTotal + shippingFee + order.Tax;

        var paymentUrl = await _paymentService.PaymentCheckoutAsync(new() { Order = order, SuccessUrl = request.SuccessUrl, CancelUrl = request.CancelUrl, ShippingFee = shippingFee });

        // Log initial history
        _ctx.OrderHistories.Add(new OrderHistory
        {
            OrderId = order.ItemID,
            ToStatus = OrderStatus.Confirmed,
            Comment = "Order confirmed via ShipRelay",
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
            CreatedAt = order.CreatedAt,
            PaymentURL = paymentUrl
        }, ["Order placed and confirmed"]);

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

    // ── Checkout Preview ──────────────────────────────────────────────────────

    public async Task<APIResponse<StoreCheckoutPreviewDTO>> GetCheckoutPreviewAsync(
        StoreCheckoutPreviewRequest request, CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();

        var cartItems = await _ctx.UserCarts
            .Where(c => c.UserId == userId)
            .ToListAsync(ct);

        if (cartItems.Count == 0)
            return APIResponse<StoreCheckoutPreviewDTO>.Failure("Cart is empty");

        var address = await _ctx.CustomerAddresses
            .Include(x => x.User)
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync(a => a.ItemID == request.AddressId && a.UserId == userId, ct);

        if (address is null)
            return APIResponse<StoreCheckoutPreviewDTO>.Failure("Address not found");

        var nodeIds = cartItems.Select(c => c.NodeId).Distinct().ToArray();
        var products = await _ctx.Products
            .Where(p => nodeIds.Contains(p.NodeID))
            .ToListAsync(ct);

        var variantIds = cartItems.Where(c => c.VariantId.HasValue).Select(c => c.VariantId!.Value).ToArray();
        var variants = variantIds.Length > 0
            ? await _ctx.ProductVariants.Where(v => variantIds.Contains(v.ItemID)).ToListAsync(ct)
            : [];

        var allImageGuids = products.SelectMany(p => p.Images).Distinct().ToArray();
        var mediaByGuid = allImageGuids.Length > 0
            ? await _ctx.MediaLibraries
                .Where(m => allImageGuids.Contains(m.FileID))
                .ToDictionaryAsync(m => m.FileID, ct)
            : new Dictionary<Guid, Core.Entities.Systems.MediaLibrary>();

        var previewItems = new List<StoreCheckoutPreviewItemDTO>();
        decimal subTotal = 0;

        foreach (var cartItem in cartItems)
        {
            var product = products.FirstOrDefault(p => p.NodeID == cartItem.NodeId);
            if (product is null) continue;

            var variant = cartItem.VariantId.HasValue
                ? variants.FirstOrDefault(v => v.ItemID == cartItem.VariantId)
                : null;

            if (variant == null) continue;

            decimal basePrice;
            decimal unitPrice;
            if (product.IsCombo)
            {
                var comboResult = await _discountRuleService.CalculateComboPriceAsync(product.NodeID);
                basePrice = comboResult.TotalPrice;
                unitPrice = comboResult.TotalPrice;
            }
            else
            {
                basePrice = product.PriceDiscount > 0 ? product.PriceDiscount : product.Price;
                unitPrice = await _discountRuleService.CalculateDiscountedPriceAsync(product.NodeID, basePrice, cartItem.Quantity);
            }

            var lineTotal = unitPrice * cartItem.Quantity;
            subTotal += lineTotal;

            string? imageUrl = null;
            if (product.Images.Length > 0 && mediaByGuid.TryGetValue(product.Images[0], out var media))
                imageUrl = MediaLibraryHelper.GetFileURL(media);

            previewItems.Add(new StoreCheckoutPreviewItemDTO
            {
                NodeId = cartItem.NodeId,
                ProductName = product.ProductName,
                VariantName = variant?.VariantName,
                SKU = variant?.SKU,
                Image = imageUrl,
                OriginalPrice = basePrice,
                UnitPrice = unitPrice,
                Quantity = cartItem.Quantity,
                LineTotal = lineTotal,
                ProductId = variant!.ShiprelayId
            });
        }

        if (previewItems.Count == 0)
            return APIResponse<StoreCheckoutPreviewDTO>.Failure("No valid products found in cart");

        decimal taxRate = 0;
        var taxSetting = await _ctx.SettingKeyValues
            .FirstOrDefaultAsync(s => s.SettingCode == "TAX_RATE", ct);
        if (taxSetting?.SettingValue is not null && decimal.TryParse(taxSetting.SettingValue, out var parsedRate))
            taxRate = parsedRate;

        decimal shippingFee = 0;
        var rates = await _shiprelayService.GetRatesAsync(new ShiprelayRateRequestDTO
        {
            OrderId = 0,
            RecipientName = address.User.FullName,
            Address1 = address.Address,
            City = address.City,
            Region = address.State ?? string.Empty,
            Country = address.Country,
            Zip = address.ZipCode,
            Phone = address.Phone,
            Email = address.User.Email,
            Items = previewItems.Select(x => new ShiprelayItemDTO
            {
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                Price = x.UnitPrice
            }).ToList()
        });
        var tax = Math.Round(subTotal * taxRate, 2);
        var total = subTotal + shippingFee + tax;

        return APIResponse<StoreCheckoutPreviewDTO>.Success(new StoreCheckoutPreviewDTO
        {
            Subtotal = subTotal,
            ShippingFee = shippingFee,
            Tax = tax,
            Total = total,
            ItemCount = previewItems.Sum(i => i.Quantity),
            Items = previewItems,
            Rates = rates
        });
    }

    // ── Cancel ────────────────────────────────────────────────────────────────

    public async Task<APIResponse<bool>> CancelOrderAsync(int orderId, StoreCancelOrderRequest request, CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        var profileDto = await _customerService.GetOrCreateProfileAsync(userId);

        var order = await _ctx.Orders.FirstOrDefaultAsync(o => o.ItemID == orderId, ct);
        if (order is null)
            return APIResponse<bool>.Failure("Order not found");

        if (order.CustomerId != profileDto.ProfileId)
            return APIResponse<bool>.Failure("Forbidden");

        var cancellableStatuses = new[] { OrderStatus.Pending, OrderStatus.Confirmed };
        if (!cancellableStatuses.Contains(order.Status))
            return APIResponse<bool>.Failure("Order cannot be cancelled at this stage");

        await _orderService.UpdateOrderStatusAsync(orderId, new OrderUpdateStatusDTO
        {
            NewStatus = OrderStatus.Cancelled,
            Comment = request.Reason ?? "Cancelled by customer"
        });

        return APIResponse<bool>.Success(true, ["Order cancelled successfully"]);
    }

    // ── Returns ───────────────────────────────────────────────────────────────

    public async Task<APIResponse<IEnumerable<OrderReturnGetDTO>>> GetOrderReturnsAsync(
        int orderId, CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        var profileDto = await _customerService.GetOrCreateProfileAsync(userId);

        var order = await _ctx.Orders.FirstOrDefaultAsync(o => o.ItemID == orderId, ct);
        if (order is null)
            return APIResponse<IEnumerable<OrderReturnGetDTO>>.Failure("Order not found");

        if (order.CustomerId != profileDto.ProfileId)
            return APIResponse<IEnumerable<OrderReturnGetDTO>>.Failure("Forbidden");

        var returns = await _orderService.GetOrderReturnsAsync(orderId);
        return APIResponse<IEnumerable<OrderReturnGetDTO>>.Success(returns);
    }

    public async Task<APIResponse<OrderReturnGetDTO>> SubmitReturnAsync(
        int orderId, OrderReturnCreateDTO dto, CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        var profileDto = await _customerService.GetOrCreateProfileAsync(userId);

        var order = await _ctx.Orders.FirstOrDefaultAsync(o => o.ItemID == orderId, ct);
        if (order is null)
            return APIResponse<OrderReturnGetDTO>.Failure("Order not found");

        if (order.CustomerId != profileDto.ProfileId)
            return APIResponse<OrderReturnGetDTO>.Failure("Forbidden");

        if (order.Status is not (OrderStatus.Delivered or OrderStatus.Completed))
            return APIResponse<OrderReturnGetDTO>.Failure(
                "Returns can only be submitted for delivered or completed orders");

        var result = await _orderService.CreateReturnAsync(orderId, dto);
        return APIResponse<OrderReturnGetDTO>.Success(result, ["Return request submitted successfully"]);
    }
}
