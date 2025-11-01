using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace aznews.Areas.Admin.Models
{
    [Table("ThongBao")]
    public class ThongBao
    {
        [Key]
        public int MaTB { get; set; }

        [Required, StringLength(200)]
        public string TieuDe { get; set; }

        public string? NoiDung { get; set; }

        public DateTime? NgayDang { get; set; }

        [StringLength(100)]
        public string? LoaiThongBao { get; set; }

        public string? TepDinhKem { get; set; }

        // bạn đã đổi sang MaVaiTro trong DB
        public int MaVaiTro { get; set; }

        // để ẩn/hiện TB
        public bool TrangThai { get; set; } = true;

        [ForeignKey(nameof(MaVaiTro))]
        public VaiTro? VaiTro { get; set; }
    }
}
