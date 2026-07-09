using System;
using System.ComponentModel.DataAnnotations;

namespace ArabamTR.Models
{
    public class Message
    {
        public int Id { get; set; }

        [Required]
        public int SenderId { get; set; }
        public virtual User Sender { get; set; } = null!;

        [Required]
        public int ReceiverId { get; set; }
        public virtual User Receiver { get; set; } = null!;

        [Required]
        [MaxLength(2000)]
        public string Content { get; set; } = string.Empty;

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Required]
        public bool IsRead { get; set; } = false;
    }
}
