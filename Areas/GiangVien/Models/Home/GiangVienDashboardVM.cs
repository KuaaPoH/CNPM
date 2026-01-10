using aznews.Areas.Admin.Models;

namespace aznews.Areas.GiangVien.Models.Home
{
    public class GiangVienDashboardVM
    {
        // Thông tin chung
        public string HoTenGV { get; set; } = "";
        public string MaSoGV { get; set; } = "";

        // KPI
        public int LopDangDay { get; set; } // Số lớp kỳ này
        public int TongSinhVien { get; set; }
        public int LopChuaNhapDiem { get; set; } // Số lớp chưa approved

        // Lịch dạy hôm nay (Giả lập hoặc lấy từ thời khóa biểu nếu có table TKB)
        // Hiện tại DB chỉ có LopHocPhan, chưa có chi tiết Lịch học (Thứ/Tiết)
        // Nên mình sẽ hiển thị "Danh sách lớp phụ trách kỳ này"
        public List<LopHocPhan> LopPhuTrach { get; set; } = new();

        // Tiến độ nhập điểm (Cho biểu đồ)
        public int CountEditable { get; set; }
        public int CountSubmitted { get; set; }
        public int CountApproved { get; set; }
    }
}
