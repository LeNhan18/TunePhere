using System;
using System.ComponentModel.DataAnnotations;

namespace TunePhere.Models
{
    public class Song
    {
        [Key]
        public int SongId { get; set; } // Khóa chính

        [Required, StringLength(200)]
        public string Title { get; set; } // Tên bài hát

        [Required, StringLength(100)]
        public string Artist { get; set; } // Nghệ sĩ

        [Required, StringLength(50)]
        public string Genre { get; set; } // Thể loại nhạc

        [Required]
        public TimeSpan Duration { get; set; } // Thời lượng bài hát

        [Required]
        public string FileUrl { get; set; } // Đường dẫn file nhạc

        [Required]
        public string ImageUrl { get; set; } // Đường dẫn ảnh bìa

        public DateTime UploadDate { get; set; } = DateTime.Now; // Ngày tải lên
    }
}
