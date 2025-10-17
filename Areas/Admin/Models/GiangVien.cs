using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace aznews.Areas.Admin.Models
{
    [Table("GiangVien")]
    public class GiangVien
    {
        [Key]
        public int MaGiangVien { get; set; }
        public int MaND { get; set; }
        public string MaSoGV { get; set; }
        public string HoTen { get; set; }
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public string? ChuyenMon { get; set; }
        public string? HocVi { get; set; }
        public string AnhDaiDien { get; set; }
    }
}