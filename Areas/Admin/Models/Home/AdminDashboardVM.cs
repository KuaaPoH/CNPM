using aznews.Models;

namespace aznews.Areas.Admin.Models.Home
{
    public class AdminDashboardVM
    {
        // === KPI Cards ===
        public int TotalSinhVien { get; set; }
        public int TotalGiangVien { get; set; }
        public int TotalLopHocPhan { get; set; }
        public int TotalChoDuyet { get; set; }

        // === Filter Data ===
        public string SelectedHocKy { get; set; } = "";
        public string SelectedNamHoc { get; set; } = "";
        public List<string> NamHocList { get; set; } = new();
        public List<string> HocKyList { get; set; } = new();

        // === Charts Data ===
        // 1. Trạng thái nhập điểm
        public List<int> StatusCounts { get; set; } = new();
        public List<string> StatusLabels { get; set; } = new();

        // 2. Phổ điểm (Grade Distribution)
        public List<int> GradeDistributionCounts { get; set; } = new(); // [Giỏi, Khá, TB, Yếu]
        public List<string> GradeDistributionLabels { get; set; } = new();

        // === Tables / Lists ===
        // Lớp vừa gửi điểm (chờ duyệt)
        public List<aznews.Areas.Admin.Models.LopHocPhan> RecentSubmitted { get; set; } = new();

        // [MỚI] Cảnh báo học vụ: Top các lớp có tỷ lệ rớt cao (>30% chẳng hạn)
        public List<LopCanhBaoVM> TopLopRotCao { get; set; } = new();
    }

    public class LopCanhBaoVM
    {
        public int MaLHP { get; set; }
        public string TenHP { get; set; } = "";
        public string TenNhom { get; set; } = "";
        public string TenGV { get; set; } = "";
        public int SoSV { get; set; }
        public int SoRot { get; set; }
        public double TyLeRot { get; set; } // %
    }
}