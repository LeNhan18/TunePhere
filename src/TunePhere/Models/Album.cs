using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TunePhere.Models
{
    public class Album
    {
        [Key]
        public int AlbumId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên album")]
        [StringLength(200)]
        public string? AlbumName { get; set; }

        public string? AlbumDescription { get; set; }

        public string? ImageUrl { get; set; }

        public string? ReleaseDate { get; set; }

        public int numberSongs { get; set; }

        public TimeSpan Time { get; set; }

        [Required]
        [ForeignKey("Artists")]
        public int ArtistId { get; set; }
        public virtual Artists? Artists { get; set; }

        public virtual ICollection<Song> Songs { get; set; }
        public Album() {
            Songs = new HashSet<Song>();
        }
    }
}
