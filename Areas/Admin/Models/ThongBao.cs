using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace aznews.Areas.Admin.Models
{
    [Table("ThongBao")]
    public class ThongBao
    {
        [Key]
        public int MaTB { get; set; }
        public string? TieuDe { get; set; }
        public string? NoiDung { get; set; }
        public DateTime? NgayDang { get; set; }
        public string? LoaiThongBao { get; set; }
        public string? TepDinhKem { get; set; }
        public int MaND { get; set; }
        [ForeignKey("MaND")]
        public NguoiDung? NguoiDung { get; set; }
        public bool TrangThai { get; set; } = true;
    }
}