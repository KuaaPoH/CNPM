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

        // Phân tiết phải dạng LT/TH/DA, ví dụ "30/0/15"
        private static readonly Regex _phanTietRe = new(@"^\d+/\d+/\d+$");

        // GET: /Admin/HocPhan
        public async Task<IActionResult> Index(string? q, int page = 1)
        {
            const int pageSize = 10;

            var query = _db.HocPhans.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var k = q.Trim().ToLower();
                query = query.Where(x =>
                    ((x.MaSoHP ?? "").ToLower().Contains(k)) ||
                    ((x.TenHP ?? "").ToLower().Contains(k)) ||
                    x.SoTinChi.ToString().Contains(k) ||
                    ((x.PhanTiet ?? "").ToLower().Contains(k)));
            }

            var total = await query.CountAsync();
            var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
            page = Math.Clamp(page, 1, totalPages);

            var list = await query
                .OrderBy(x => x.MaHP)            // MaSoHP là computed, sắp theo MaHP ổn định
                .ThenBy(x => x.TenHP)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.Q = q ?? "";
            return View(list);
        }

        // GET: Create
        [HttpGet]
        public IActionResult Create() => View();

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HocPhan model)
        {
            // KHÔNG ghi MaSoHP vì là computed ở DB
            ModelState.Remove(nameof(HocPhan.MaSoHP));

            if (!_phanTietRe.IsMatch(model.PhanTiet ?? ""))
                ModelState.AddModelError(nameof(HocPhan.PhanTiet), "Phân tiết phải theo dạng LT/TH/DA, ví dụ: 30/0/15.");

            if (!ModelState.IsValid) return View(model);

            // Không cho trùng tên học phần
            if (await _db.HocPhans.AnyAsync(x => x.TenHP == model.TenHP))
            {
                ModelState.AddModelError(nameof(HocPhan.TenHP), "Tên học phần đã tồn tại.");
                return View(model);
            }

            _db.HocPhans.Add(model);            // MaSoHP sẽ tự sinh ở DB
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Edit
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var hp = await _db.HocPhans.FindAsync(id);
            if (hp == null) return NotFound();
            return View(hp);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(HocPhan model)
        {
            // KHÔNG bind/validate MaSoHP
            ModelState.Remove(nameof(HocPhan.MaSoHP));

            if (!_phanTietRe.IsMatch(model.PhanTiet ?? ""))
                ModelState.AddModelError(nameof(HocPhan.PhanTiet), "Phân tiết phải theo dạng LT/TH/DA, ví dụ: 30/0/15.");
            if (!ModelState.IsValid) return View(model);

            // Không cho trùng tên với học phần khác
            var tenTrung = await _db.HocPhans.AnyAsync(x => x.MaHP != model.MaHP && x.TenHP == model.TenHP);
            if (tenTrung)
            {
                ModelState.AddModelError(nameof(HocPhan.TenHP), "Tên học phần đã tồn tại.");
                return View(model);
            }

            _db.Entry(model).Property(x => x.MaSoHP).IsModified = false; // bảo vệ cột computed
            _db.Update(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
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
