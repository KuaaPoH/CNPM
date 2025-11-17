using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace aznews.Areas.Admin.Models
{
    [Table("HocPhan")]
    public class HocPhan
    {
        [Key]
        public int MaHP { get; set; }

        [Required, StringLength(10)]
        [Column("MaSoHP")]                  // <-- map đúng tên cột hiện có
        [Display(Name = "Mã học phần")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string? MaSoHP { get; private set; }

        [Required, StringLength(100)]
        [Display(Name = "Tên học phần")]
        public string TenHP { get; set; } = "";

        [Range(1, 10)]
        [Display(Name = "Số tín chỉ")]
        public int SoTinChi { get; set; }

        [Required, StringLength(20)]
        [Display(Name = "Phân tiết (LT/TH/DA)")]
        public string PhanTiet { get; set; } = "0/0/0";
        public bool TrangThai { get; set; } = true;
    }
}
