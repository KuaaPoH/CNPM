using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace aznews.Areas.Admin.Models
{
    [Table("LopHanhChinh")]
    public class LopHanhChinh
    {
        [Key]
        public int MaLopHC { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên lớp hành chính")]
        [StringLength(50)]
        public string? TenLopHC { get; set; }

        [StringLength(10)]
        public string? KhoaHoc { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngành")]
        [ForeignKey("Nganh")]
        public int MaNganh { get; set; }

        // Liên kết khóa ngoại đến bảng Nganh
        public virtual Nganh? Nganh { get; set; }
    }
}