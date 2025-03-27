using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TunePhere.Models
{
    public class Artists
    {
        [Key]
        public int ArtistId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên nghệ danh")]
        [StringLength(100, ErrorMessage = "Tên nghệ danh không được vượt quá 100 ký tự")]
        public required string ArtistName { get; set; }

        public string? ImageUrl { get; set; }

        public string? CoverImageUrl { get; set; }

        public string? Bio { get; set; }

        [Required]
        [ForeignKey("AppUser")]
        public required string userId { get; set; }
        public AppUser? AppUser { get; set; }

        public virtual ICollection<Album> Albums { get; set; }
        public virtual ICollection<Song> Songs { get; set; }
        public virtual ICollection<Remix> Remixes { get; set; }
        public virtual ICollection<Playlist> Playlists { get; set; }
        public virtual ICollection<ArtistFollower> Followers { get; set; }

        public Artists()
        {
            Albums = new HashSet<Album>();
            Songs = new HashSet<Song>();
            Remixes = new HashSet<Remix>();
            Playlists = new HashSet<Playlist>();
            Followers = new HashSet<ArtistFollower>();
        }

        public int GetFollowersCount()
        {
            return Followers?.Count ?? 0;
        }
    }
}
