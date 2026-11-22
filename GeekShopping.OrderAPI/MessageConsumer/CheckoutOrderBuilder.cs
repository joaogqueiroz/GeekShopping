using GeekShopping.OrderAPI.Messages;
using GeekShopping.OrderAPI.Model;

namespace GeekShopping.OrderAPI.MessageConsumer
{
    // Turns a checkout message into the order that is saved and the payment request that follows.
    public static class CheckoutOrderBuilder
    {
        public static OrderHeader BuildOrder(CheckoutHeaderVO vo)
        {
            OrderHeader order = new()
            {
                UserId = vo.UserId,
                CouponCode = vo.CouponCode,
                PurchaseAmount = vo.PurchaseAmount,
                DiscountTotal = vo.DiscountTotal,
                FirstName = vo.FirstName,
                LastName = vo.LastName,
                DateTime = vo.DateTime,
                OrderTime = DateTime.Now,
                Phone = vo.Phone,
                Email = vo.Email,
                // The saved order keeps only the last four digits. The full card travels in the payment
                // message and the CVV is never stored.
                CardNumber = MaskCardNumber(vo.CardNumber),
                CVV = null,
                ExpiryMonthYear = vo.ExpiryMonthYear,
                PaymentStatus = false,
                OrderDetails = new List<OrderDetail>(),
            };

            foreach (var orderDetail in vo.CartDetails ?? Enumerable.Empty<CartDetailVO>())
            {
                OrderDetail detail = new()
                {
                    ProductId = orderDetail.ProductId,
                    ProductName = orderDetail.Product.Name,
                    Price = orderDetail.Product.Price,
                    Count = orderDetail.Count,
                };
                // OrderTotal is the number of items (OrderHeader starts it at 0). An item without a
                // quantity would turn the sum into null, so it counts as 0.
                order.OrderTotal += detail.Count ?? 0;
                order.OrderDetails.Add(detail);
            }

            return order;
        }

        public static string? MaskCardNumber(string? cardNumber)
        {
            var digits = new string((cardNumber ?? "").Where(char.IsDigit).ToArray());
            if (digits.Length == 0) return null;
            return "**** **** **** " + digits[^Math.Min(4, digits.Length)..];
        }

        // Built after the order is saved, so OrderId is the id the database assigned. The card comes from
        // the checkout message because the saved order no longer holds it.
        public static PaymentVO BuildPayment(OrderHeader order, CheckoutHeaderVO checkout) => new()
        {
            Name = order.FirstName + " " + order.LastName,
            CardNumber = checkout.CardNumber,
            CVV = checkout.CVV,
            ExpiryMonthYear = checkout.ExpiryMonthYear,
            OrderId = order.Id,
            PurchaseAmount = order.PurchaseAmount,
            Email = order.Email,
            MessageCreated = DateTime.Now
        };
    }
}
