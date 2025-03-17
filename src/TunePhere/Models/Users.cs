using System;
using System.ComponentModel.DataAnnotations;

namespace TunePhere.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; } // Khóa chính

        [Required, StringLength(50)]
        public string Username { get; set; } // Tên đăng nhập

        [Required, EmailAddress, StringLength(100)]
        public string Email { get; set; } // Email

        [Required]
        public string PasswordHash { get; set; } // Mã hóa mật khẩu

        [StringLength(100)]
        public string FullName { get; set; } // Họ và tên đầy đủ

        public DateTime CreatedAt { get; set; } = DateTime.Now; // Ngày tạo tài khoản

        public DateTime? LastLogin { get; set; } // Lần đăng nhập gần nhất
    }
}