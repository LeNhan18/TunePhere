using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TunePhere.Models
{
    public class Song
    {
        [Key]
        public int SongId { get; set; } // Khóa chính

        [Required, StringLength(200)]
        public required string Title { get; set; } // Tên bài hát

        [Required, StringLength(100)]
        public required string Artist { get; set; } // Nghệ sĩ

        [Required, StringLength(50)]
        public required string Genre { get; set; } // Thể loại nhạc

        [Required]
        public TimeSpan Duration { get; set; } // Thời lượng bài hát

        [Required]
        public required string FileUrl { get; set; } // Đường dẫn file nhạc

        [Required]
        public required string ImageUrl { get; set; } // Đường dẫn ảnh bìa

        public DateTime UploadDate { get; set; } = DateTime.Now; // Ngày tải lên

        // Navigation properties
        public virtual ICollection<Lyric> Lyrics { get; set; } // Lời bài hát
        public virtual ICollection<Remix> Remixes { get; set; } // Danh sách remix
        public virtual ICollection<PlaylistSong> PlaylistSongs { get; set; } // Danh sách playlist chứa bài hát này

        public Song()
        {
            Lyrics = new HashSet<Lyric>();
            Remixes = new HashSet<Remix>();
            PlaylistSongs = new HashSet<PlaylistSong>();
        }
    }
}
