namespace aznews.Areas.Admin.Models
{
    public enum LopLoai : byte { LT = 1, TH = 2, DO_AN = 3 }

    public class LopHocPhan
    {
        public int MaLHP { get; set; }          // PK
        public int MaHP { get; set; }   // Mã học phần (kiểu của bạn, có thể NVARCHAR: INF001...)
        public int MaGiangVien { get; set; }
        public string HocKy { get; set; } = "";  // HK1/HK2/Hè
        public string NamHoc { get; set; } = ""; // 2024-2025

        public LopLoai? LoaiLop { get; set; }    // 1,2,3
        public int? MaLopCha { get; set; }       // lớp cha (khi tách TH/ĐA từ LT)
        public string? TenNhom { get; set; }     // N1/N2...
        public bool TrangThai { get; set; } = true;

        // nav
        public HocPhan? HocPhan { get; set; }
        public GiangVien? GiangVien { get; set; }
        public LopHocPhan? LopCha { get; set; }
        public ICollection<LopHocPhan>? LopCon { get; set; }
    }
}
