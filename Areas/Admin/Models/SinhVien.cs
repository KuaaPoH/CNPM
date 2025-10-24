using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace aznews.Areas.Admin.Models
{
    [Table("SinhVien")]
    public class SinhVien
    {
        [Key] public int MaSinhVien { get; set; }
        public int MaNguoiDung { get; set; }

        [Required, StringLength(20)] public string MaSoSV { get; set; } = string.Empty;
        [Required, StringLength(100)] public string HoTen { get; set; } = string.Empty;
        [StringLength(100)] public string? Email { get; set; }
    }
}
