using LumStoreAPI.Application.DTOs.OrderDTO;
using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Application.DTOs.ShiprelayDTO;
using LumStoreAPI.Application.DTOs.StoreOrderDTO;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Entities.Orders;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Core.Models.Constants.Systems;
using LumStoreAPI.Core.Models.Enums;
using LumStoreAPI.Infrastructure;
using LumStoreAPI.Infrastructure.Extensions;
using LumStoreAPI.Libraries.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace LumStoreAPI.Application.Services;

internal class StoreOrderService : IStoreOrderService
{
    private readonly ICustomerService _customerService;
    private readonly IShiprelayService _shiprelayService;
    private readonly IOrderService _orderService;
    private readonly LumStoreContext _ctx;
    private readonly IPaymentService _paymentService;
    private readonly IDiscountRuleService _discountRuleService;
    private readonly IAddressService _addressService;
    private readonly IMediaService _mediaService;
    private readonly IUserService _userService;
    private readonly ISettingKeyValueRepository _settingKeyValueRepository;

    public StoreOrderService(
        ICustomerService customerService,
        IShiprelayService shiprelayService,
        IOrderService orderService,
        LumStoreContext ctx,
        IPaymentService paymentService,
        IDiscountRuleService discountRuleService,
        IAddressService addressService,
        IMediaService mediaService,
        IUserService userService,
        ISettingKeyValueRepository settingKeyValueRepository)
    {
        _customerService = customerService;
        _shiprelayService = shiprelayService;
        _orderService = orderService;
        _ctx = ctx;
        _paymentService = paymentService;
        _discountRuleService = discountRuleService;
        _addressService = addressService;
        _mediaService = mediaService;
        _userService = userService;
        _settingKeyValueRepository = settingKeyValueRepository;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string GenerateOrderCode()
        => $"ORD-{DateTimeOffset.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";


    // ── Commands ──────────────────────────────────────────────────────────────

    public async Task<APIResponse<StoreOrderSummaryDTO>> PlaceOrderAsync(StorePlaceOrderRequest request, CancellationToken ct = default)
    {
        var currentUser = await _userService.GetCurrentUserAsync();
        if (currentUser == null) return APIResponse<StoreOrderSummaryDTO>.Failure(ErrorStatusNameConstants.NOT_FOUND, ["Invalid user"]);

        // Load cart
        var cartItems = await _ctx.UserCarts
            .AsNoTracking()
            .Where(c => c.UserId == currentUser.UserID)
            .ToListAsync(ct);

        if (cartItems.Count == 0)
            return APIResponse<StoreOrderSummaryDTO>.Failure("Cart is empty");

        // Verify address ownership
        var address = await _addressService.GetAddressAsync(request.AddressId);
        if (address is null) return APIResponse<StoreOrderSummaryDTO>.Failure("Address not found");

        // Ensure customer profile exists (auto-create if first order)
        var profileDto = await _customerService.GetOrCreateProfileAsync(currentUser.UserID);

        // Load products for snapshot
        var nodeIds = cartItems.Select(c => c.NodeId).Distinct().ToArray();
        var products = await _ctx.Products
            .AsNoTrackingWithIdentityResolution()
            .Where(x => nodeIds.Contains(x.NodeID))
            .Include(x => x.ProductVariants)
            .Include(x => x.ProductCombos)
            .ThenInclude(x => x.Product)
            .Select(x => new
            {
                x.ProductName,
                x.ProductCombos,
                x.ProductVariants,
                x.Images,
                x.NodeID,
                x.Price,
                x.PriceDiscount,
                x.IsCombo
            }).ToDictionaryAsync(x => x.NodeID);

        var allImageGuids = products.SelectMany(p => p.Value.Images).Distinct().ToArray();
        var mediaByGuids = (await _mediaService.GetMediaItemsAsync(allImageGuids))?.ToDictionary(x => x.FileID) ?? [];
        var minQty = cartItems.Min(x => x.Quantity);
        var rules = await _discountRuleService.GetDiscountForProductsAsync(nodeIds, minQty);

        // Build order items
        var orderItems = new ConcurrentBag<OrderItem>();

        Parallel.ForEach(cartItems, cartItem =>
        {
            var product = products.GetValueOrDefault(cartItem.NodeId);
            if (product is null) return;
            var variant = product.ProductVariants.FirstOrDefault(x => x.ItemID == cartItem.VariantId);
            if (variant is null && !product.IsCombo) return;

            decimal unitPrice = product.Price;
            var productRules = rules.GetValueOrDefault(cartItem.NodeId, []);

            if (product.IsCombo)
            {
                var discountPercent = product.PriceDiscount / 100;
                var totalComboPrices = product.ProductCombos.Sum(c =>
                {
                    var finalPrice = c.Product.Price - c.Product.PriceDiscount;
                    return OrderHelper.CaculateUnitPrice(finalPrice, cartItem.Quantity, productRules);
                });
                unitPrice = discountPercent > 0 ? totalComboPrices * (1 - discountPercent) : totalComboPrices;
            }
            else
            {
                unitPrice = OrderHelper.CaculateUnitPrice((product.Price - product.PriceDiscount), cartItem.Quantity, productRules);
            }

            var lineTotal = unitPrice * cartItem.Quantity;

            orderItems.Add(new OrderItem
            {
                ProductId = cartItem.NodeId,
                VariantId = cartItem.VariantId,
                ProductName = product.ProductName,
                VariantName = variant?.VariantName,
                SKU = variant?.SKU,
                ImageUrl = mediaByGuids!.GetValueOrDefault(product.Images.FirstOrDefault(), null)?.FileURL,
                Quantity = cartItem.Quantity,
                UnitPrice = unitPrice,
                Discount = 0,
                Total = lineTotal,
                ShiprelayId = variant?.ShiprelayId ?? 0
            });
        });

        if (orderItems.Count == 0)
            return APIResponse<StoreOrderSummaryDTO>.Failure("No valid products found in cart");

        var subTotal = orderItems.Sum(x => x.Total);

        // Get tax rate from settings (fallback to 0)
        decimal taxRate = 0;
        var taxSetting = await _settingKeyValueRepository.GetSettingKeyAsync("TAX_RATE");
        if (taxSetting?.SettingValue is not null && decimal.TryParse(taxSetting.SettingValue, out var parsedRate))
            taxRate = parsedRate;

        // Get shipping threshold
        decimal shippingFee = 9.99m;
        var shippingSetting = await _settingKeyValueRepository.GetSettingKeyAsync("FREE_SHIPPING");
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
            RecipientName = currentUser.FullName,
            Email = currentUser.Email,
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
                ProductId = i.ShiprelayId,
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
            CustomerName = currentUser.FullName,
            CustomerEmail = currentUser.Email,
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
            OrderItems = orderItems.ToList()
        };

        _ctx.Orders.Add(order);
        await _ctx.SaveChangesAsync(ct);

        var rateResults = await _shiprelayService.GetRatesAsync(new ShiprelayRateRequestDTO
        {
            OrderId = 0,
            RecipientName = currentUser.FullName,
            Address1 = address.Address,
            City = address.City,
            Region = address.State ?? string.Empty,
            Country = "US",
            Zip = address.ZipCode,
            Phone = address.Phone,
            Email = currentUser.Email,
            Items = orderItems.Select(x => new ShiprelayItemDTO
            {
                ProductId = x.ShiprelayId,
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
        await _ctx.UserCarts.Where(c => c.UserId == currentUser.UserID).ExecuteDeleteAsync(ct);

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
        var userId = _userService.GetCurrentUserId();
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
        var userId = _userService.GetCurrentUserId();
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
    public async Task<APIResponse<StoreCheckoutPreviewDTO>> GetCheckoutPreviewAsync(StoreCheckoutPreviewRequest request, CancellationToken cancellationToken)
    {
        var currentUser = await _userService.GetCurrentUserAsync();
        if (currentUser == null) return APIResponse<StoreCheckoutPreviewDTO>.Failure(ErrorStatusNameConstants.NOT_FOUND, ["Invalid user"]);

        var cartItems = await _ctx.UserCarts
            .AsNoTracking()
            .Where(c => c.UserId == currentUser.UserID)
            .ToListAsync(cancellationToken);

        if (cartItems.Count == 0) return APIResponse<StoreCheckoutPreviewDTO>.Failure("Cart is empty");

        var address = await _addressService.GetAddressAsync(request.AddressId);
        if (address is null)
            return APIResponse<StoreCheckoutPreviewDTO>.Failure("Address not found");
        var nodeIds = cartItems.Select(c => c.NodeId).Distinct().ToArray();
        var variantIds = cartItems.Select(x => x.VariantId).Distinct().ToArray();

        var products = await _ctx.Products
            .AsNoTrackingWithIdentityResolution()
            .Where(x => nodeIds.Contains(x.NodeID))
            .Include(x => x.ProductVariants)
            .Include(x => x.ProductCombos)
            .ThenInclude(x => x.Product)
            .Select(x => new
            {
                x.ProductName,
                x.ProductCombos,
                x.ProductVariants,
                x.Images,
                x.NodeID,
                x.Price,
                x.PriceDiscount,
                x.IsCombo
            }).ToDictionaryAsync(x => x.NodeID);

        var allImageGuids = products.SelectMany(p => p.Value.Images).Distinct().ToArray();
        var mediaByGuids = (await _mediaService.GetMediaItemsAsync(allImageGuids))?.ToDictionary(x => x.FileID) ?? [];
        var minQty = cartItems.Min(x => x.Quantity);
        var rules = await _discountRuleService.GetDiscountForProductsAsync(nodeIds, minQty);
        var previewItems = new ConcurrentBag<StoreCheckoutPreviewItemDTO>();

        Parallel.ForEach(cartItems, cartItem =>
        {
            var product = products.GetValueOrDefault(cartItem.NodeId);
            if (product is null) return;
            var variant = product.ProductVariants.FirstOrDefault(x => x.ItemID == cartItem.VariantId);
            if (variant is null && !product.IsCombo) return;

            decimal unitPrice = product.Price;
            var productRules = rules.GetValueOrDefault(cartItem.NodeId, []);

            if (product.IsCombo)
            {
                var discountPercent = product.PriceDiscount / 100;
                var totalComboPrices = product.ProductCombos.Sum(c =>
                {
                    var finalPrice = c.Product.Price - c.Product.PriceDiscount;
                    return OrderHelper.CaculateUnitPrice(finalPrice, cartItem.Quantity, productRules);
                });
                unitPrice = discountPercent > 0 ? totalComboPrices * (1 - discountPercent) : totalComboPrices;
            }
            else
            {
                unitPrice = OrderHelper.CaculateUnitPrice((product.Price - product.PriceDiscount), cartItem.Quantity, productRules);
            }

            var linePrice = unitPrice * cartItem.Quantity;

            previewItems.Add(new StoreCheckoutPreviewItemDTO
            {
                NodeId = cartItem.NodeId,
                ProductName = product.ProductName,
                VariantName = variant?.VariantName,
                SKU = variant?.SKU,
                Image = mediaByGuids!.GetValueOrDefault(product.Images.FirstOrDefault(), null)?.FileURL,
                UnitPrice = unitPrice,
                Quantity = cartItem.Quantity,
                LineTotal = linePrice,
                ProductId = variant!.ShiprelayId,
            });
        });

        var subTotal = previewItems.Sum(x => x.LineTotal);

        if (previewItems.Count == 0)
            return APIResponse<StoreCheckoutPreviewDTO>.Failure("No valid products found in cart");

        decimal taxRate = 0;
        var taxSetting = await _settingKeyValueRepository.GetSettingKeyAsync("TAX_RATE");
        if (taxSetting?.SettingValue is not null && decimal.TryParse(taxSetting.SettingValue, out var parsedRate))
            taxRate = parsedRate;
        decimal shippingFee = 0;
        var rates = await _shiprelayService.GetRatesAsync(new ShiprelayRateRequestDTO
        {
            OrderId = 0,
            RecipientName = currentUser.FullName,
            Address1 = address.Address,
            City = address.City,
            Region = address.State ?? string.Empty,
            Country = address.Country,
            Zip = address.ZipCode,
            Phone = address.Phone,
            Email = currentUser.Email,
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
        var userId = _userService.GetCurrentUserId();
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
        var userId = _userService.GetCurrentUserId();
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
        var userId = _userService.GetCurrentUserId();
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
