using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArabamTR.Models
{
    public class Vehicle
    {
        public int Id { get; set; }

        public string ImageUrl { get; set; } = "default-car.png";

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required]
        public int KM { get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        [MaxLength(20)]
        public string PlateNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string ChassisNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string EngineNumber { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Active"; // Active, Sold, Passive

        [Required]
        public int UserId { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;

        [Required]
        public int ModelId { get; set; }
        public virtual Model Model { get; set; } = null!;

        public virtual ICollection<VehicleFeature> VehicleFeatures { get; set; } = new List<VehicleFeature>();
    }
}
