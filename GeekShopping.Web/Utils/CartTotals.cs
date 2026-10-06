using GeekShopping.Web.Models;

namespace GeekShopping.Web.Utils
{
    // The total shown on the cart page. CartAPI computes the amount actually charged at checkout.
    public static class CartTotals
    {
        public static decimal PurchaseAmount(IEnumerable<CartDetailViewModel>? details, decimal? discount)
        {
            // A missing discount or quantity counts as 0 instead of turning the whole total into null
            var total = details?.Sum(d => (d.Product?.Price ?? 0) * (d.Count ?? 0)) ?? 0;

            // A coupon worth more than the cart makes it free, never negative
            return Math.Max(0, total - (discount ?? 0));
        }
    }
}
