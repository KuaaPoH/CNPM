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

        public async Task<IActionResult> Index(string? hk, string? nam)
        {
            // 1. Get Distinct Years/Semesters for Filter
            var namHocs = await _db.LopHocPhans.Select(x => x.NamHoc).Distinct().OrderByDescending(x => x).ToListAsync();
            var hocKys = await _db.LopHocPhans.Select(x => x.HocKy).Distinct().OrderBy(x => x).ToListAsync();

            // Default Filter Logic: Chọn năm mới nhất -> Chọn học kỳ mới nhất của năm đó
            if (string.IsNullOrEmpty(nam) && namHocs.Any()) nam = namHocs.First();
            if (string.IsNullOrEmpty(hk) && !string.IsNullOrEmpty(nam))
            {
                // Thử lấy học kỳ mới nhất CÓ DỮ LIỆU trong năm đó
                var lastHK = await _db.LopHocPhans.Where(x => x.NamHoc == nam)
                                    .Select(x => x.HocKy)
                                    .Distinct()
                                    .OrderByDescending(x => x)
                                    .FirstOrDefaultAsync();
                hk = lastHK ?? (hocKys.FirstOrDefault() ?? "HK1");
            }

            // Init VM
            var vm = new AdminDashboardVM
            {
                SelectedNamHoc = nam ?? "",
                SelectedHocKy = hk ?? "",
                NamHocList = namHocs,
                HocKyList = hocKys
            };

            // 2. Base Query theo Filter
            var queryLop = _db.LopHocPhans.AsNoTracking()
                .Where(x => x.NamHoc == vm.SelectedNamHoc && x.HocKy == vm.SelectedHocKy);

            // === KPI STATS (Theo kỳ đã chọn) ===
            // Tổng SV: Đếm distinct MaSV trong bảng DiemLop của các lớp trong kỳ này (chính xác hơn count bảng SinhVien)
            // Tuy nhiên để nhanh, ta có thể count bảng SinhVien (tổng toàn trường) hoặc count theo đăng ký.
            // Ở đây mình count tổng toàn trường hoạt động (thường KPI Dashboard cần số tổng quan)
            vm.TotalSinhVien = await _db.SinhViens.CountAsync(x => x.TrangThai);
            
            // Giảng viên tham gia dạy trong kỳ
            vm.TotalGiangVien = await queryLop.Select(x => x.MaGiangVien).Distinct().CountAsync();

            vm.TotalLopHocPhan = await queryLop.CountAsync();
            
            // Lớp chờ duyệt (Trong kỳ này)
            vm.TotalChoDuyet = await queryLop.CountAsync(x => x.DiemStatus == DiemStatus.Submitted);


            // === CHART 1: Trạng thái nhập điểm ===
            var statusStats = await queryLop
                .GroupBy(x => x.DiemStatus)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            vm.StatusLabels = new List<string> { "Đang nhập/Sửa", "Chờ duyệt", "Đã chốt" };
            vm.StatusCounts = new List<int>
            {
                statusStats.FirstOrDefault(s => s.Status == DiemStatus.Editable)?.Count ?? 0,
                statusStats.FirstOrDefault(s => s.Status == DiemStatus.Submitted)?.Count ?? 0,
                statusStats.FirstOrDefault(s => s.Status == DiemStatus.Approved)?.Count ?? 0
            };

            // === TABLE: Lớp vừa gửi duyệt (Global - không cần theo kỳ để Admin xử lý nhanh) ===
            vm.RecentSubmitted = await _db.LopHocPhans
                .AsNoTracking()
                .Include(x => x.HocPhan)
                .Include(x => x.GiangVien)
                .Where(x => x.DiemStatus == DiemStatus.Submitted)
                .OrderByDescending(x => x.SubmittedAt)
                .Take(5)
                .ToListAsync();

            // === CHART 2 & WARNING: Phân tích điểm (Chỉ tính các lớp ĐÃ CHỐT) ===
            // Lấy ID các lớp đã chốt trong kỳ
            var lopApprovedIds = queryLop.Where(x => x.DiemStatus == DiemStatus.Approved).Select(x => x.MaLHP);
            
            // Lấy điểm tổng kết
            var allScores = await _db.DiemLops
                .AsNoTracking()
                .Where(d => lopApprovedIds.Contains(d.MaLHP) && d.DiemTong != null)
                .Select(d => new { d.MaLHP, Diem = d.DiemTong.Value })
                .ToListAsync();

            // Phổ điểm
            int gioi = allScores.Count(s => s.Diem >= 8.0m);
            int kha = allScores.Count(s => s.Diem >= 6.5m && s.Diem < 8.0m);
            int tb = allScores.Count(s => s.Diem >= 5.0m && s.Diem < 6.5m);
            int yeu = allScores.Count(s => s.Diem < 5.0m);

            vm.GradeDistributionCounts = new List<int> { gioi, kha, tb, yeu };
            vm.GradeDistributionLabels = new List<string> { "Giỏi (≥8.0)", "Khá (6.5-7.9)", "Trung bình (5.0-6.4)", "Rớt (<5.0)" };

            // === CẢNH BÁO: Top lớp có tỷ lệ rớt cao (> 30% chẳng hạn) ===
            // Group score theo MaLHP
            var lopStats = allScores
                .GroupBy(s => s.MaLHP)
                .Select(g => new
                {
                    MaLHP = g.Key,
                    Total = g.Count(),
                    Rot = g.Count(s => s.Diem < 5.0m)
                })
                .Where(x => x.Total > 0)
                .Select(x => new
                {
                    x.MaLHP,
                    x.Total,
                    x.Rot,
                    Rate = (double)x.Rot * 100.0 / x.Total
                })
                .Where(x => x.Rate >= 30) // Chỉ lấy lớp rớt >= 30%
                .OrderByDescending(x => x.Rate)
                .Take(5) // Top 5
                .ToList();

            // Join ngược lại để lấy tên lớp/GV (client-side join vì lopStats là list memory)
            if (lopStats.Any())
            {
                var badLopIds = lopStats.Select(x => x.MaLHP).ToList();
                var badLopsInfo = await _db.LopHocPhans
                    .Include(x => x.HocPhan)
                    .Include(x => x.GiangVien)
                    .Where(x => badLopIds.Contains(x.MaLHP))
                    .ToListAsync();

                foreach (var stat in lopStats)
                {
                    var info = badLopsInfo.FirstOrDefault(x => x.MaLHP == stat.MaLHP);
                    if (info != null)
                    {
                        vm.TopLopRotCao.Add(new LopCanhBaoVM
                        {
                            MaLHP = stat.MaLHP,
                            TenHP = info.HocPhan?.TenHP ?? "",
                            TenNhom = info.TenNhom ?? "",
                            TenGV = info.GiangVien?.HoTen ?? "N/A",
                            SoSV = stat.Total,
                            SoRot = stat.Rot,
                            TyLeRot = Math.Round(stat.Rate, 1)
                        });
                    }
                }
            }

            return View(vm);
        }
    }
}
