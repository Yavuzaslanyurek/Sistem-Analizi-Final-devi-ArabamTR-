using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ArabamTR.Models.Dtos
{
    public class VehicleCreateDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0, 999999999999)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, 10000000)]
        public int KM { get; set; }

        [Required]
        [Range(1900, 2100)]
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
        public int ModelId { get; set; }

        public List<int> FeatureIds { get; set; } = new List<int>();
    }

    public class VehicleUpdateDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0, 999999999999)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, 10000000)]
        public int KM { get; set; }

        [Required]
        [Range(1900, 2100)]
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
        [MaxLength(50)]
        public string Status { get; set; } = "Active";

        [Required]
        public int ModelId { get; set; }

        public List<int> FeatureIds { get; set; } = new List<int>();
    }

    public class VehicleResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int KM { get; set; }
        public int Year { get; set; }
        public string PlateNumber { get; set; } = string.Empty;
        public string ChassisNumber { get; set; } = string.Empty;
        public string EngineNumber { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int ModelId { get; set; }
        public string ModelName { get; set; } = string.Empty;
        public int BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public List<CarFeatureDto> Features { get; set; } = new List<CarFeatureDto>();
    }

    public class CarFeatureDto
    {
        public int Id { get; set; }
        public string FeatureName { get; set; } = string.Empty;
        public string FeatureType { get; set; } = string.Empty;
    }
}
