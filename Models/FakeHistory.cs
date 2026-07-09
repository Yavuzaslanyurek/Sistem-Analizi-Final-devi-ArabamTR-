using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArabamTR.Models
{
    public class FakeHistory
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string PlateNumber { get; set; } = string.Empty;

        [Required]
        public bool HasDamageRecord { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DamageAmount { get; set; }

        [Required]
        public int LastKM { get; set; }

        [Required]
        public string KmHistoryJson { get; set; } = string.Empty; // Holds JSON array of mileage checkpoints
    }
}
