using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace aznews.Areas.Admin.Models
{
    [Table("SinhVien")]
    public class SinhVien
    {
        [Key]
        public int MaSinhVien { get; set; }

        // nối sang VaiTro (3 = SV)
        public int MaVaiTro { get; set; } = 3;

        [Required, StringLength(20)]
        public string MaSoSV { get; set; }

        [Required, StringLength(100)]
        public string HoTen { get; set; }

        public DateTime? NgaySinh { get; set; }

        [StringLength(10)]
        public string? GioiTinh { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(15)]
        public string? SoDienThoai { get; set; }

        [StringLength(200)]
        public string? DiaChi { get; set; }

        [StringLength(255)]
        public string AnhDaiDien { get; set; } = "default-sv.jpg";

        // mật khẩu riêng cho SV
        [Required, StringLength(100)]
        public string MatKhau { get; set; } = "123456";

        // 1 = đang học, 0 = nghỉ
        public bool TrangThai { get; set; } = true;

        [ForeignKey(nameof(MaVaiTro))]
        public VaiTro? VaiTro { get; set; }
    }
}
