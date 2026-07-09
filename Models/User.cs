using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ArabamTR.Models
{
    public class User : IdentityUser<int>
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public bool IsEmailConfirmed { get; set; }

        public bool Is2FAEnabled { get; set; }

        [MaxLength(6)]
        public string? TwoFactorCode { get; set; }

        public string? ResetPasswordToken { get; set; }

        [Required]
        [MaxLength(50)]
        public string AccountStatus { get; set; } = "Active";

        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = "User";

        public string SelectedLanguage { get; set; } = "TR";

        [Required]
        public DateTime LastActiveDate { get; set; } = DateTime.UtcNow;

        // Navigation properties 
        public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public virtual ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public virtual ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
    }
}
