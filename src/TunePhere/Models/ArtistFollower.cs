using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TunePhere.Models
{
    public class ArtistFollower
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? UserId { get; set; }  // ID của người theo dõi

        [Required]
        public int? ArtistId { get; set; }  // ID của nghệ sĩ được theo dõi

        public DateTime FollowedAt { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; }  // Người theo dõi

        [ForeignKey("ArtistId")]
        public virtual Artists Artist { get; set; }  // Nghệ sĩ được theo dõi
    }
} 