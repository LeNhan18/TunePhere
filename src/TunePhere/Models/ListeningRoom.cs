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

        public string CreatorId { get; set; }

        [ForeignKey("CreatorId")]
        public AppUser? Creator { get; set; } // Quan hệ với User

        [Required(ErrorMessage = "Vui lòng nhập tên phòng")]
        [StringLength(200, ErrorMessage = "Tên phòng không được vượt quá 200 ký tự")]
        [Display(Name = "Tên Phòng")]
        public string Title { get; set; }

        [Display(Name = "Kích Hoạt")]
        public bool IsActive { get; set; } = true; // Trạng thái phòng (đang hoạt động hay không)

        [Required(ErrorMessage = "Vui lòng chọn bài hát")]
        [Display(Name = "Bài Hát Hiện Tại")]
        public int? CurrentSongId { get; set; } // ID của bài hát đang phát (có thể null)

        [ForeignKey("CurrentSongId")]
        public Song? CurrentSong { get; set; } // Bài hát đang phát

        public DateTime CreatedAt { get; set; } = DateTime.Now; // Ngày tạo phòng

        // Thuộc tính điều hướng
        public virtual ICollection<ListeningRoomParticipant> Participants { get; set; }

        public ListeningRoom()
        {
            Title = string.Empty;
            CreatorId = string.Empty;
            Participants = new HashSet<ListeningRoomParticipant>();
        }
    }
}
