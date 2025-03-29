using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using System.Text.Json.Serialization;

namespace TunePhere.Models
{
    public class Song
    {
        [Key]
        public int SongId { get; set; } // Khóa chính

        [Required, StringLength(200)]
        public required string Title { get; set; } // Tên bài hát

        [Required, StringLength(50)]
        public required string Genre { get; set; } // Thể loại nhạc

        [Required]
        public TimeSpan Duration { get; set; } // Thời lượng bài hát

        [Required]
        public required string FileUrl { get; set; } // Đường dẫn file nhạc

        [Required]
        public required string ImageUrl { get; set; } // Đường dẫn ảnh bìa

        public DateTime UploadDate { get; set; } = DateTime.Now; // Ngày tải lên

        [ForeignKey("AlbumId")]
        public int? AlbumId { get; set; } // Khóa ngoại liên kết với Album

        [ForeignKey("ArtistId")]
        public int ArtistId { get; set; } // Khóa ngoại liên kết với Artist
        
        public int PlayCount { get; set; } = 0; // Số lượt nghe
        public int LikeCount { get; set; } = 0;
        [NotMapped]
        public bool IsLiked { get; set; }

        public string? VideoUrl { get; set; }

        [JsonIgnore]
        public virtual Artists? Artists { get; set; }
        
        [JsonIgnore]
        public virtual Album? Albums { get; set; }

        [JsonIgnore]
        public virtual ICollection<Lyric>? Lyrics { get; set; }

        [JsonIgnore]
        public virtual ICollection<Remix>? Remixes { get; set; } // Danh sách remix
        
        [JsonIgnore]
        public virtual ICollection<PlaylistSong>? PlaylistSongs { get; set; } // Danh sách playlist chứa bài hát này
        
        [JsonIgnore]
        public virtual ICollection<UserFavoriteSong> FavoritedBy { get; set; }
        
        public Song()
        {
            Lyrics = new HashSet<Lyric>();
            Remixes = new HashSet<Remix>();
            PlaylistSongs = new HashSet<PlaylistSong>();
            FavoritedBy = new HashSet<UserFavoriteSong>();
            Title = "";
            Genre = "";
            FileUrl = "";
            ImageUrl = "";
            UploadDate = DateTime.Now;
            PlayCount = 0;
            LikeCount = 0;
        }
       
    }
}
