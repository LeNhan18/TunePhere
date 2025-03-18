using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TunePhere.Models
{
    public class Lyric
    {
        [Key]
        public int LyricId { get; set; } // Khóa chính

        [Required]
        public int SongId { get; set; } // Khóa ngoại liên kết với Song

        [ForeignKey("SongId")]
        public required Song Song { get; set; } // Quan hệ với bài hát

        [Required]
        public required string Content { get; set; } // Nội dung lời bài hát (định dạng JSON hoặc LRC)

        [Required, StringLength(50)]
        public required string Language { get; set; } // Ngôn ngữ của lyrics (VD: "en", "vi")

        public DateTime CreatedAt { get; set; } = DateTime.Now; // Ngày thêm lyrics

        public DateTime? UpdatedAt { get; set; }
    }
}
