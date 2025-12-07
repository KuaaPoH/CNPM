using aznews.Areas.Admin.Models;
using aznews.Areas.Admin.Models.Home;
using aznews.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace aznews.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly DataContext _db;
        public HomeController(DataContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new AdminDashboardVM();

            // 1. KPI Counts
            vm.TotalSinhVien = await _db.SinhViens.CountAsync(x => x.TrangThai);
            vm.TotalGiangVien = await _db.GiangViens.CountAsync(x => x.TrangThai);
            vm.TotalLopHocPhan = await _db.LopHocPhans.CountAsync();
            
            // Số lớp đang chờ duyệt (Submitted)
            vm.TotalChoDuyet = await _db.LopHocPhans.CountAsync(x => x.DiemStatus == DiemStatus.Submitted);

            // 2. Chart Data: Thống kê trạng thái nhập điểm
            // Group by DiemStatus
            var statusGroup = await _db.LopHocPhans
                .GroupBy(x => x.DiemStatus)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            // Map ra list cố định để vẽ biểu đồ cho dễ (Editable, Submitted, Approved)
            // Thứ tự: [0] Editable, [1] Submitted, [2] Approved
            int c1 = statusGroup.FirstOrDefault(x => x.Status == DiemStatus.Editable)?.Count ?? 0;
            int c2 = statusGroup.FirstOrDefault(x => x.Status == DiemStatus.Submitted)?.Count ?? 0;
            int c3 = statusGroup.FirstOrDefault(x => x.Status == DiemStatus.Approved)?.Count ?? 0;

            vm.StatusCounts = new List<int> { c1, c2, c3 };
            vm.StatusLabels = new List<string> { "Đang nhập/Sửa", "Chờ duyệt", "Đã chốt" };

            // 3. Danh sách 5 lớp vừa gửi điểm gần nhất
            vm.RecentSubmitted = await _db.LopHocPhans
                .AsNoTracking()
                .Include(x => x.HocPhan)
                .Include(x => x.GiangVien)
                .Where(x => x.DiemStatus == DiemStatus.Submitted)
                .OrderByDescending(x => x.SubmittedAt)
                .Take(5)
                .ToListAsync();

            // 4. Biểu đồ Phổ điểm (Chỉ tính các lớp ĐÃ DUYỆT - Approved)
            // Lấy toàn bộ điểm tổng kết (DiemTong) của các lớp đã duyệt
            // Lưu ý: Query này có thể nặng nếu dữ liệu lớn -> Cần tối ưu sau.
            // Ở đây demo mình load list diemTong về memory rồi count cho nhanh (với data nhỏ).
            // Nếu data lớn: Dùng SQL Case When trực tiếp.
            
            var scores = await _db.DiemLops
                .AsNoTracking()
                .Where(d => _db.LopHocPhans.Any(l => l.MaLHP == d.MaLHP && l.DiemStatus == DiemStatus.Approved))
                .Where(d => d.DiemTong != null)
                .Select(d => d.DiemTong)
                .ToListAsync();

            int gioi = scores.Count(s => s >= 8.0m);
            int kha = scores.Count(s => s >= 6.5m && s < 8.0m);
            int tb = scores.Count(s => s >= 5.0m && s < 6.5m);
            int yeu = scores.Count(s => s < 5.0m);

            vm.GradeDistributionCounts = new List<int> { gioi, kha, tb, yeu };
            vm.GradeDistributionLabels = new List<string> { "Giỏi (>=8.0)", "Khá (6.5-7.9)", "Trung bình (5.0-6.4)", "Rớt (<5.0)" };

            return View(vm);
        }
    }
}