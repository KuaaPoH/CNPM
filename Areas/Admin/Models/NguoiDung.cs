using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace aznews.Areas.Admin.Models
{
    [Table("NguoiDung")]
    public class NguoiDung
    {
        [Key]
        public int MaND { get; set; }
        public string TenDangNhap { get; set; }
        public string MatKhau { get; set; }
        public int MaVaiTro { get; set; }
         public bool? TrangThai { get; set; }
        
    }
}