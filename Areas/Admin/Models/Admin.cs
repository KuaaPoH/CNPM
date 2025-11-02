using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace aznews.Areas.Admin.Models
{
    [Table("Admin")]
    public class Admin
    {
        [Key]
        public int MaAdmin { get; set; }

        [Required, StringLength(50)]
        public string TenDangNhap { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string MatKhau { get; set; } = string.Empty;

        public int MaVaiTro { get; set; }

        public bool TrangThai { get; set; } = true;

        [ForeignKey(nameof(MaVaiTro))]
        public VaiTro? VaiTro { get; set; }
    }
}
