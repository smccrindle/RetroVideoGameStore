using System.ComponentModel.DataAnnotations;

namespace RetroVideoGameStore.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        // Reference the child model (1 category -> many products)
        public List<Product>? Products { get; set; }
    }
}
