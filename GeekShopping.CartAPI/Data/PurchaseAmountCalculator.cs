using GeekShopping.CartAPI.Data.ValueObjects;

namespace GeekShopping.CartAPI.Data
{
    // The amount the order is charged. It is computed on the server from the saved cart,
    // because the amount a client sends at checkout cannot be trusted.
    public static class PurchaseAmountCalculator
    {
        public static decimal Calculate(IEnumerable<CartDetailVO> items, decimal discount)
        {
            if (discount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(discount), "A discount cannot be negative.");
            }

            decimal total = 0;
            foreach (var item in items)
            {
                if (item.Product?.Price is not decimal price || price <= 0)
                {
                    throw new InvalidOperationException($"Cart item {item.Id} has no valid product price.");
                }
                if (item.Count is not int count || count <= 0)
                {
                    throw new InvalidOperationException($"Cart item {item.Id} has an invalid quantity.");
                }
                total += price * count;
            }

            // A coupon worth more than the cart makes it free, never negative
            return Math.Max(0, total - discount);
        }
    }
}
