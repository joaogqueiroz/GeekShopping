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
                CardNumber = vo.CardNumber,
                CVV = vo.CVV,
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

        // Built after the order is saved, so OrderId is the id the database assigned
        public static PaymentVO BuildPayment(OrderHeader order) => new()
        {
            Name = order.FirstName + " " + order.LastName,
            CardNumber = order.CardNumber,
            CVV = order.CVV,
            ExpiryMonthYear = order.ExpiryMonthYear,
            OrderId = order.Id,
            PurchaseAmount = order.PurchaseAmount,
            Email = order.Email,
            MessageCreated = DateTime.Now
        };
    }
}
