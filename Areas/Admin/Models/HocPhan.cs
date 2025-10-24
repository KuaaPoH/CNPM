using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace aznews.Areas.Admin.Models
{
    [Table("HocPhan")]
    public class HocPhan
    {
        [Key] public int MaHP { get; set; }
        [Required, StringLength(100)] public string TenHP { get; set; } = string.Empty;
        public int SoTinChi { get; set; }
    }
}
