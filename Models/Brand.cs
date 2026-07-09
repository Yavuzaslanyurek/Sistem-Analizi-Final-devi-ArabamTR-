using System.ComponentModel.DataAnnotations;

namespace ArabamTR.Models
{
    public class Brand
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<Model> Models { get; set; } = new List<Model>();
    }
}
