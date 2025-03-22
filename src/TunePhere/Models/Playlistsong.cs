using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TunePhere.Models
{
    public class PlaylistSong
    {
        [Key]
        public int Id { get; set; } // Khóa chính (để tránh lỗi khi tạo bảng)

        [Required]
        public int PlaylistId { get; set; } // Khóa ngoại liên kết với Playlist

        [ForeignKey("PlaylistId")]
        public required Playlist Playlist { get; set; } // Quan hệ với Playlist

        [Required]
        public int SongId { get; set; } // Khóa ngoại liên kết với Song

        [ForeignKey("SongId")]
        public required Song Song { get; set; } // Quan hệ với Song

        [Required]
        public string AddedByUserId { get; set; } // Người thêm bài hát

        [ForeignKey("AddedByUserId")]
        public required AppUser AddedByUser { get; set; } // Quan hệ với User

        public DateTime AddedAt { get; set; } = DateTime.Now; // Ngày thêm vào playlist

        public int VoteCount { get; set; } = 0; // Số lượt bình chọn bài hát trong playlist
    }
}
