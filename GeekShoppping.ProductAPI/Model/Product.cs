using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GeekShoppping.ProductAPI.Model.Base;

namespace GeekShoppping.ProductAPI.Model
{
    [Table("product")]
    public class Product : BaseEntity
    {
        [Required]
        [Column("name")]
        [StringLength(150)]
        public string Name { get; set; }

        [Required]
        [Column("price")]
        [Range(1,999999999999)]
        public decimal Price { get; set; }

        [Column("description")]
        [StringLength(500)]
        public string? Description { get; set; }

        [Column("category_name")]
        [StringLength(100)]
        public string? CategoryName { get; set; }

        [Column("image_url")]
        [StringLength(300)]
        public string? ImageURL { get; set; }
    }
}