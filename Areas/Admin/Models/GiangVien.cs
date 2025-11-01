using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace aznews.Areas.Admin.Models
{
    [Table("GiangVien")]
    public class GiangVien
    {
        [Key]
        public int MaGiangVien { get; set; }

        // giờ nối thẳng sang VaiTro
        public int MaVaiTro { get; set; } = 2;   // 2 = Giảng viên

        [Required, StringLength(20)]
        public string MaSoGV { get; set; }

        [Required, StringLength(100)]
        public string HoTen { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(15)]
        public string? SoDienThoai { get; set; }

        [StringLength(100)]
        public string? ChuyenMon { get; set; }

        [StringLength(50)]
        public string? HocVi { get; set; }

        [StringLength(255)]
        public string AnhDaiDien { get; set; } = "default-gv.jpg";

        // mật khẩu riêng cho GV
        [Required, StringLength(100)]
        public string MatKhau { get; set; } = "123456";

        // bật/tắt tài khoản GV
        public bool TrangThai { get; set; } = true;

        [ForeignKey(nameof(MaVaiTro))]
        public VaiTro? VaiTro { get; set; }
    }
}
