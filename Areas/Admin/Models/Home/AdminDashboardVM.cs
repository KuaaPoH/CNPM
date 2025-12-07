using aznews.Models;

namespace aznews.Areas.Admin.Models.Home
{
    public class AdminDashboardVM
    {
        // 1. Thẻ số liệu (KPIs)
        public int TotalSinhVien { get; set; }
        public int TotalGiangVien { get; set; }
        public int TotalLopHocPhan { get; set; }
        public int TotalChoDuyet { get; set; } // Quan trọng

        // 2. Dữ liệu biểu đồ (Trạng thái nhập điểm)
        // [Editable, Submitted, Approved]
        public List<int> StatusCounts { get; set; } = new List<int>();
        public List<string> StatusLabels { get; set; } = new List<string>();

        // 3. Danh sách lớp vừa gửi điểm (cần duyệt)
        public List<LopHocPhan> RecentSubmitted { get; set; } = new List<LopHocPhan>();

        // 4. Biểu đồ Phổ điểm (Grade Distribution)
        // [Gioi, Kha, TB, Yeu]
        public List<int> GradeDistributionCounts { get; set; } = new List<int>();
        public List<string> GradeDistributionLabels { get; set; } = new List<string>();
    }
}
