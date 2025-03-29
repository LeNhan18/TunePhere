using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TunePhere.Models
{
    public class UserFavoriteSong
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; }
        
        [Required]
        public int SongId { get; set; }
        
        public DateTime AddedDate { get; set; } = DateTime.Now;
        
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; }
        
        [ForeignKey("SongId")]
        public virtual Song Song { get; set; }
    }
} 