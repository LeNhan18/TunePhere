using System.ComponentModel.DataAnnotations;

namespace TunePhere.Models.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public List<string> Roles { get; set; }
    }

    public class EditUserViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [Display(Name = "Họ tên")]
        public string FullName { get; set; }

        public List<RoleViewModel> Roles { get; set; }
    }

    public class RoleViewModel
    {
        public string Name { get; set; }
        public bool IsSelected { get; set; }
    }
} 