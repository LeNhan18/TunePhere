using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TunePhere.Models
{
    public class Remix
    {
        [Key]
        public int RemixId { get; set; } // Khóa chính

        [Required]
        public string UserId { get; set; } // Khóa ngoại liên kết với User (người tạo remix)

        [ForeignKey("UserId")]
        public required AppUser User { get; set; } // Quan hệ với User

        [Required]
        public int OriginalSongId { get; set; } // Khóa ngoại liên kết với bài hát gốc

        [ForeignKey("OriginalSongId")]
        public required Song OriginalSong { get; set; } // Quan hệ với Song gốc

        [Required, StringLength(200)]
        public required string Title { get; set; } // Tên bản remix

        [Required]
        public required string FileUrl { get; set; } // Đường dẫn file remix

        public int Likes { get; set; } = 0; // Số lượt thích

        public DateTime CreatedAt { get; set; } = DateTime.Now; // Ngày tạo remix
    }
}
