using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ArabamTR.Data;
using ArabamTR.Models;
using ArabamTR.Models.Dtos;

namespace ArabamTR.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MessageController : ControllerBase
    {
        private readonly ArabamTRDbContext _context;
        private readonly ILogger<MessageController> _logger;

        // Thread-safe static cache to track the last sent message content and time for spam check
        private static readonly ConcurrentDictionary<int, (string Content, DateTime Timestamp)> _lastUserMessages = new();

        public MessageController(ArabamTRDbContext context, ILogger<MessageController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // POST: api/message/send
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] MessageSendDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var senderIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(senderIdString) || !int.TryParse(senderIdString, out int senderId))
                return Unauthorized("Geçersiz kullanıcı oturumu.");

            if (senderId == dto.ReceiverId)
                return BadRequest("Kendinize mesaj gönderemezsiniz.");

            // Verify receiver exists
            var receiverExists = await _context.Users.AnyAsync(u => u.Id == dto.ReceiverId);
            if (!receiverExists)
                return BadRequest("Mesaj gönderilmek istenen alıcı bulunamadı.");

            // SPAM FILTER: Prevent sending the exact same message within 3 seconds
            var now = DateTime.UtcNow;
            if (_lastUserMessages.TryGetValue(senderId, out var lastMsg))
            {
                if (lastMsg.Content == dto.Content && (now - lastMsg.Timestamp).TotalSeconds < 3)
                {
                    _logger.LogWarning("Spam check triggered for User {UserId}. Tried to send duplicate message too quickly.", senderId);
                    return BadRequest("Spam koruması aktif. Lütfen 3 saniye içinde aynı mesajı tekrar göndermeyin.");
                }
            }

            // Update spam cache
            _lastUserMessages[senderId] = (dto.Content, now);

            // Log message transaction (excluding sensitive info if needed, but logging sender, receiver, and length)
            _logger.LogInformation("User {SenderId} is sending a message to User {ReceiverId}. Content length: {Length} chars.", 
                senderId, dto.ReceiverId, dto.Content.Length);

            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = dto.ReceiverId,
                Content = dto.Content,
                Timestamp = now,
                IsRead = false
            };

            _context.Messages.Add(message);

            // Update sender active presence time
            var senderUser = await _context.Users.FindAsync(senderId);
            if (senderUser != null)
            {
                senderUser.LastActiveDate = now;
            }

            await _context.SaveChangesAsync();

            // Return clean response DTO
            var senderName = senderUser?.Name ?? "Bilinmeyen Gönderici";
            var receiverName = await _context.Users.Where(u => u.Id == dto.ReceiverId).Select(u => u.Name).FirstOrDefaultAsync() ?? "Bilinmeyen Alıcı";

            var responseDto = new MessageResponseDto
            {
                Id = message.Id,
                SenderId = message.SenderId,
                SenderName = senderName,
                ReceiverId = message.ReceiverId,
                ReceiverName = receiverName,
                Content = message.Content,
                Timestamp = message.Timestamp,
                IsRead = message.IsRead
            };

            return Ok(responseDto);
        }

        // GET: api/message/history/{otherUserId}
        [HttpGet("history/{otherUserId}")]
        public async Task<IActionResult> GetChatHistory(int otherUserId)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
                return Unauthorized();

            var otherUserExists = await _context.Users.AnyAsync(u => u.Id == otherUserId);
            if (!otherUserExists)
                return BadRequest("Geçersiz kullanıcı ID.");

            // Fetch history ordered by timestamp
            var messages = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => (m.SenderId == userId && m.ReceiverId == otherUserId) || 
                             (m.SenderId == otherUserId && m.ReceiverId == userId))
                .OrderBy(m => m.Timestamp)
                .ToListAsync();

            // Update user activity presence
            await UpdateUserActivityPresence(userId);

            var response = messages.Select(m => new MessageResponseDto
            {
                Id = m.Id,
                SenderId = m.SenderId,
                SenderName = m.Sender.Name,
                ReceiverId = m.ReceiverId,
                ReceiverName = m.Receiver.Name,
                Content = m.Content,
                Timestamp = m.Timestamp,
                IsRead = m.IsRead
            }).ToList();

            return Ok(response);
        }

        // GET: api/message/conversations
        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
                return Unauthorized();

            // Find all messages involving the current user
            var userMessages = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .ToListAsync();

            // Group by the "other user" ID
            var conversationGroups = userMessages
                .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .ToList();

            var conversations = new List<ConversationResponseDto>();
            var now = DateTime.UtcNow;

            foreach (var group in conversationGroups)
            {
                var otherUserId = group.Key;
                
                // Fetch other user's presence/details
                var otherUser = await _context.Users.FindAsync(otherUserId);
                if (otherUser == null) continue;

                // Last message in this conversation
                var lastMsg = group.OrderByDescending(m => m.Timestamp).First();

                // Count unread messages received by current user from the other user
                var unreadCount = group.Count(m => m.ReceiverId == userId && m.SenderId == otherUserId && !m.IsRead);

                // Online check: active within the last 5 minutes
                bool isOnline = (now - otherUser.LastActiveDate).TotalMinutes <= 5;

                conversations.Add(new ConversationResponseDto
                {
                    OtherUserId = otherUser.Id,
                    OtherUserName = otherUser.Name,
                    LastMessageContent = lastMsg.Content,
                    LastMessageTimestamp = lastMsg.Timestamp,
                    UnreadCount = unreadCount,
                    OtherUserLastActiveDate = otherUser.LastActiveDate,
                    IsOnline = isOnline
                });
            }

            // Sort by most recent message activity
            var sortedConversations = conversations
                .OrderByDescending(c => c.LastMessageTimestamp)
                .ToList();

            // Update user activity presence
            await UpdateUserActivityPresence(userId);

            return Ok(sortedConversations);
        }

        // PUT: api/message/read/{messageId}
        [HttpPut("read/{messageId}")]
        public async Task<IActionResult> MarkAsRead(int messageId)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
                return Unauthorized();

            var message = await _context.Messages.FindAsync(messageId);
            if (message == null)
                return NotFound("Mesaj bulunamadı.");

            // Only the recipient can mark the message as read
            if (message.ReceiverId != userId)
                return Forbid("Bu işlemi yapma yetkiniz bulunmamaktadır.");

            message.IsRead = true;
            
            // Update user activity presence
            await UpdateUserActivityPresence(userId);
            
            await _context.SaveChangesAsync();

            return Ok(new { message = "Mesaj okundu olarak işaretlendi." });
        }

        // PUT: api/message/read-chat/{otherUserId} (bulk read helper)
        [HttpPut("read-chat/{otherUserId}")]
        public async Task<IActionResult> MarkChatAsRead(int otherUserId)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
                return Unauthorized();

            var unreadMessages = await _context.Messages
                .Where(m => m.ReceiverId == userId && m.SenderId == otherUserId && !m.IsRead)
                .ToListAsync();

            if (unreadMessages.Any())
            {
                foreach (var msg in unreadMessages)
                {
                    msg.IsRead = true;
                }
                await _context.SaveChangesAsync();
            }

            // Update user activity presence
            await UpdateUserActivityPresence(userId);

            return Ok(new { message = $"{unreadMessages.Count} adet mesaj okundu olarak işaretlendi." });
        }

        // POST: api/message/heartbeat (Explicit online presence update)
        [HttpPost("heartbeat")]
        public async Task<IActionResult> Heartbeat()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
                return Unauthorized();

            await UpdateUserActivityPresence(userId);
            return Ok(new { message = "Aktiflik durumu güncellendi." });
        }

        private async Task UpdateUserActivityPresence(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.LastActiveDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}
