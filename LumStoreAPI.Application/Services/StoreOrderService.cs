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

    public StoreOrderService(
        ICustomerService customerService,
        IShiprelayService shiprelayService,
        IOrderService orderService,
        LumStoreContext ctx,
        IHttpContextAccessor httpContextAccessor,
        IPaymentService paymentService)
    {
        _customerService = customerService;
        _shiprelayService = shiprelayService;
        _orderService = orderService;
        _ctx = ctx;
        _httpContextAccessor = httpContextAccessor;
        _paymentService = paymentService;
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

        // Tính shipping fee: ưu tiên giá từ ShipRelay rates, fallback về threshold setting
        decimal shippingFee;
        if (!string.IsNullOrEmpty(address.ZipCode))
        {
            var rateRequest = new ShiprelayRateRequestDTO
            {
                OrderId = 0,
                RecipientName = user.FullName,
                Address1 = address.Address,
                Address2 = address.Details,
                City = address.City,
                Region = address.State,
                Country = "US",
                Zip = address.ZipCode,
                Phone = address.Phone,
                Email = user.Email,
                Items = orderItems.Select(i => new ShiprelayItemDTO
                {
                    ProductId = variants.FirstOrDefault(v => v.ItemID == i.VariantId)?.ShiprelayId ?? 0,
                    Quantity = i.Quantity,
                    Price = i.UnitPrice
                }).Where(i => i.ProductId > 0).ToList()
            };

            var rates = (await _shiprelayService.GetRatesAsync(rateRequest))?.ToList();
            var selectedRate = rates?.FirstOrDefault(r => r.ServiceCode == request.ShippingServiceCode)
                            ?? rates?.FirstOrDefault();
            shippingFee = selectedRate?.TotalPrice ?? 9.99m;
        }
        else
        {
            shippingFee = 9.99m;
            var shippingSetting = await _ctx.SettingKeyValues
                .FirstOrDefaultAsync(s => s.SettingCode == "Site.FreeShippingThreshold", ct);
            if (shippingSetting?.SettingValue is not null && decimal.TryParse(shippingSetting.SettingValue, out var threshold))
                if (subTotal >= threshold) shippingFee = 0;
        }

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
            Zip = address.ZipCode ?? string.Empty,
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
            ShippingZip = address.ZipCode ?? string.Empty,
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

        var paymentUrl = await _paymentService.PaymentCheckoutAsync(new() { Order = order, SuccessUrl = request.SuccessUrl, CancelUrl = request.CancelUrl });

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

            var unitPrice = product.PriceDiscount > 0 ? product.PriceDiscount : product.Price;
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
                UnitPrice = unitPrice,
                Quantity = cartItem.Quantity,
                LineTotal = lineTotal
            });
        }

        if (previewItems.Count == 0)
            return APIResponse<StoreCheckoutPreviewDTO>.Failure("No valid products found in cart");

        decimal taxRate = 0;
        var taxSetting = await _ctx.SettingKeyValues
            .FirstOrDefaultAsync(s => s.SettingCode == "Site.TaxRate", ct);
        if (taxSetting?.SettingValue is not null && decimal.TryParse(taxSetting.SettingValue, out var parsedRate))
            taxRate = parsedRate;

        var tax = Math.Round(subTotal * taxRate, 2);

        // Gọi ShipRelay rates/calculate để lấy tùy chọn vận chuyển thực tế
        IEnumerable<ShippingOptionDTO>? shippingOptions = null;
        ShippingOptionDTO? selectedShipping = null;

        var shiprelayItems = cartItems
            .Select(c => new ShiprelayItemDTO
            {
                ProductId = variants.FirstOrDefault(v => v.ItemID == c.VariantId)?.ShiprelayId ?? 0,
                Quantity = c.Quantity,
                Price = (products.FirstOrDefault(p => p.NodeID == c.NodeId) is { } prod
                    ? (prod.PriceDiscount > 0 ? prod.PriceDiscount : prod.Price)
                    : 0m)
            })
            .Where(i => i.ProductId > 0)
            .ToList();

        if (shiprelayItems.Count > 0 && !string.IsNullOrEmpty(address.ZipCode))
        {
            var rateRequest = new ShiprelayRateRequestDTO
            {
                OrderId = 0,
                RecipientName = address.Phone, // placeholder — name không cần thiết cho rates
                Address1 = address.Address,
                Address2 = address.Details,
                City = address.City,
                Region = address.State,
                Country = "US",
                Zip = address.ZipCode,
                Phone = address.Phone,
                Items = shiprelayItems
            };

            var rates = (await _shiprelayService.GetRatesAsync(rateRequest))?.ToList();
            if (rates is { Count: > 0 })
            {
                shippingOptions = rates.Select(r => new ShippingOptionDTO
                {
                    ServiceCode = r.ServiceCode,
                    ServiceName = r.ServiceName,
                    Price = r.TotalPrice,
                    Description = r.Description,
                    Currency = r.Currency,
                    MinDeliveryDate = r.MinDeliveryDate,
                    MaxDeliveryDate = r.MaxDeliveryDate,
                }).ToList();

                selectedShipping = shippingOptions
                    .FirstOrDefault(o => o.ServiceCode == request.ShippingServiceCode)
                    ?? shippingOptions.First();
            }
        }

        // Fallback nếu không lấy được rates từ ShipRelay
        decimal shippingFee;
        if (selectedShipping is not null)
        {
            shippingFee = selectedShipping.Price;
        }
        else
        {
            shippingFee = 9.99m;
            var shippingSetting = await _ctx.SettingKeyValues
                .FirstOrDefaultAsync(s => s.SettingCode == "Site.FreeShippingThreshold", ct);
            if (shippingSetting?.SettingValue is not null && decimal.TryParse(shippingSetting.SettingValue, out var threshold))
                if (subTotal >= threshold) shippingFee = 0;
        }

        var total = subTotal + shippingFee + tax;

        return APIResponse<StoreCheckoutPreviewDTO>.Success(new StoreCheckoutPreviewDTO
        {
            Subtotal = subTotal,
            Tax = tax,
            Total = total,
            ItemCount = previewItems.Sum(i => i.Quantity),
            SelectedShipping = selectedShipping,
            ShippingOptions = shippingOptions,
            Items = previewItems
        });
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
