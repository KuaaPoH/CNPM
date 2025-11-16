using System.ComponentModel.DataAnnotations;

namespace aznews.Areas.Admin.Models
{
    public class LopHocPhanBatchVM
    {
        [Required] public int MaHP { get; set; }
        [Required] public int MaGiangVien { get; set; }
        [Required, StringLength(20)] public string HocKy { get; set; } = "";
        [Required, StringLength(20)] public string NamHoc { get; set; } = "";

        [Range(1, 50)] public int SoLT { get; set; } = 1;
        [Range(0, 50)] public int SoTHPerLT { get; set; } = 0;
        [Range(0, 50)] public int SoDAPerLT { get; set; } = 0;
    }
}
