using System.ComponentModel.DataAnnotations;

namespace aznews.Areas.Admin.Models.Profile
{
    public class ProfileVM
    {
        // Thông tin hiển thị (Read-only hoặc lấy từ Session)
        public string? Username { get; set; }
        public string? RoleName { get; set; }
        public string? LastLogin { get; set; }
        public string? AvatarUrl { get; set; }

        // Form cập nhật thông tin
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = "";

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string? Phone { get; set; }

        [Display(Name = "Giới thiệu")]
        public string? Description { get; set; }

        public IFormFile? AvatarFile { get; set; }

        // Form đổi mật khẩu
        [Display(Name = "Mật khẩu hiện tại")]
        public string? CurrentPass { get; set; }

        [Display(Name = "Mật khẩu mới")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải từ 6 ký tự trở lên")]
        public string? NewPass { get; set; }

        [Display(Name = "Nhập lại mật khẩu mới")]
        [Compare("NewPass", ErrorMessage = "Mật khẩu nhập lại không khớp")]
        public string? ConfirmPass { get; set; }

        // Thông báo
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
