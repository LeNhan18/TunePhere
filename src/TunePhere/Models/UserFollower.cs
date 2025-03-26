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
        public string FollowedId { get; set; }  // ID của người được theo dõi

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("FollowerId")]
        public virtual AppUser Follower { get; set; }  // Người theo dõi

        [ForeignKey("FollowedId")]
        public virtual AppUser Followed { get; set; }  // Người được theo dõi
    }
}
