using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TunePhere.Models
{
    public class ListeningRoom
    {
        [Key]
        public int RoomId { get; set; } // Khóa chính

        [Required]
        public int HostUserId { get; set; } // Khóa ngoại liên kết với User (người tạo phòng)

        [ForeignKey("HostUserId")]
        public User HostUser { get; set; } // Quan hệ với User

        [Required, StringLength(200)]
        public string Title { get; set; } // Tên phòng nghe nhạc

        public bool IsActive { get; set; } = true; // Trạng thái phòng (đang hoạt động hay không)

        public int? CurrentSongId { get; set; } // Khóa ngoại liên kết với bài hát đang phát (có thể null)

        [ForeignKey("CurrentSongId")]
        public Song CurrentSong { get; set; } // Quan hệ với bài hát hiện tại

        public DateTime CreatedAt { get; set; } = DateTime.Now; // Ngày tạo phòng
    }
}
