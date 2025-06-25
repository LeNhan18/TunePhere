using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TunePhere.Models
{
    public class PlaylistSong
    {
        [Key]
        public int PlaylistSongId { get; set; }

        [Required]
        public int PlaylistId { get; set; }

        [ForeignKey("PlaylistId")]
        public virtual Playlist Playlist { get; set; }

        [Required]
        public int SongId { get; set; }

        [ForeignKey("SongId")]
        public virtual Song Song { get; set; }

        public int Order { get; set; }

        [Required]
        public string? AddedByUserId { get; set; } // Người thêm bài hát

        [ForeignKey("AddedByUserId")]
        public AppUser? AddedByUser { get; set; } // Quan hệ với User

        public DateTime AddedAt { get; set; } = DateTime.Now; // Ngày thêm vào playlist

        public int VoteCount { get; set; } = 0; // Số lượt bình chọn bài hát trong playlist
    }
}
