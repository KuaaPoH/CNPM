using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using aznews.Models;
using Microsoft.EntityFrameworkCore;
using aznews.Areas.Admin.Models;

namespace aznews.Controllers;

public class HomeController : Controller
{
    private readonly DataContext _db;

    public HomeController(DataContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        // Kiểm tra xem có phải Sinh viên đang đăng nhập không
        var role = HttpContext.Session.GetInt32("Role");
        var maSV = HttpContext.Session.GetInt32("MaSV");

        if (role == 3 && maSV.HasValue)
        {
            // --- LOGIC DASHBOARD SINH VIÊN ---
            var sv = await _db.SinhViens
                .Include(x => x.LopHanhChinh)
                .FirstOrDefaultAsync(x => x.MaSinhVien == maSV);

            if (sv == null) return View(); // Fallback nếu lỗi data

            var allScores = await _db.DiemLops
                .AsNoTracking()
                .Include(x => x.LopHocPhan).ThenInclude(l => l.HocPhan)
                .Where(x => x.MaSinhVien == maSV && x.LopHocPhan.DiemStatus == DiemStatus.Approved)
                .OrderByDescending(x => x.LopHocPhan.NamHoc)
                .ThenByDescending(x => x.LopHocPhan.HocKy)
                .ToListAsync();

            // Tính toán GPA
            double ToHe4(double d10) {
                if (d10 >= 8.5) return 4.0; if (d10 >= 7.0) return 3.0;
                if (d10 >= 5.5) return 2.0; if (d10 >= 4.0) return 1.0;
                return 0.0;
            }
            string ToChu(double d10) {
                if (d10 >= 8.5) return "A"; if (d10 >= 7.0) return "B";
                if (d10 >= 5.5) return "C"; if (d10 >= 4.0) return "D";
                return "F";
            }

            double totalPoints = 0; int totalCredits = 0; int failedCredits = 0;
            foreach (var item in allScores) {
                if (item.DiemTong.HasValue) {
                    double d4 = ToHe4((double)item.DiemTong.Value);
                    int tinChi = item.LopHocPhan?.HocPhan?.SoTinChi ?? 0;
                    totalPoints += d4 * tinChi; totalCredits += tinChi;
                    if (d4 == 0) failedCredits += tinChi;
                }
            }

            var vm = new SinhVienDashboardVM {
                HoTen = sv.HoTen, MaSV = sv.MaSoSV,
                LopHanhChinh = sv.LopHanhChinh?.TenLopHC ?? "Chưa phân lớp",
                Avatar = sv.AnhDaiDien ?? "default-user.png",
                GPA_TichLuy = totalCredits > 0 ? Math.Round(totalPoints / totalCredits, 2) : 0,
                TinChi_TichLuy = totalCredits,
                TinChi_No = failedCredits,
                MonDangHoc = await _db.DiemLops.Include(x => x.LopHocPhan).CountAsync(x => x.MaSinhVien == maSV && x.LopHocPhan.DiemStatus != DiemStatus.Approved),
                
                BieudoHocTap = allScores.GroupBy(x => new { x.LopHocPhan.NamHoc, x.LopHocPhan.HocKy })
                    .Select(g => new GPAPerSemester {
                        HocKyNam = $"{g.Key.HocKy} / {g.Key.NamHoc}",
                        GPA = g.Sum(s => s.DiemTong.HasValue ? ToHe4((double)s.DiemTong) * (s.LopHocPhan?.HocPhan?.SoTinChi ?? 0) : 0) / (g.Sum(s => s.LopHocPhan?.HocPhan?.SoTinChi ?? 0) == 0 ? 1 : g.Sum(s => s.LopHocPhan?.HocPhan?.SoTinChi ?? 0))
                    }).ToList(),
                DiemMoiNhat = allScores.Take(5).Select(x => new DiemLopHocPhanVM {
                    TenHP = x.LopHocPhan?.HocPhan?.TenHP ?? "", SoTinChi = x.LopHocPhan?.HocPhan?.SoTinChi ?? 0,
                    DiemCC = (double?)x.DiemQT, DiemCK = (double?)x.DiemThi, DiemTong = (double?)x.DiemTong,
                    DiemHe4 = x.DiemTong.HasValue ? ToHe4((double)x.DiemTong) : 0,
                    DiemChu = x.DiemTong.HasValue ? ToChu((double)x.DiemTong) : ""
                }).ToList()
            };

            return View("Index_Student", vm);
        }

        // Nếu là Admin hoặc GV hoặc chưa Login, trả về trang mặc định
        return View();
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}