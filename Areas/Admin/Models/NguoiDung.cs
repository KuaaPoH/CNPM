using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace aznews.Areas.Admin.Models
{
    [Table("NguoiDung")]
    public class NguoiDung
    {
        [Key] public int MaND { get; set; }

        [Required] public string TenDangNhap { get; set; } = string.Empty;
        [Required] public string MatKhau { get; set; } = string.Empty;

        public int MaVaiTro { get; set; }          // FK thực
        public bool? TrangThai { get; set; }

        // KHÔNG gắn [ForeignKey] ở đây để tránh EF sinh shadow FK
        public VaiTro? VaiTro { get; set; }         // navigation
    }
}
