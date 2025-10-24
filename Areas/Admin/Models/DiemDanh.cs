using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace aznews.Areas.Admin.Models
{
    public enum AttendanceStatus : byte
    {
        CoMat = 0,
        CoPhep = 1,
        Muon = 2,
        Vang = 3
    }

    [Table("DiemDanh")]
    public class DiemDanh
    {
        [Key] public int Id { get; set; }

        public int MaLHP { get; set; }
        [ForeignKey(nameof(MaLHP))] public LopHocPhan? LopHocPhan { get; set; }

        public DateTime Ngay { get; set; }   // chỉ dùng phần Date

        public int MaSinhVien { get; set; }
        [ForeignKey(nameof(MaSinhVien))] public SinhVien? SinhVien { get; set; }

        public AttendanceStatus TrangThai { get; set; } = AttendanceStatus.CoMat;
        public string? GhiChu { get; set; }
    }
}
