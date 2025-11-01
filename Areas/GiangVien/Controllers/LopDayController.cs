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

            // lấy từ session do AccountController đã set
            var maGV = HttpContext.Session.GetInt32("MaGV");
            if (maGV == null) return RedirectToAction("Login", "Account", new { area = "" });

            // lớp mà giảng viên này đang dạy
            var query = _db.LopHocPhans
                .AsNoTracking()
                .Include(x => x.HocPhan)
                .Where(x => x.MaGiangVien == maGV.Value);

            // filter
            if (!string.IsNullOrWhiteSpace(hk))
                query = query.Where(x => x.HocKy == hk);

            if (!string.IsNullOrWhiteSpace(nam))
                query = query.Where(x => x.NamHoc == nam);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var kw = q.Trim().ToLower();
                query = query.Where(x =>
                    (x.HocPhan != null && x.HocPhan.TenHP.ToLower().Contains(kw)) ||
                    x.HocKy.ToLower().Contains(kw) ||
                    x.NamHoc.ToLower().Contains(kw));
            }

            // dropdown năm học
            ViewBag.NamHocs = await _db.LopHocPhans
                .Where(x => x.MaGiangVien == maGV.Value)
                .Select(x => x.NamHoc)
                .Distinct()
                .OrderByDescending(x => x)
                .ToListAsync();

            // phân trang
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
