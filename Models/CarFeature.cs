using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ArabamTR.Models
{
    public class CarFeature
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FeatureName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string FeatureType { get; set; } = string.Empty; // e.g., Safety, Comfort, Interior, Exterior

        // Navigation properties
        public virtual ICollection<VehicleFeature> VehicleFeatures { get; set; } = new List<VehicleFeature>();
    }
}
