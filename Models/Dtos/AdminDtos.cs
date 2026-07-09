using System.ComponentModel.DataAnnotations;

namespace ArabamTR.Models.Dtos
{
    public class BrandCreateDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
    }

    public class BrandUpdateDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
    }

    public class ModelCreateDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int BrandId { get; set; }
    }

    public class ModelUpdateDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int BrandId { get; set; }
    }

    public class CarFeatureCreateDto
    {
        [Required]
        [MaxLength(100)]
        public string FeatureName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string FeatureType { get; set; } = string.Empty;
    }

    public class CarFeatureUpdateDto
    {
        [Required]
        [MaxLength(100)]
        public string FeatureName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string FeatureType { get; set; } = string.Empty;
    }
}
