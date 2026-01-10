using System.ComponentModel.DataAnnotations;

namespace aznews.Models
{
    public class SinhVienProfileVM
    {
        // Thông tin Read-only
        public string MaSoSV { get; set; } = "";
        public string LopHanhChinh { get; set; } = "";
        public string CurrentAvatar { get; set; } = "";

        // Thông tin Update được
        [Required(ErrorMessage = "Họ tên không được để trống")]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; } = "";

        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime? NgaySinh { get; set; }

        [Display(Name = "Giới tính")]
        public string? GioiTinh { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "SĐT không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        [Display(Name = "Ảnh đại diện")]
        public IFormFile? AvatarFile { get; set; }

        // Đổi mật khẩu
        [Display(Name = "Mật khẩu hiện tại")]
        public string? CurrentPass { get; set; }

        [Display(Name = "Mật khẩu mới")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải từ 6 ký tự")]
        public string? NewPass { get; set; }

        [Display(Name = "Nhập lại mật khẩu")]
        [Compare("NewPass", ErrorMessage = "Mật khẩu không khớp")]
        public string? ConfirmPass { get; set; }

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
