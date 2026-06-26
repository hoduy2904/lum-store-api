using LumStoreAPI.Application.DTOs.OrderDTO;
using LumStoreAPI.Application.DTOs.ProductDTO;
using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Application.DTOs.ShiprelayDTO;
using LumStoreAPI.Application.DTOs.StoreOrderDTO;
using LumStoreAPI.Application.Exceptions;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Entities.Integrations;
using LumStoreAPI.Core.Entities.Orders;
using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.Core.Models.Enums;
using LumStoreAPI.Infrastructure;
using LumStoreAPI.Infrastructure.Extensions;
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
    private readonly IEmailService _emailService;
    private readonly ICartService _cartService;

    public StoreOrderService(
        ICustomerService customerService,
        IShiprelayService shiprelayService,
        IOrderService orderService,
        LumStoreContext ctx,
        IHttpContextAccessor httpContextAccessor,
        IPaymentService paymentService,
        IDiscountRuleService discountRuleService,
        IEmailService emailService,
        ICartService cartService)
    {
        _customerService = customerService;
        _shiprelayService = shiprelayService;
        _orderService = orderService;
        _ctx = ctx;
        _httpContextAccessor = httpContextAccessor;
        _paymentService = paymentService;
        _discountRuleService = discountRuleService;
        _emailService = emailService;
        _cartService = cartService;
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

    /// <summary>
    /// Applies best matching discount rule in-memory using pre-loaded tiers.
    /// Formula: Max(0, Round(unitPrice × (1 − DiscountPercent/100) − DiscountAmount, 2))
    /// </summary>
    private static decimal ApplyBestDiscount(
        IEnumerable<ProductDiscountTierDTO> tiers, decimal unitPrice, int quantity)
    {
        var best = tiers
            .Where(t => t.MinQuantity <= quantity && (t.MaxQuantity == null || t.MaxQuantity >= quantity))
            .OrderByDescending(t => t.DiscountPercent + t.DiscountAmount)
            .FirstOrDefault();

        if (best is null) return unitPrice;
        return Math.Max(0, Math.Round(unitPrice * (1 - best.DiscountPercent / 100) - best.DiscountAmount, 2));
    }


    // ── Commands ──────────────────────────────────────────────────────────────

    public async Task<APIResponse<StoreOrderSummaryDTO>> PlaceOrderAsync(StorePlaceOrderRequest request, CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();


        // Verify address ownership
        var address = await _ctx.CustomerAddresses
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.ItemID == request.AddressId && a.UserId == userId, ct);

        if (address is null)
            return APIResponse<StoreOrderSummaryDTO>.Failure("Address not found");

        // Load user
        var user = await _ctx.Users.AsNoTracking().FirstOrDefaultAsync(u => u.ItemID == userId, ct);
        if (user is null)
            return APIResponse<StoreOrderSummaryDTO>.Failure("User not found");

        var orderItems = new List<OrderItem>();
        var shipmentItems = new List<ShiprelayItemDTO>();
        var previewSnapshots = new List<OrderPreviewItemDTO>();

        decimal subTotal = 0;

        var cartItems = await _cartService.BuildCartResponseAsync(userId, (comboPrices, cartItem) =>
        {
            var product = (cartItem.Product!.Fields as ProductClientDTO)!;
            var variant = cartItem.VariantId.HasValue
                ? product.ProductVariants.FirstOrDefault(v => v.VariantId == cartItem.VariantId)
                : null;


            var discountPerUnit = cartItem.BasePrice - cartItem.UnitPrice;
            var lineTotal = cartItem.UnitPrice * cartItem.Quantity;
            subTotal += lineTotal;

            orderItems.Add(new OrderItem
            {
                ProductId = cartItem.NodeID,
                VariantId = cartItem.VariantId,
                ProductName = product.ProductName,
                VariantName = variant?.VariantName,
                SKU = variant?.SKU,
                ImageUrl = product.Images.FirstOrDefault(),
                Quantity = cartItem.Quantity,
                UnitPrice = cartItem.BasePrice,
                Discount = discountPerUnit,
                Total = lineTotal
            });

            if (product?.IsCombo == true)
            {
                var comboResult = comboPrices.GetValueOrDefault(cartItem.NodeID);
                if (comboResult?.Items is not null)
                    foreach (var ci in comboResult.Items)
                    {
                        shipmentItems.Add(new ShiprelayItemDTO
                        {
                            ProductId = ci.ShiprelayId,
                            Quantity = cartItem.Quantity,
                            Price = ci.DiscountedPrice
                        });
                    }
            }
            else
            {
                shipmentItems.Add(new ShiprelayItemDTO
                {
                    ProductId = variant?.ShiprelayId ?? 0,
                    Quantity = cartItem.Quantity,
                    Price = cartItem.UnitPrice
                });
            }
        });

        if (cartItems.ItemCount == 0)
            return APIResponse<StoreOrderSummaryDTO>.Failure("Cart is empty");
        // Ensure customer profile exists (auto-create if first order)
        var profileDto = await _customerService.GetOrCreateProfileAsync(userId);

        if (orderItems.Count == 0)
            return APIResponse<StoreOrderSummaryDTO>.Failure("No valid products found in cart");

        // Get tax rate from settings (fallback to 0)
        decimal taxRate = 0;
        var taxSetting = await _ctx.SettingKeyValues
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SettingCode == "TAX_RATE", ct);
        if (taxSetting?.SettingValue is not null && decimal.TryParse(taxSetting.SettingValue, out var parsedRate))
            taxRate = parsedRate;

        // Get free-shipping threshold
        decimal shippingThreshold = 0;
        var shippingSetting = await _ctx.SettingKeyValues
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SettingCode == "FREE_SHIPPING", ct);
        if (shippingSetting?.SettingValue is not null && decimal.TryParse(shippingSetting.SettingValue, out var parsedThreshold))
            shippingThreshold = parsedThreshold;

        // Build items list for CreateShipment — combo items expanded into their sub-variants

        // Guard: all items must be synced to ShipRelay before an order can be placed
        if (shipmentItems.Any(i => i.ProductId == 0))
            return APIResponse<StoreOrderSummaryDTO>.Failure(
                "One or more items in your cart have not been synced for shipping. Please contact support.");

        // Fetch actual shipping rate BEFORE creating the shipment or saving the order
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
            Items = shipmentItems
        });

        var selectedRate = rateResults.FirstOrDefault(r => r.CarrierId == request.ShippingServiceCode);
        decimal shippingFee = selectedRate?.Price ?? 9.99m;
        if (shippingThreshold > 0 && subTotal >= shippingThreshold) shippingFee = 0;

        var tax = Math.Round(subTotal * taxRate, 2);
        var total = subTotal + shippingFee + tax;

        var orderCode = GenerateOrderCode();

        // Save order as Pending FIRST so it exists in DB before ShipRelay fires the queued webhook
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
            ShippingState = address.State ?? "",
            ShippingZip = address.ZipCode,
            ShippingCountry = "US",
            CustomerNote = request.Note,
            SubTotal = subTotal,
            ShippingFee = shippingFee,
            Tax = tax,
            Total = total,
            Status = OrderStatus.Pending,
            PaymentStatus = PaymentStatus.Unpaid,
            ShippingServiceName = selectedRate?.CarrierName,
            ShippingServiceDescription = selectedRate?.Description,
            EstimatedDeliveryMin = selectedRate?.MinDeliveryDate.HasValue == true
                ? new DateTimeOffset(DateTime.SpecifyKind(selectedRate.MinDeliveryDate.Value, DateTimeKind.Utc))
                : null,
            EstimatedDeliveryMax = selectedRate?.MaxDeliveryDate.HasValue == true
                ? new DateTimeOffset(DateTime.SpecifyKind(selectedRate.MaxDeliveryDate.Value, DateTimeKind.Utc))
                : null,
            OrderItems = orderItems
        };

        _ctx.Orders.Add(order);
        await _ctx.SaveChangesAsync(ct);  // commit now so webhook handler can find this order

        // Call ShipRelay AFTER saving — order already in DB when webhook fires
        var shipmentDto = new ShiprelayCreateShipmentDTO
        {
            OrderId = order.ItemID,
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
            Items = shipmentItems
        };

        ShiprelayShipmentResult shipResult;
        bool needsReconciliation = false;
        string? reconciliationRawResponse = null;
        try
        {
            shipResult = await _shiprelayService.CreateShipmentAsync(shipmentDto);
        }
        catch (ShiprelayIntegrationException ex) when (ex.ShipmentMayExistOnRemote)
        {
            shipResult = new ShiprelayShipmentResult { Success = true, ShipmentId = "" };
            needsReconciliation = true;
            reconciliationRawResponse = ex.RawResponse ?? ex.Message;
        }

        if (!shipResult.Success)
        {
            // ShipRelay rejected — delete the pending order and abort
            _ctx.Orders.Remove(order);
            await _ctx.SaveChangesAsync(ct);
            return APIResponse<StoreOrderSummaryDTO>.Failure(
                shipResult.ErrorMessage ?? "Unable to create shipment. Please try again.");
        }

        // ShipRelay accepted — promote order to Confirmed and persist shipment data
        order.Status = OrderStatus.Confirmed;
        order.ShiprelayShipmentId = shipResult.ShipmentId;
        order.TrackingNumber = shipResult.TrackingNumber;
        order.TrackingUrl = shipResult.TrackingUrl;
        order.ShippingCarrier = shipResult.Carrier;
        order.ShippingService = shipResult.Service;
        order.NeedsShiprelayReconciliation = needsReconciliation;
        await _ctx.SaveChangesAsync(ct);

        if (needsReconciliation)
        {
            _ctx.ShiprelayReconciliationLogs.Add(new ShiprelayReconciliationLog
            {
                OrderId = order.ItemID,
                OrderCode = order.OrderCode,
                RawResponse = reconciliationRawResponse ?? string.Empty,
                IsResolved = false
            });
            await _ctx.SaveChangesAsync(ct);
        }

        var paymentUrl = await _paymentService.PaymentCheckoutAsync(new() { Order = order, SuccessUrl = request.SuccessUrl, CancelUrl = request.CancelUrl, ShippingFee = shippingFee });

        // Log initial history
        _ctx.OrderHistories.Add(new OrderHistory
        {
            OrderId = order.ItemID,
            ToStatus = OrderStatus.Confirmed,
            Comment = needsReconciliation
                ? "Order confirmed — ShipRelay response unparseable, manual reconciliation required"
                : "Order confirmed via ShipRelay",
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

        // Build preview snapshot for the summary (max 3 items)
        var previewItemsSnapshot = orderItems.Take(3).Select(i =>
        {
            return new OrderPreviewItemDTO
            {
                ProductName = i.ProductName,
                Image = i.ImageUrl,
                VariantName = i.VariantName,
                Price = i.UnitPrice,
                Quantity = i.Quantity
            };
        }).ToList();

        return APIResponse<StoreOrderSummaryDTO>.Success(new StoreOrderSummaryDTO
        {
            OrderId = order.ItemID,
            OrderCode = order.OrderCode,
            Status = order.Status.ToString().ToLower(),
            PaymentStatus = order.PaymentStatus.ToString().ToLower(),
            Total = order.Total,
            ItemCount = orderItems.Sum(i => i.Quantity),
            CreatedAt = order.CreatedAt,
            PaymentURL = paymentUrl,
            PreviewItems = previewItemsSnapshot
        }, ["Order placed and confirmed"]);

    }

    // ── Queries ───────────────────────────────────────────────────────────────

    public async Task<PagedResponse<StoreOrderSummaryDTO>> GetOrdersAsync(int page, int pageSize, string? status = null, CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        var profileDto = await _customerService.GetOrCreateProfileAsync(userId);

        var parsedStatus = status is not null && Enum.TryParse<OrderStatus>(status, ignoreCase: true, out var s) ? s : (OrderStatus?)null;

        var query = _ctx.Orders
            .Where(o => o.CustomerId == profileDto.ProfileId)
            .Where(o => parsedStatus == null || o.Status == parsedStatus)
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
            TrackingNumber = o.TrackingNumber,
            TrackingUrl = o.TrackingUrl,
            ShippingCarrier = o.ShippingCarrier,
            ShippingService = o.ShippingService,
            ShippingServiceName = o.ShippingServiceName,
            ShippingServiceDescription = o.ShippingServiceDescription,
            EstimatedDeliveryMin = o.EstimatedDeliveryMin,
            EstimatedDeliveryMax = o.EstimatedDeliveryMax,
            ShippedAt = o.ShippedAt,
            DeliveredAt = o.DeliveredAt,
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
            ShippingCarrier = order.ShippingCarrier,
            ShippingService = order.ShippingService,
            ShippingServiceName = order.ShippingServiceName,
            ShippingServiceDescription = order.ShippingServiceDescription,
            EstimatedDeliveryMin = order.EstimatedDeliveryMin,
            EstimatedDeliveryMax = order.EstimatedDeliveryMax,
            ShippedAt = order.ShippedAt,
            DeliveredAt = order.DeliveredAt,
            PreviewItems = order.OrderItems.Take(3).Select(i => new OrderPreviewItemDTO
            {
                ProductName = i.ProductName,
                Image = i.ImageUrl,
                VariantName = i.VariantName,
                Price = i.UnitPrice,
                Quantity = i.Quantity
            }),
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
            Discount = order.Discount,
            Shipping = order.ShippingFee,
            Tax = order.Tax
        };

        return APIResponse<StoreOrderDetailDTO>.Success(detail);
    }

    // ── Tracking ──────────────────────────────────────────────────────────────

    public async Task<APIResponse<StoreOrderTrackingDTO?>> GetTrackingAsync(int orderId, CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        var profileDto = await _customerService.GetOrCreateProfileAsync(userId);

        var order = await _ctx.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.ItemID == orderId, ct);

        if (order is null)
            return APIResponse<StoreOrderTrackingDTO?>.Failure("Order not found");

        if (order.CustomerId != profileDto.ProfileId)
            return APIResponse<StoreOrderTrackingDTO?>.Failure("Forbidden");

        // Order not yet shipped — return null data (not an error)
        if (string.IsNullOrEmpty(order.ShiprelayShipmentId))
            return APIResponse<StoreOrderTrackingDTO?>.Success(null);

        // Try live tracking from ShipRelay; fall back to stored fields on failure
        ShiprelayTrackingResult? live = null;
        try { live = await _shiprelayService.GetTrackingAsync(order.ShiprelayShipmentId); }
        catch { /* non-fatal — use stored data */ }

        // Persist carrier back to DB if it was missing and live fetch returned it
        if (order.ShippingCarrier is null && live?.Carrier is not null)
        {
            await _ctx.Orders
                .Where(o => o.ItemID == orderId)
                .ExecuteUpdateAsync(s => s.SetProperty(o => o.ShippingCarrier, live.Carrier), ct);
        }

        return APIResponse<StoreOrderTrackingDTO?>.Success(new StoreOrderTrackingDTO
        {
            TrackingNumber = live?.TrackingNumber ?? order.TrackingNumber,
            TrackingUrl = live?.TrackingUrl ?? order.TrackingUrl,
            Carrier = live?.Carrier ?? order.ShippingCarrier,
            Status = live?.StatusDescription ?? live?.Status ?? order.Status.ToString(),
            ShippedAt = order.ShippedAt,
            DeliveredAt = order.DeliveredAt ?? live?.DeliveredAt
        });
    }

    // ── Checkout Preview ──────────────────────────────────────────────────────

    public async Task<APIResponse<StoreCheckoutPreviewDTO>> GetCheckoutPreviewAsync(
        StoreCheckoutPreviewRequest request, CancellationToken ct = default)
    {
        var previewRateItems = new List<ShiprelayItemDTO>();
        var previewItems = new List<StoreCheckoutPreviewItemDTO>();
        decimal subTotal = 0;

        var userId = GetCurrentUserId();

        var address = await _ctx.CustomerAddresses
            .Include(x => x.User)
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync(a => a.ItemID == request.AddressId && a.UserId == userId, ct);

        if (address is null)
            return APIResponse<StoreCheckoutPreviewDTO>.Failure("Address not found");

        var cartItems = await _cartService.BuildCartResponseAsync(userId, (comboPrices, cartItem) =>
        {
            var product = (cartItem.Product!.Fields as ProductClientDTO)!;
            var variant = cartItem.VariantId.HasValue
                ? product.ProductVariants.FirstOrDefault(v => v.VariantId == cartItem.VariantId)
                : null;

            var lineTotal = cartItem.UnitPrice * cartItem.Quantity;
            subTotal += lineTotal;

            previewItems.Add(new StoreCheckoutPreviewItemDTO
            {
                NodeId = cartItem.NodeID,
                ProductName = product.ProductName,
                VariantName = variant?.VariantName,
                SKU = variant?.SKU,
                Image = product.Images.FirstOrDefault(),
                OriginalPrice = cartItem.BasePrice,
                UnitPrice = cartItem.UnitPrice,
                Quantity = cartItem.Quantity,
                LineTotal = lineTotal,
                ProductId = variant?.ShiprelayId ?? 0
            });

            if (product?.IsCombo == true)
            {
                var comboResult = comboPrices.GetValueOrDefault(cartItem.NodeID);
                if (comboResult?.Items is not null)
                    foreach (var ci in comboResult.Items)
                    {
                        previewRateItems.Add(new ShiprelayItemDTO
                        {
                            ProductId = ci.ShiprelayId,
                            Quantity = cartItem.Quantity,
                            Price = ci.DiscountedPrice
                        });
                    }
            }
            else
            {
                previewRateItems.Add(new ShiprelayItemDTO
                {
                    ProductId = variant?.ShiprelayId ?? 0,
                    Quantity = cartItem.Quantity,
                    Price = cartItem.UnitPrice
                });
            }
        }, ct: ct);

        if (cartItems.ItemCount == 0)
            return APIResponse<StoreCheckoutPreviewDTO>.Failure("Cart is empty");

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
            Items = previewRateItems
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

    public async Task<APIResponse<StoreOrderSummaryDTO>> CancelOrderAsync(int orderId, StoreCancelOrderRequest request, CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        var profileDto = await _customerService.GetOrCreateProfileAsync(userId);

        var order = await _ctx.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.ItemID == orderId, ct);
        if (order is null)
            return APIResponse<StoreOrderSummaryDTO>.Failure("Order not found");

        if (order.CustomerId != profileDto.ProfileId)
            return APIResponse<StoreOrderSummaryDTO>.Failure("Forbidden");

        var cancellableStatuses = new[] { OrderStatus.Pending, OrderStatus.Confirmed };
        if (!cancellableStatuses.Contains(order.Status))
            return APIResponse<StoreOrderSummaryDTO>.Failure("Order cannot be cancelled at this stage");

        await _orderService.UpdateOrderStatusAsync(orderId, new OrderUpdateStatusDTO
        {
            NewStatus = OrderStatus.Cancelled,
            Comment = request.Reason ?? "Cancelled by customer"
        });

        string refundMessage = "Order cancelled successfully";
        if (order.PaymentStatus == PaymentStatus.Paid && !string.IsNullOrEmpty(order.PaymentIntentId))
        {
            var localRefund = await _orderService.CreateReturnAsync(order.ItemID, new OrderReturnCreateDTO
            {
                Items = order.OrderItems.Select(x => new ReturnItemDTO
                {
                    OrderItemId = x.ItemID,
                    Quantity = x.Quantity,
                    Reason = "Cancelled order",
                }).ToList(),
                Reason = "Cancelled order"
            });

            var stripeReturn = await _paymentService.CreateRefundAsync(new(localRefund.ReturnId, order.OrderCode, order.PaymentIntentId), order);
            if (stripeReturn is null)
            {
                refundMessage = "Cancelled success, but need contact us to refund";
            }
        }

        return APIResponse<StoreOrderSummaryDTO>.Success(new StoreOrderSummaryDTO
        {
            OrderId = order.ItemID,
            OrderCode = order.OrderCode,
            Status = OrderStatus.Cancelled.ToString().ToLower(),
            PaymentStatus = order.PaymentStatus.ToString().ToLower(),
            Total = order.Total,
            ItemCount = order.OrderItems.Sum(i => i.Quantity),
            CreatedAt = order.CreatedAt,
            PreviewItems = order.OrderItems.Take(3).Select(i => new OrderPreviewItemDTO
            {
                ProductName = i.ProductName,
                Image = i.ImageUrl,
                VariantName = i.VariantName,
                Price = i.UnitPrice,
                Quantity = i.Quantity
            })
        }, [refundMessage]);
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

    public async Task<APIResponse<StoreOrderSummaryDTO>> RequestReturnAsync(
        int orderId, StoreRequestReturnRequest request, CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        var profileDto = await _customerService.GetOrCreateProfileAsync(userId);

        var order = await _ctx.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.ItemID == orderId, ct);

        if (order is null)
            return APIResponse<StoreOrderSummaryDTO>.Failure("Order not found");

        if (order.CustomerId != profileDto.ProfileId)
            return APIResponse<StoreOrderSummaryDTO>.Failure("Forbidden");

        if (order.Status != OrderStatus.Delivered)
            return APIResponse<StoreOrderSummaryDTO>.Failure("Returns can only be requested for delivered orders");

        if (order.PaymentStatus != Core.Models.Enums.PaymentStatus.Paid)
            return APIResponse<StoreOrderSummaryDTO>.Failure("Returns can only be requested for paid orders");

        await _orderService.CreateReturnAsync(orderId, new OrderReturnCreateDTO
        {
            Reason = request.Reason ?? "Return requested by customer",
            Items = []
        });

        return APIResponse<StoreOrderSummaryDTO>.Success(new StoreOrderSummaryDTO
        {
            OrderId = order.ItemID,
            OrderCode = order.OrderCode,
            Status = OrderStatus.ReturnRequested.ToString().ToLower(),
            PaymentStatus = order.PaymentStatus.ToString().ToLower(),
            Total = order.Total,
            ItemCount = order.OrderItems.Sum(i => i.Quantity),
            CreatedAt = order.CreatedAt,
            TrackingNumber = order.TrackingNumber,
            TrackingUrl = order.TrackingUrl,
            ShippingCarrier = order.ShippingCarrier,
            PreviewItems = order.OrderItems.Take(3).Select(i => new OrderPreviewItemDTO
            {
                ProductName = i.ProductName,
                Image = i.ImageUrl,
                VariantName = i.VariantName,
                Price = i.UnitPrice,
                Quantity = i.Quantity
            })
        }, ["Return request submitted successfully"]);
    }

    // ── Payment ───────────────────────────────────────────────────────────────

    public async Task<APIResponse<PaymentStatusCheckDTO>> CheckPaymentStatusAsync(int orderId, CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        var profileDto = await _customerService.GetOrCreateProfileAsync(userId);

        var order = await _ctx.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.ItemID == orderId, ct);

        if (order is null)
            return APIResponse<PaymentStatusCheckDTO>.Failure("Order not found");

        if (order.CustomerId != profileDto.ProfileId)
            return APIResponse<PaymentStatusCheckDTO>.Failure("Forbidden");

        return APIResponse<PaymentStatusCheckDTO>.Success(new PaymentStatusCheckDTO
        {
            OrderId = orderId,
            PaymentStatus = order.PaymentStatus.ToString().ToLower(),
            OrderStatus = order.Status.ToString().ToLower(),
            IsPaid = order.PaymentStatus == Core.Models.Enums.PaymentStatus.Paid
        });
    }
}
