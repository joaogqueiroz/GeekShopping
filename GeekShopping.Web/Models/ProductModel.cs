using System.ComponentModel.DataAnnotations;

namespace GeekShopping.Web.Models
{
    public class ProductModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public string? Description { get; set; }

        public string? CategoryName { get; set; }

        public string? ImageURL { get; set; }

        [Range(1, 100)]
        public int Count { get; set; } = 1;

        public string SubStringName()
        {
            if (Name.Length < 24)
            {
                return Name;
            }
            else{
                return $"{Name.Substring(0, 21)} ...";
            }
        }
        public string SubStringDescription()
        {
            if (Description.Length < 355)
            {
                return Description;
            }
            else{
                return $"{Description.Substring(0, 352)} ...";
            }
        }

    }
}