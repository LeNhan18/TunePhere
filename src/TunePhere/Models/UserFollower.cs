using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TunePhere.Models
{
    public class UserFollower
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FollowerId { get; set; }  // ID của người theo dõi

        [Required]
        public string FollowingId { get; set; }  // ID của người được theo dõi

        public DateTime FollowedAt { get; set; } = DateTime.Now;

        [ForeignKey("FollowerId")]
        public virtual AppUser Follower { get; set; }  // Người theo dõi

        [ForeignKey("FollowingId")]
        public virtual AppUser Following { get; set; }  // Người được theo dõi
    }
}
