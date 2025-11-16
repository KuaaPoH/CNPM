using aznews.Areas.Admin.Models;
using aznews.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace aznews.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HocPhanController : Controller
    {
        private readonly DataContext _db;
        public HocPhanController(DataContext db) => _db = db;

        // Helpers
        private static readonly Regex _phanTietRe = new(@"^\d+/\d+/\d+$");

        private async Task<string> GenerateNextCodeAsync(string prefix = "INF")
        {
            var last = await _db.HocPhans
                .Where(x => x.MaSoHP.StartsWith(prefix))
                .OrderByDescending(x => x.MaSoHP)
                .Select(x => x.MaSoHP)
                .FirstOrDefaultAsync();

            var next = 1;
            if (!string.IsNullOrEmpty(last))
            {
                var digits = new string(last.Skip(prefix.Length).ToArray());
                if (int.TryParse(digits, out var n)) next = n + 1;
            }
            return $"{prefix}{next:000}";
        }

        // GET: /Admin/HocPhan
        public async Task<IActionResult> Index(string? q, int page = 1)
        {
            const int pageSize = 10;
            var query = _db.HocPhans.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var k = q.Trim().ToLower();
                query = query.Where(x =>
                    (x.MaSoHP != null && x.MaSoHP.ToLower().Contains(k)) ||
                    (x.TenHP != null && x.TenHP.ToLower().Contains(k)) ||
                    x.SoTinChi.ToString().Contains(k) ||
                    (x.PhanTiet != null && x.PhanTiet.ToLower().Contains(k)));
            }

            var total = await query.CountAsync();
            var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
            page = Math.Clamp(page, 1, totalPages);

            var list = await query.OrderBy(x => x.MaSoHP)
                                  .ThenBy(x => x.TenHP)
                                  .Skip((page - 1) * pageSize)
                                  .Take(pageSize)
                                  .ToListAsync();

            ViewBag.Page = page; ViewBag.TotalPages = totalPages; ViewBag.PageSize = pageSize; ViewBag.Q = q ?? "";
            return View(list);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HocPhan model)
        {
            // Chuẩn hóa
            if (string.IsNullOrWhiteSpace(model.MaSoHP))
                model.MaSoHP = await GenerateNextCodeAsync("INF");

            if (!_phanTietRe.IsMatch(model.PhanTiet ?? ""))
                ModelState.AddModelError(nameof(HocPhan.PhanTiet), "Phân tiết phải theo dạng LT/TH/DA, ví dụ: 30/0/15.");

            if (!ModelState.IsValid) return View(model);

            // Trùng mã & trùng tên
            if (await _db.HocPhans.AnyAsync(x => x.MaSoHP == model.MaSoHP))
            {
                ModelState.AddModelError(nameof(HocPhan.MaSoHP), "Mã học phần đã tồn tại.");
                return View(model);
            }
            if (await _db.HocPhans.AnyAsync(x => x.TenHP == model.TenHP))
            {
                ModelState.AddModelError(nameof(HocPhan.TenHP), "Tên học phần đã tồn tại.");
                return View(model);
            }

            _db.HocPhans.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var hp = await _db.HocPhans.FindAsync(id);
            if (hp == null) return NotFound();
            return View(hp);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(HocPhan model)
        {
            if (!_phanTietRe.IsMatch(model.PhanTiet ?? ""))
                ModelState.AddModelError(nameof(HocPhan.PhanTiet), "Phân tiết phải theo dạng LT/TH/DA, ví dụ: 30/0/15.");
            if (!ModelState.IsValid) return View(model);

            // Trùng (trừ chính nó)
            if (await _db.HocPhans.AnyAsync(x => x.MaHP != model.MaHP && x.MaSoHP == model.MaSoHP))
            {     
                ModelState.AddModelError(nameof(HocPhan.MaSoHP), "Mã học phần đã tồn tại.");

            return View(model);
            }
            if (await _db.HocPhans.AnyAsync(x => x.MaHP != model.MaHP && x.TenHP == model.TenHP))
            {
                ModelState.AddModelError(nameof(HocPhan.TenHP), "Tên học phần đã tồn tại.");
                return View(model);
            }

            _db.Update(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var hp = await _db.HocPhans.FindAsync(id);
            if (hp == null) return NotFound();
            _db.HocPhans.Remove(hp);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
