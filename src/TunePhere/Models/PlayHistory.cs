using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TunePhere.Models
{
    public class PlayHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SongId { get; set; }

        public string? UserId { get; set; }

        [Required]
        public DateTime PlayedAt { get; set; } = DateTime.Now;

        [ForeignKey("SongId")]
        public virtual Song? Song { get; set; }

        [ForeignKey("UserId")]
        public virtual AppUser? User { get; set; }
    }
} 