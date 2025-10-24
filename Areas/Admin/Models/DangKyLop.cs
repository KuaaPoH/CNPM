using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace aznews.Areas.Admin.Models
{
    [Table("DangKyLop")]
    public class DangKyLop
    {
        [Key] public int Id { get; set; }

        public int MaLHP { get; set; }
        [ForeignKey(nameof(MaLHP))] public LopHocPhan? LopHocPhan { get; set; }

        public int MaSinhVien { get; set; }
        [ForeignKey(nameof(MaSinhVien))] public SinhVien? SinhVien { get; set; }

        public DateTime NgayDK { get; set; } = DateTime.UtcNow;
        public byte TrangThai { get; set; } = 1;
        public string? GhiChu { get; set; }
    }
}
