using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace aznews.Areas.Admin.Models
{
    [Table("Nganh")]
    public class Nganh
    {
        [Key]
        public int MaNganh { get; set; }

        public string? TenNganh { get; set; }

        public int MaKhoaVien { get; set; }
        
        [ForeignKey("MaKhoaVien")]
        public KhoaVien? KhoaVien { get; set; }
    }
}
