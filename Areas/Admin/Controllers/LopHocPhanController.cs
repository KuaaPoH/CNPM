using aznews.Areas.Admin.Models;
using aznews.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace aznews.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LopHocPhanController : Controller
    {
        private readonly DataContext _db;
        public LopHocPhanController(DataContext db) => _db = db;

        // GET: /Admin/LopHocPhan
        public async Task<IActionResult> Index(string? q, string? hk, string? nam, int page = 1)
        {
            const int pageSize = 10;

            var query = _db.LopHocPhans
                           .AsNoTracking()
                           .Include(x => x.HocPhan)
                           .Include(x => x.GiangVien)
                           .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var k = q.Trim().ToLower();
                query = query.Where(x =>
                    (x.HocPhan != null && x.HocPhan.TenHP.ToLower().Contains(k)) ||
                    (x.GiangVien != null && x.GiangVien.HoTen.ToLower().Contains(k)));
            }
            if (!string.IsNullOrWhiteSpace(hk)) query = query.Where(x => x.HocKy == hk);
            if (!string.IsNullOrWhiteSpace(nam)) query = query.Where(x => x.NamHoc == nam);

            var total = await query.CountAsync();
            var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
            page = Math.Clamp(page, 1, totalPages);

            var list = await query.OrderByDescending(x => x.NamHoc)
                                  .ThenBy(x => x.HocKy)
                                  .ThenBy(x => x.MaLHP)
                                  .Skip((page - 1) * pageSize)
                                  .Take(pageSize)
                                  .ToListAsync();

            // dữ liệu filter
            ViewBag.HocKys = await _db.LopHocPhans.Select(x => x.HocKy).Distinct().OrderBy(x => x).ToListAsync();
            ViewBag.NamHocs = await _db.LopHocPhans.Select(x => x.NamHoc).Distinct().OrderByDescending(x => x).ToListAsync();

            ViewBag.Page = page; ViewBag.TotalPages = totalPages; ViewBag.PageSize = pageSize;
            ViewBag.Q = q ?? ""; ViewBag.HK = hk ?? ""; ViewBag.NAM = nam ?? "";
            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await FillDropdowns();
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LopHocPhan model)
        {
            if (!ModelState.IsValid)
            {
                await FillDropdowns();
                return View(model);
            }

            bool trùng = await _db.LopHocPhans
                .AnyAsync(x => x.MaHP == model.MaHP &&
                               x.MaGiangVien == model.MaGiangVien &&
                               x.HocKy == model.HocKy &&
                               x.NamHoc == model.NamHoc);
            if (trùng)
            {
                ModelState.AddModelError("", "Lớp học phần đã tồn tại (cùng Học phần, Giảng viên, Học kỳ, Năm học).");
                await FillDropdowns();
                return View(model);
            }

            _db.LopHocPhans.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var lhp = await _db.LopHocPhans.FindAsync(id);
            if (lhp == null) return NotFound();
            await FillDropdowns();
            return View(lhp);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(LopHocPhan model)
        {
            if (!ModelState.IsValid)
            {
                await FillDropdowns();
                return View(model);
            }

            bool trùng = await _db.LopHocPhans
                .AnyAsync(x => x.MaLHP != model.MaLHP &&
                               x.MaHP == model.MaHP &&
                               x.MaGiangVien == model.MaGiangVien &&
                               x.HocKy == model.HocKy &&
                               x.NamHoc == model.NamHoc);
            if (trùng)
            {
                ModelState.AddModelError("", "Lớp học phần đã tồn tại (cùng Học phần, Giảng viên, Học kỳ, Năm học).");
                await FillDropdowns();
                return View(model);
            }

            _db.Update(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var lhp = await _db.LopHocPhans.FindAsync(id);
            if (lhp == null) return NotFound();
            _db.LopHocPhans.Remove(lhp);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task FillDropdowns()
        {
            var hps = await _db.HocPhans.AsNoTracking()
                          .OrderBy(x => x.TenHP)
                          .Select(x => new { x.MaHP, Ten = x.TenHP + " (" + x.SoTinChi + " TC)" })
                          .ToListAsync();

            var gvs = await _db.GiangViens.AsNoTracking()
                          .OrderBy(x => x.HoTen)
                          .Select(x => new { x.MaGiangVien, Ten = x.MaSoGV + " - " + x.HoTen })
                          .ToListAsync();

            ViewBag.HocPhanList = new SelectList(hps, "MaHP", "Ten");
            ViewBag.GiangVienList = new SelectList(gvs, "MaGiangVien", "Ten");
            ViewBag.HocKyList = new SelectList(new[] { "HK1", "HK2", "Hè" });
        }
    }
}
