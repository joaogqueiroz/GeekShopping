using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GeekShopping.CouponAPI.Model.Base;

namespace GeekShopping.CouponAPI.Model
{
    [Table("coupon")]
    public class Coupon : BaseEntity
    {
        [Required]
        [Column("coupon_conde")]
        [StringLength(30)]
        public string? CouponCode { get; set; }

        [Required]
        [Column("discount_amount")]
        public decimal? DiscountAmount { get; set; }


    }
}