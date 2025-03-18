using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TunePhere.Models
{
    public class ListeningRoomParticipant
    {
        [Key]
        public int Id { get; set; } // Khóa chính (tránh lỗi khi tạo bảng)

        [Required]
        public int RoomId { get; set; } // Khóa ngoại liên kết với ListeningRoom

        [ForeignKey("RoomId")]
        public required ListeningRoom Room { get; set; } // Quan hệ với ListeningRoom

        [Required]
        public int UserId { get; set; } // Khóa ngoại liên kết với User

        [ForeignKey("UserId")]
        public required User User { get; set; } // Quan hệ với User

        public DateTime JoinedAt { get; set; } = DateTime.Now; // Thời gian tham gia phòng
    }
}
