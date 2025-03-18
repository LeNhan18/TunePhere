using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TunePhere.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; } // Khóa chính

        [Required, StringLength(50)]
        public required string Username { get; set; } // Tên đăng nhập

        [Required, EmailAddress, StringLength(100)]
        public required string Email { get; set; } // Email

        [Required]
        public required string PasswordHash { get; set; } // Mã hóa mật khẩu

        [Required, StringLength(100)]
        public required string FullName { get; set; } // Họ và tên đầy đủ

        public DateTime CreatedAt { get; set; } = DateTime.Now; // Ngày tạo tài khoản

        public DateTime? LastLogin { get; set; } // Lần đăng nhập gần nhất

        // Các thuộc tính điều hướng (Navigation properties)
        public virtual ICollection<ListeningRoom> ListeningRooms { get; set; } // Danh sách phòng do người dùng tạo
        public virtual ICollection<ListeningRoomParticipant> ListeningRoomParticipants { get; set; } // Danh sách phòng đã tham gia
        public virtual ICollection<Playlist> Playlists { get; set; } // Danh sách playlist của người dùng
        public virtual ICollection<Remix> Remixes { get; set; } // Danh sách remix của người dùng
        public virtual UserPreference Preferences { get; set; } // Tùy chọn người dùng

        public User()
        {
            ListeningRooms = new HashSet<ListeningRoom>();
            ListeningRoomParticipants = new HashSet<ListeningRoomParticipant>();
            Playlists = new HashSet<Playlist>();
            Remixes = new HashSet<Remix>();
        }
    }
}