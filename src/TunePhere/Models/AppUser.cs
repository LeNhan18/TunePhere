using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TunePhere.Models
{
    public class AppUser : IdentityUser
    {

        public required string FullName { get; set; } // Họ và tên đầy đủ

        public DateTime CreatedAt { get; set; } = DateTime.Now; // Ngày tạo tài khoản

        public DateTime? LastLogin { get; set; } // Lần đăng nhập gần nhất

        // Các thuộc tính điều hướng (Navigation properties)
        public virtual ICollection<ListeningRoom> ListeningRooms { get; set; } // Danh sách phòng do người dùng tạo
        public virtual ICollection<ListeningRoomParticipant> ListeningRoomParticipants { get; set; } // Danh sách phòng đã tham gia
        public virtual ICollection<Playlist> Playlists { get; set; } // Danh sách playlist của người dùng
        public virtual ICollection<Remix> Remixes { get; set; } // Danh sách remix của người dùng
        public virtual UserPreference? Preferences { get; set; } // Tùy chọn người dùng

        public AppUser()
        {
            ListeningRooms = new HashSet<ListeningRoom>();
            ListeningRoomParticipants = new HashSet<ListeningRoomParticipant>();
            Playlists = new HashSet<Playlist>();
            Remixes = new HashSet<Remix>();
        }

    }
}
