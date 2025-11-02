using aznews.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// alias để tránh đụng với namespace aznews.Areas.GiangVien
using AdminGiangVien = aznews.Areas.Admin.Models.GiangVien;

namespace aznews.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class GiangVienController : Controller
    {
        private readonly DataContext _db;
        public GiangVienController(DataContext db) => _db = db;

        public async Task<IActionResult> Index(string? q, int page = 1)
        {
            const int pageSize = 10;

            var query = _db.GiangViens.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim().ToLower();
                query = query.Where(x =>
                    x.MaSoGV.ToLower().Contains(q) ||
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
        public async Task<IActionResult> Create(AdminGiangVien model)
        {
            if (!ModelState.IsValid) return View(model);
            _db.GiangViens.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var gv = await _db.GiangViens.FindAsync(id);
            if (gv == null) return NotFound();
            return View(gv);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AdminGiangVien model)
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
            var gv = await _db.GiangViens.FindAsync(id);
            if (gv == null) return NotFound();
            _db.GiangViens.Remove(gv);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
