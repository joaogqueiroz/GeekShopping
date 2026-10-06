using System.ComponentModel.DataAnnotations;

namespace GeekShopping.ProductAPI.Data.ValeuObjects
{
    public class ProductVO
    {
        public long Id { get; set; }

        // Limits match the product table columns, so bad input is a 400 instead of a SQL error
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(150, ErrorMessage = "Name should have at most 150 characters.")]
        public string Name { get; set; }
        
        [Range(typeof(decimal), "1", "999999999999", ErrorMessage = "Price must be between 1 and 999999999999.")]
        public decimal Price { get; set; }

        [StringLength(500, ErrorMessage = "Description should have at most 500 characters.")]
        public string? Description { get; set; }

        [StringLength(100, ErrorMessage = "Category should have at most 100 characters.")]
        public string? CategoryName { get; set; }

        [StringLength(300, ErrorMessage = "Image URL should have at most 300 characters.")]
        public string? ImageURL { get; set; }

    }
}