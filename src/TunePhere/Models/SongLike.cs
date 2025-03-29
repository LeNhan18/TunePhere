using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TunePhere.Models
{
    public class SongLike
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }  // ID của người like

        [Required]
        public int SongId { get; set; }  // ID của bài hát được like

        public DateTime LikedAt { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; }  // Người like

        [ForeignKey("SongId")]
        public virtual Song Song { get; set; }  // Bài hát được like
    }
} 