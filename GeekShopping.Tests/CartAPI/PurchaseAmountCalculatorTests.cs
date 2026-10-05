using GeekShopping.CartAPI.Data;
using GeekShopping.CartAPI.Data.ValueObjects;

namespace GeekShopping.Tests.CartAPI
{
    public class PurchaseAmountCalculatorTests
    {
        private static CartDetailVO Item(decimal? price, int? count, long id = 1) => new CartDetailVO
        {
            Id = id,
            Count = count,
            Product = price == null ? null : new ProductVO { Id = id, Name = $"Product {id}", Price = price }
        };

        [Fact]
        public void OneItem_IsPriceTimesQuantity()
        {
            Assert.Equal(99.80m, PurchaseAmountCalculator.Calculate(new[] { Item(49.90m, 2) }, 0));
        }

        [Fact]
        public void SeveralItems_AreAddedUp()
        {
            // 49.90 × 2 + 19.99 × 3 + 120.00 × 1 = 99.80 + 59.97 + 120.00
            var items = new[] { Item(49.90m, 2, 1), Item(19.99m, 3, 2), Item(120.00m, 1, 3) };

            Assert.Equal(279.77m, PurchaseAmountCalculator.Calculate(items, 0));
        }

        [Theory]
        [InlineData("0.10", 3, "0.30")]       // the classic floating-point trap: exact with decimal
        [InlineData("33.33", 3, "99.99")]
        [InlineData("0.01", 100, "1.00")]
        [InlineData("19.999", 3, "59.997")]   // no rounding is applied to the stored prices
        public void Amounts_AreExactWithoutFloatingPointErrors(string price, int count, string expected)
        {
            var amount = PurchaseAmountCalculator.Calculate(new[] { Item(decimal.Parse(price, System.Globalization.CultureInfo.InvariantCulture), count) }, 0);

            Assert.Equal(decimal.Parse(expected, System.Globalization.CultureInfo.InvariantCulture), amount);
        }

        [Fact]
        public void LargeOrders_DoNotOverflow()
        {
            Assert.Equal(999_999_990.00m, PurchaseAmountCalculator.Calculate(new[] { Item(999_999.99m, 1000) }, 0));
        }

        [Fact]
        public void Discount_IsSubtractedFromTheTotal()
        {
            var items = new[] { Item(49.90m, 2, 1), Item(19.99m, 3, 2) };   // 159.77

            Assert.Equal(149.77m, PurchaseAmountCalculator.Calculate(items, 10m));
        }

        [Theory]
        [InlineData("99.80")]   // exactly the total
        [InlineData("150")]     // more than the total
        public void DiscountOfTheWholeCartOrMore_MakesItFreeNotNegative(string discount)
        {
            var amount = PurchaseAmountCalculator.Calculate(new[] { Item(49.90m, 2) }, decimal.Parse(discount, System.Globalization.CultureInfo.InvariantCulture));

            Assert.Equal(0m, amount);
        }

        [Fact]
        public void NoItems_IsZero()
        {
            Assert.Equal(0m, PurchaseAmountCalculator.Calculate(Array.Empty<CartDetailVO>(), 0));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]     // a negative quantity would lower the total
        [InlineData(null)]
        public void InvalidQuantity_IsRejected(int? count)
        {
            var error = Assert.Throws<InvalidOperationException>(() =>
                PurchaseAmountCalculator.Calculate(new[] { Item(49.90m, 2, 1), Item(10m, count, 2) }, 0));

            Assert.Contains("quantity", error.Message);
        }

        [Theory]
        [InlineData("0")]
        [InlineData("-10")]
        [InlineData(null)]    // item without a product
        public void InvalidPriceOrMissingProduct_IsRejected(string? price)
        {
            decimal? parsed = price == null ? null : decimal.Parse(price, System.Globalization.CultureInfo.InvariantCulture);

            var error = Assert.Throws<InvalidOperationException>(() => PurchaseAmountCalculator.Calculate(new[] { Item(parsed, 1) }, 0));

            Assert.Contains("price", error.Message);
        }

        [Fact]
        public void NegativeDiscount_IsRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => PurchaseAmountCalculator.Calculate(new[] { Item(49.90m, 1) }, -5m));
        }
    }
}
