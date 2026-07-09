using System;
using System.ComponentModel.DataAnnotations;

namespace ArabamTR.Models.Dtos
{
    public class MessageSendDto
    {
        [Required]
        public int ReceiverId { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Content { get; set; } = string.Empty;
    }

    public class MessageResponseDto
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public int ReceiverId { get; set; }
        public string ReceiverName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public bool IsRead { get; set; }
    }

    public class ConversationResponseDto
    {
        public int OtherUserId { get; set; }
        public string OtherUserName { get; set; } = string.Empty;
        public string LastMessageContent { get; set; } = string.Empty;
        public DateTime LastMessageTimestamp { get; set; }
        public int UnreadCount { get; set; }
        public DateTime OtherUserLastActiveDate { get; set; }
        public bool IsOnline { get; set; }
    }
}
