using System.ComponentModel.DataAnnotations;

namespace ArabamTR.Models
{
    public class Model
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int BrandId { get; set; }

        // Navigation properties
        public virtual Brand Brand { get; set; } = null!;
        public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    }
}
