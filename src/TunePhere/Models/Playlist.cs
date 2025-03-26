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

        [Required, ForeignKey("Id")]
        public string UserId { get; set; } // Khóa ngoại liên kết với User

 
        public virtual AppUser User { get; set; } // Quan hệ với bảng Users

        [Required, StringLength(200)]
        public string Title { get; set; } // Tên playlist

        public bool IsPublic { get; set; } = true; // Playlist công khai hoặc riêng tư

        public DateTime CreatedAt { get; set; } = DateTime.Now; // Ngày tạo

        // Navigation property
        public virtual ICollection<PlaylistSong> PlaylistSongs { get; set; } // Danh sách bài hát trong playlist

        public Playlist()
        {
            PlaylistSongs = new HashSet<PlaylistSong>();
        }
    }
}
