using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TunePhere.Models
{
    public class UserPreference
    {
        [Key, ForeignKey("User")]
        public required string   UserId { get; set; } // Khóa chính đồng thời là khóa ngoại liên kết với User

        [Required]
        public required AppUser User { get; set; } // Quan hệ với User

        [Required]
        public required string FavoriteGenres { get; set; } // Danh sách thể loại yêu thích (lưu dạng JSON hoặc chuỗi)

        [Required]
        public required string ListeningHistory { get; set; } // Lịch sử nghe nhạc (lưu ID bài hát dạng JSON hoặc chuỗi)

        public DateTime LastUpdated { get; set; } = DateTime.Now; // Thời gian cập nhật lần cuối
    }
}
