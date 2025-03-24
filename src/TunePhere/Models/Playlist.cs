using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TunePhere.Models
{
    public class Playlist
    {
        [Key]
        public int PlaylistId { get; set; } // Khóa chính

        [Required]
        public int UserId { get; set; } // Khóa ngoại liên kết với User

        [ForeignKey("UserId")]
        public required User User { get; set; } // Quan hệ với bảng Users

        [Required, StringLength(200)]
        public required string Title { get; set; } // Tên playlist

        [Required]
        public required string ImageUrl { get; set; } = "/images/default-playlist.jpg"; // Đường dẫn ảnh bìa

        public bool IsPublic { get; set; } = true; // Playlist công khai hoặc riêng tư

        public DateTime CreatedAt { get; set; } = DateTime.Now; // Ngày tạo

        public int PlayCount { get; set; } = 0; // Số lượt phát playlist

        // Navigation property
        public virtual ICollection<PlaylistSong> PlaylistSongs { get; set; } // Danh sách bài hát trong playlist

        public Playlist()
        {
            PlaylistSongs = new HashSet<PlaylistSong>();
        }
        
        // Thuộc tính để trả về số lượng bài hát trong playlist
        [NotMapped]
        public int SongCount => PlaylistSongs?.Count ?? 0;
    }
}
