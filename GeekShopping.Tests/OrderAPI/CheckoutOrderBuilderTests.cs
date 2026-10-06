using GeekShopping.OrderAPI.MessageConsumer;
using GeekShopping.OrderAPI.Messages;

namespace GeekShopping.Tests.OrderAPI
{
    public class CheckoutOrderBuilderTests
    {
        private static CartDetailVO Item(long productId, string name, decimal price, int? count) => new CartDetailVO
        {
            ProductId = productId,
            Count = count,
            Product = new ProductVO { Id = productId, Name = name, Price = price }
        };

        private static CheckoutHeaderVO Checkout(params CartDetailVO[] items) => new CheckoutHeaderVO
        {
            UserId = "user-1",
            CouponCode = "GEEK10",
            PurchaseAmount = 120m,
            DiscountTotal = 10m,
            FirstName = "Test",
            LastName = "User",
            Email = "user@test.com",
            CardNumber = "4111111111111111",
            CVV = "123",
            ExpiryMonthYear = "12/30",
            CartDetails = items
        };

        [Fact]
        public void OrderTotal_IsTheSumOfTheQuantities()
        {
            var order = CheckoutOrderBuilder.BuildOrder(Checkout(
                Item(1, "T-shirt", 50m, 2),
                Item(2, "Mug", 30m, 1),
                Item(3, "Poster", 15m, 3)));

            Assert.Equal(6, order.OrderTotal);
        }

        [Fact]
        public void OrderTotal_WithOneItem_IsItsQuantity()
        {
            Assert.Equal(4, CheckoutOrderBuilder.BuildOrder(Checkout(Item(1, "T-shirt", 50m, 4))).OrderTotal);
        }

        // With the original += detail.Count, one item without a quantity made the whole total null
        [Fact]
        public void ItemsWithoutQuantity_CountAsZero()
        {
            var order = CheckoutOrderBuilder.BuildOrder(Checkout(Item(1, "T-shirt", 50m, 2), Item(2, "Mug", 30m, null)));

            Assert.Equal(2, order.OrderTotal);
            Assert.Equal(2, order.OrderDetails!.Count);
        }

        [Fact]
        public void Items_CopyProductNamePriceAndQuantity()
        {
            var order = CheckoutOrderBuilder.BuildOrder(Checkout(Item(7, "T-shirt", 49.90m, 2)));

            var detail = Assert.Single(order.OrderDetails!);
            Assert.Equal(7, detail.ProductId);
            Assert.Equal("T-shirt", detail.ProductName);
            Assert.Equal(49.90m, detail.Price);
            Assert.Equal(2, detail.Count);
        }

        [Fact]
        public void Order_KeepsTheAmountsAndCustomerFromTheCheckout()
        {
            var order = CheckoutOrderBuilder.BuildOrder(Checkout(Item(1, "T-shirt", 50m, 1)));

            Assert.Equal("user-1", order.UserId);
            Assert.Equal("GEEK10", order.CouponCode);
            Assert.Equal(120m, order.PurchaseAmount);
            Assert.Equal(10m, order.DiscountTotal);
            Assert.Equal("4111111111111111", order.CardNumber);
            Assert.False(order.PaymentStatus);
        }

        [Fact]
        public void CheckoutWithoutItems_GivesAnEmptyOrderInsteadOfThrowing()
        {
            var withNull = Checkout();
            withNull.CartDetails = null;

            foreach (var checkout in new[] { Checkout(), withNull })
            {
                var order = CheckoutOrderBuilder.BuildOrder(checkout);
                Assert.Equal(0, order.OrderTotal);
                Assert.Empty(order.OrderDetails!);
            }
        }

        [Fact]
        public void Payment_CarriesTheOrderIdNameCardAndAmount()
        {
            var order = CheckoutOrderBuilder.BuildOrder(Checkout(Item(1, "T-shirt", 50m, 1)));
            order.Id = 42;   // assigned by the database when the order is saved

            var payment = CheckoutOrderBuilder.BuildPayment(order);

            Assert.Equal(42, payment.OrderId);
            Assert.Equal("Test User", payment.Name);
            Assert.Equal("4111111111111111", payment.CardNumber);
            Assert.Equal("123", payment.CVV);
            Assert.Equal("12/30", payment.ExpiryMonthYear);
            Assert.Equal(120m, payment.PurchaseAmount);
            Assert.Equal("user@test.com", payment.Email);
        }
    }
}
