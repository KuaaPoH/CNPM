using aznews.Areas.Admin.Models;
using aznews.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace aznews.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    public class LopDayController : Controller
    {
        private readonly DataContext _db;
        public LopDayController(DataContext db) => _db = db;

        // /GiangVien/LopDay
        public async Task<IActionResult> Index(int page = 1, string? hk = null, string? nam = null, string? q = null)
        {
            const int pageSize = 10;

            var username = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(username)) return RedirectToAction("Login", "Account");

            // Map NguoiDung -> GiangVien thông qua MaND
            var gv = await _db.NguoiDungs
                .Where(x => x.TenDangNhap == username)
                .Join(_db.GiangViens, nd => nd.MaND, gv => gv.MaND, (nd, gv) => gv)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (gv == null) return Unauthorized();

            // Lấy lớp GV đang dạy, Include HocPhan để hiển thị TenHP/SoTinChi
            var query = _db.LopHocPhans
                .AsNoTracking()
                .Include(x => x.HocPhan)
                .Where(x => x.MaGiangVien == gv.MaGiangVien);

            if (!string.IsNullOrWhiteSpace(hk)) query = query.Where(x => x.HocKy == hk);
            if (!string.IsNullOrWhiteSpace(nam)) query = query.Where(x => x.NamHoc == nam);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var kw = q.Trim().ToLower();
                query = query.Where(x =>
                    (x.HocPhan != null && x.HocPhan.TenHP.ToLower().Contains(kw)) ||
                    x.HocKy.ToLower().Contains(kw) ||
                    x.NamHoc.ToLower().Contains(kw));
            }

            // Dropdown Năm học cho GV này
            ViewBag.NamHocs = await _db.LopHocPhans
                .Where(x => x.MaGiangVien == gv.MaGiangVien)
                .Select(x => x.NamHoc)
                .Distinct()
                .OrderByDescending(x => x)
                .ToListAsync();

            // Phân trang
            int total = await query.CountAsync();
            int totalPages = (int)System.Math.Ceiling(total / (double)pageSize);
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var list = await query
                .OrderByDescending(x => x.NamHoc)
                .ThenBy(x => x.HocKy)
                .ThenBy(x => x.MaLHP)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.HK = hk;
            ViewBag.NAM = nam;
            ViewBag.Q = q;

            return View(list);
        }
    }
}
