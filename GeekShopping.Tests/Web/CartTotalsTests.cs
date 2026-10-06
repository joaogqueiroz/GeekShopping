using GeekShopping.Web.Models;
using GeekShopping.Web.Utils;

namespace GeekShopping.Tests.Web
{
    public class CartTotalsTests
    {
        private static CartDetailViewModel Item(decimal price, int? count) => new CartDetailViewModel
        {
            Count = count,
            Product = new ProductViewModel { Name = "Product", Price = price }
        };

        [Fact]
        public void Total_IsPriceTimesQuantityForEveryItem()
        {
            // 50.00 × 2 + 30.00 × 1 + 19.99 × 3
            var items = new[] { Item(50.00m, 2), Item(30.00m, 1), Item(19.99m, 3) };

            Assert.Equal(189.97m, CartTotals.PurchaseAmount(items, 0));
        }

        // CartAPI does not send a discount, so without a coupon it arrives as null.
        // The old "PurchaseAmount -= DiscountTotal" made the total null and the page showed it blank.
        [Fact]
        public void NoCoupon_ShowsTheFullTotalInsteadOfNothing()
        {
            Assert.Equal(130.00m, CartTotals.PurchaseAmount(new[] { Item(50.00m, 2), Item(30.00m, 1) }, null));
        }

        [Fact]
        public void Coupon_IsSubtracted()
        {
            Assert.Equal(120.00m, CartTotals.PurchaseAmount(new[] { Item(50.00m, 2), Item(30.00m, 1) }, 10m));
        }

        [Theory]
        [InlineData("130")]     // exactly the total
        [InlineData("500")]     // more than the total: used to show a negative amount
        public void CouponWorthTheWholeCartOrMore_ShowsZero(string discount)
        {
            var amount = CartTotals.PurchaseAmount(new[] { Item(50.00m, 2), Item(30.00m, 1) }, decimal.Parse(discount));

            Assert.Equal(0m, amount);
        }

        [Fact]
        public void EmptyOrMissingCart_ShowsZero()
        {
            Assert.Equal(0m, CartTotals.PurchaseAmount(Array.Empty<CartDetailViewModel>(), null));
            Assert.Equal(0m, CartTotals.PurchaseAmount(null, 10m));
        }

        [Fact]
        public void ItemWithoutQuantityOrProduct_DoesNotBlankTheTotal()
        {
            var items = new[] { Item(50.00m, 2), Item(30.00m, null), new CartDetailViewModel { Count = 3, Product = null } };

            Assert.Equal(100.00m, CartTotals.PurchaseAmount(items, null));
        }
    }
}
