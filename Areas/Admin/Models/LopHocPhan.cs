using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace aznews.Areas.Admin.Models
{
    [Table("LopHocPhan")]
    public class LopHocPhan
    {
        [Key] public int MaLHP { get; set; }

        public int MaHP { get; set; }
        [ForeignKey(nameof(MaHP))] public HocPhan? HocPhan { get; set; }

        public int MaGiangVien { get; set; }
        [ForeignKey(nameof(MaGiangVien))] public GiangVien? GiangVien { get; set; }

        [Required, StringLength(20)] public string HocKy { get; set; } = "HK1";
        [Required, StringLength(20)] public string NamHoc { get; set; } = "2024-2025";
    }
}
