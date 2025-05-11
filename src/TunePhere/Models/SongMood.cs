using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TunePhere.Models
{
    public class SongMood
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SongId { get; set; }

        [ForeignKey("SongId")]
        public Song Song { get; set; }

        // Các chỉ số từ Spotify API
        public float Valence { get; set; }        // Mức độ tích cực (0-1)
        public float Energy { get; set; }         // Năng lượng (0-1)
        public float Danceability { get; set; }   // Khả năng nhảy múa (0-1)
        public float Tempo { get; set; }          // Tempo (BPM)
        public string Mode { get; set; }          // Mode (major/minor)
        public string Key { get; set; }           // Giọng

        // Kết quả phân tích cảm xúc
        public string Mood { get; set; }          // Vui, buồn, trung tính
        public string Description { get; set; }   // Mô tả chi tiết về cảm xúc
    }
} 