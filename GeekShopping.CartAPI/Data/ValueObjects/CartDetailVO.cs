
using System.ComponentModel.DataAnnotations;

namespace GeekShopping.CartAPI.Data.ValueObjects
{
    public class CartDetailVO
    {
        public long Id { get; set; }
        public long? CartHeaderId { get; set; }

        public CartHeaderVO? CartHeader { get; set; }

        public long? ProductId { get; set; }

        public ProductVO? Product { get; set; }

        [Required(ErrorMessage = "Count is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Count must be at least 1.")]
        public int? Count { get; set; }
    }
}