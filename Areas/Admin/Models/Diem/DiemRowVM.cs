using System;

namespace aznews.Areas.Admin.Models.Diem
{
    // Dòng điểm cho 1 SV trong lớp học phần
    public class DiemRowVM
    {
        public int MaSinhVien { get; set; }
        public string MaSoSV { get; set; } = "";
        public string HoTen { get; set; } = "";
        public decimal? DiemQT { get; set; }
        public decimal? DiemThi { get; set; }

        // Điểm tổng = (QT + Thi)/2; làm tròn 2 số; null nếu cả 2 đều null
        public decimal? DiemTong =>
            (DiemQT.HasValue || DiemThi.HasValue)
                ? Math.Round(((DiemQT ?? 0m) + (DiemThi ?? 0m)) / 2m, 2)
                : (decimal?)null;
    }
}
