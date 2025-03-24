using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TunePhere.Models
{
    public class Artists
    {
        [Key]
        public int ArtistId { get; set; }
        [Required,StringLength(100)]
        public required string ArtistName { get; set; }

        public string? ImageUrl { get; set; }

        public string? Bio { get; set; }

        [ForeignKey("userId")]
        public required string userId { get; set; }
        public AppUser? AppUser { get; set; }

        //public virtual ICollection<Album> Albums { get; set; }
        public virtual ICollection<Song> Songs { get; set; }

        public virtual ICollection<Remix> Remixes { get; set; }
         
        public virtual ICollection<Playlist> Playlists { get; set; }

        public virtual ICollection<ArtistSong> ArtistSongs { get; set; }


        public Artists()
        {
            Songs = new HashSet<Song>();
            Remixes = new HashSet<Remix>();
            Playlists = new HashSet<Playlist>();
        }
    }
}
