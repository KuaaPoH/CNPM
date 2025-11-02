using aznews.Areas.Admin.Models;
using aznews.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace aznews.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SinhVienController : Controller
    {
        private readonly DataContext _db;
        public SinhVienController(DataContext db) => _db = db;

        // GET: /Admin/SinhVien
        public async Task<IActionResult> Index(string? q, int page = 1)
        {
            const int pageSize = 10;
            var query = _db.SinhViens.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim().ToLower();
                query = query.Where(x =>
                    x.MaSoSV.ToLower().Contains(q) ||
                    x.HoTen.ToLower().Contains(q) ||
                    (x.Email != null && x.Email.ToLower().Contains(q)));
            }

            var total = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);
            if (totalPages == 0) totalPages = 1;
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var list = await query
                .OrderBy(x => x.HoTen)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.Q = q ?? "";

            return View(list);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SinhVien model)
        {
            if (!ModelState.IsValid) return View(model);
            _db.SinhViens.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var sv = await _db.SinhViens.FindAsync(id);
            if (sv == null) return NotFound();
            return View(sv);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SinhVien model)
        {
            if (!ModelState.IsValid) return View(model);
            _db.Update(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var sv = await _db.SinhViens.FindAsync(id);
            if (sv == null) return NotFound();
            _db.SinhViens.Remove(sv);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
