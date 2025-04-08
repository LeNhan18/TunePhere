using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TunePhere.Models
{
    public class ChatMessage
    {
        [Key]
        public int? MessageId { get; set; }

        [Required]
        public string? Content { get; set; }

        public DateTime SentAt { get; set; }

        [Required]
        public string? SenderId { get; set; }
        [ForeignKey("SenderId")]
        public AppUser? Sender { get; set; }

        [Required]
        public int? RoomId { get; set; }
        [ForeignKey("RoomId")]
        public ListeningRoom? Room { get; set; }

        public bool IsSystemMessage { get; set; }
    }
} 