using System;
using LumStoreAPI.Core.Entities.Orders;
using LumStoreAPI.Core.Models.Systems;

namespace LumStoreAPI.Core.Models.Constants;

public class EmailTemplateConstant
{
    public static EmailTemplate USER_ORDER_PAID = new()
    {
        EmailHeader = $@"Order Confirmation & Payment Received - Order #{{{{{nameof(Order.OrderCode)}}}}}",
        EmailBody = $@"
            <html>
            <body>
            <p>Dear {{{{{nameof(Order.CustomerName)}}}}}</p>  
            <p>Thank you for shopping with Lum Store!</p>
            <p>We are thrilled to confirm that we have received your order {{{{{nameof(Order.OrderCode)}}}}} and your  
            payment was successful. We are now preparing your items for shipment.</p>
            <p>Order Summary:</p>
            <ul>
            <li>Order Number: {{{{{nameof(Order.OrderCode)}}}}} </li>
            <li>Order Date: {{{{{nameof(Order.CreatedAt)}}}}}</li>
            <li>Payment Method: [Payment Method, e.g., Credit Card / PayPal]</li>
            </ul>
            Items Ordered:
            <ul>
            {{{{for orderItem in {nameof(Order.OrderItems)}}}}}
            <li> {{{{ orderItem.{nameof(OrderItem.ProductName)}}}}}{{{{(orderItem.{nameof(OrderItem.VariantName)}) }}}} - {{{{ orderItem.{nameof(OrderItem.Total)} }}}}</li>
            {{{{ end }}}}
            </ul>
            <p>Total Paid: {{{{{nameof(Order.Total)}}}}}</p>
            <p>Shipping Address:</p>
            <p>{{{{{nameof(Order.ShippingAddress)}}}}}</p>
            <p>We will send you another email with tracking information as soon as your order ships.  
            If you have any questions or need to make changes to your order, please contact our support  
            team at {{{{LumnailsEmail}}}} or call us at {{{{LumnailsPhone}}}}. </p> 
            <p>Warm regards</p>
            </body>
            </html>
        "
    };

    public static EmailTemplate ADMIN_ORDER_PAID = new()
    {
        EmailHeader = $"ACTION REQUIRED: New Paid Order Received - #{{{{{nameof(Order.OrderCode)}}}}}",
        EmailBody = $$$"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>ACTION REQUIRED: New Paid Order Received - #{{{{{nameof(Order.OrderCode)}}}}}</title>
            </head>
            <body style="margin:0; padding:0; background-color:#f4f4f4; font-family:Arial, sans-serif;">
                <table width="100%" cellpadding="0" cellspacing="0" style="background-color:#f4f4f4; padding:20px 0;">
                    <tr>
                        <td align="center">
                            <table width="600" cellpadding="0" cellspacing="0" style="background-color:#ffffff; border-radius:8px; overflow:hidden;">
                                
                                <tr>
                                    <td style="background-color:#2563eb; color:#ffffff; padding:20px; text-align:center;">
                                        <h2 style="margin:0;">New Paid Order Received - #{{{{{nameof(Order.OrderCode)}}}}}</h2>
                                    </td>
                                </tr>

                                <tr>
                                    <td style="padding:30px; color:#333333; line-height:1.6;">
                                        <p>Hello Admin,</p>

                                        <p>
                                            A new order has been successfully placed and paid for by
                                            <strong>{{{{{nameof(Order.CustomerName)}}}}}</strong>.
                                            Please process this order for fulfillment.
                                        </p>

                                        <h3 style="color:#2563eb;">Order Details</h3>
                                        <table width="100%" cellpadding="8" cellspacing="0" style="border-collapse:collapse;">
                                            <tr>
                                                <td style="border:1px solid #dddddd;"><strong>Order Number</strong></td>
                                                <td style="border:1px solid #dddddd;">{{{{{nameof(Order.OrderCode)}}}}}</td>
                                            </tr>
                                            <tr>
                                                <td style="border:1px solid #dddddd;"><strong>Date & Time</strong></td>
                                                <td style="border:1px solid #dddddd;">{{{{{nameof(Order.CreatedAt)}}}}}</td>
                                            </tr>
                                            <tr>
                                                <td style="border:1px solid #dddddd;"><strong>Customer Email</strong></td>
                                                <td style="border:1px solid #dddddd;">{{{{{nameof(Order.CustomerEmail)}}}}}</td>
                                            </tr>
                                            <tr>
                                                <td style="border:1px solid #dddddd;"><strong>Customer Phone</strong></td>
                                                <td style="border:1px solid #dddddd;">{{{{{nameof(Order.CustomerPhone)}}}}}</td>
                                            </tr>
                                        </table>

                                        <h3 style="color:#2563eb; margin-top:25px;">Financials</h3>
                                        <table width="100%" cellpadding="8" cellspacing="0" style="border-collapse:collapse;">
                                            <tr>
                                                <td style="border:1px solid #dddddd;"><strong>Total Amount</strong></td>
                                                <td style="border:1px solid #dddddd;">{{{{{nameof(Order.Total)}}}}}</td>
                                            </tr>
                                            <tr>
                                                <td style="border:1px solid #dddddd;"><strong>Payment Status</strong></td>
                                                <td style="border:1px solid #dddddd;">
                                                    <span style="color:#16a34a; font-weight:bold;">
                                                        PAID ({{{{{nameof(Order.PaymentMethod)}}}}})
                                                    </span>
                                                </td>
                                            </tr>
                                        </table>

                                        <h3 style="color:#2563eb; margin-top:25px;">Shipping Information</h3>
                                        <div style="padding:15px; background-color:#f8fafc; border:1px solid #e5e7eb; border-radius:6px;">
                                            {{{{{nameof(Order.ShippingAddress)}}}}}
                                        </div>

                                        <p style="margin-top:30px;">
                                            Best,<br>
                                            <strong>System Notification</strong>
                                        </p>
                                    </td>
                                </tr>

                                <tr>
                                    <td style="background-color:#f8fafc; text-align:center; padding:15px; color:#666666; font-size:12px;">
                                        This is an automated notification email. Please do not reply.
                                    </td>
                                </tr>

                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>
            """
    };

    public static EmailTemplate USER_CANCELLED_ORDER = new()
    {
        EmailHeader = $"Order Cancellation Confirmation - Order #{{{nameof(Order.OrderCode)}}}",
        EmailBody = $$$"""
                <p>Dear {{{{{nameof(Order.CustomerName)}}}}},</p>

                <p>
                    As requested, your order #{{{{{nameof(Order.OrderCode)}}}}} has been successfully cancelled.
                </p>

                <p>
                    <strong>Refund Information:</strong><br>
                    Since your order was already paid, we have initiated a full refund of
                    {{{{{nameof(Order.Total)}}}}} to your original payment method. Please allow
                    2 to 5 business days for the funds to appear
                    in your account, depending on your bank's processing time.
                </p>

                <p>
                    We are sorry that this order didn't work out. If you cancelled by mistake
                    or need help finding something else, we would love to assist you! Feel free
                    to reply to this email or visit our website.
                </p>

                <p>
                    Thank you for considering Lum Store, and we hope to serve you again in
                    the future.
                </p>

                <p>
                    Best regards,<br><br>
                    The Lum Store Team<br>
                </p>
                """
    };

    public static EmailTemplate ADMIN_CANCELLED_ORDER = new()
    {
        EmailHeader = $"ALERT: Order Cancelled by User - #{{{nameof(Order.OrderCode)}}}",
        EmailBody = $$$"""
            <p>Hello Admin,</p>

            <p>
                Please be advised that {{{{{nameof(Order.CustomerName)}}}}} has cancelled their order
                #{{{{{nameof(Order.OrderCode)}}}}}.
            </p>

            <p><strong>Cancellation Details:</strong></p>

            <p>
                • Order Number: {{{{{nameof(Order.OrderCode)}}}}}<br>
                • Cancellation Reason: {{{{{nameof(Order.OrderNotes)}}}}}<br>
                • Total Amount: {{{{{nameof(Order.Total)}}}}}<br>
                • Payment Status: PAID (Needs Refund)
            </p>

            <p><strong>Action Required:</strong></p>

            <p>
                1. Please verify that the automated refund process has been triggered via
                {{{{{nameof(Order.PaymentMethod)}}}}}, or process the refund manually if required.<br>
                2. Ensure that the items are restocked in the inventory.
            </p>

            <p>
                Best,<br>
                System Notification
            </p>
            """
    };
}
