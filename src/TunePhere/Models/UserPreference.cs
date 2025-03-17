using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TunePhere.Models
{
    public class UserPreference
    {
        [Key, ForeignKey("User")]
        public int UserId { get; set; } // Khóa chính đồng thời là khóa ngoại liên kết với User

        public User User { get; set; } // Quan hệ với User

        [Required]
        public string FavoriteGenres { get; set; } // Danh sách thể loại yêu thích (lưu dạng JSON hoặc chuỗi)

        [Required]
        public string ListeningHistory { get; set; } // Lịch sử nghe nhạc (lưu ID bài hát dạng JSON hoặc chuỗi)

        public DateTime LastUpdated { get; set; } = DateTime.Now; // Thời gian cập nhật lần cuối
    }
}
