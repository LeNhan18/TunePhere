using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TunePhere.Models
{
    public class Playlist
    {
        [Key]
        public int PlaylistId { get; set; }

        [Required]
        public string UserId { get; set; }

        public virtual AppUser? User { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; }

        public bool IsPublic { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? ImageUrl { get; set; }

        public virtual ICollection<PlaylistSong> PlaylistSongs { get; set; }

        public Playlist()
        {
            PlaylistSongs = new HashSet<PlaylistSong>();
        }
        
        // Add this property if it doesn't exist
        public string Description { get; set; }
    }
}
