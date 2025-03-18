using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TunePhere.Models
{
    public class ListeningRoom
    {
        [Key]
        public int RoomId { get; set; } // Khóa chính

        [Required]
        public int CreatorId { get; set; } // Khóa ngoại liên kết với User (người tạo phòng)

        [ForeignKey("CreatorId")]
        public required User? Creator { get; set; } // Quan hệ với User

        [Required, StringLength(200)]
        public required string Title { get; set; } // Tên phòng nghe nhạc

        public bool IsActive { get; set; } = true; // Trạng thái phòng (đang hoạt động hay không)

        public int? CurrentSongId { get; set; } // ID của bài hát đang phát (có thể null)

        [ForeignKey("CurrentSongId")]
        public Song? CurrentSong { get; set; } // Bài hát đang phát

        public DateTime CreatedAt { get; set; } = DateTime.Now; // Ngày tạo phòng

        // Thuộc tính điều hướng
        public virtual ICollection<ListeningRoomParticipant> Participants { get; set; }

        public ListeningRoom()
        {
            Participants = new HashSet<ListeningRoomParticipant>();
        }
    }
}
