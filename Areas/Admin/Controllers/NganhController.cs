using System;
using System.Linq;
using System.Threading.Tasks;
using aznews.Areas.Admin.Models;
using aznews.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace aznews.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class NganhController : Controller
    {
        private readonly DataContext _context;

        public NganhController(DataContext context)
        {
            _context = context;
        }

        // ------------------ INDEX ------------------
        public async Task<IActionResult> Index(string? searchString, int page = 1)
        {
            const int pageSize = 10;

            var query = _context.Nganhs
                                .Include(n => n.KhoaVien) // nạp thông tin khoa viện
                                .AsNoTracking()
                                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var k = searchString.Trim().ToLower();
                query = query.Where(n =>
                    (n.TenNganh != null && n.TenNganh.ToLower().Contains(k)) ||
                    (n.KhoaVien != null && n.KhoaVien.TenKhoaVien.ToLower().Contains(k)));
            }

            int total = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(total / (double)pageSize);
            if (totalPages == 0) totalPages = 1;
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var list = await query
                .OrderBy(n => n.MaNganh)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;
            ViewBag.Total = total;
            ViewData["SearchString"] = searchString;

            return View(list);
        }

        // ------------------ CREATE ------------------
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.KhoaVienList = new SelectList(_context.KhoaViens, "MaKhoaVien", "TenKhoaVien");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Nganh model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.KhoaVienList = new SelectList(_context.KhoaViens, "MaKhoaVien", "TenKhoaVien", model.MaKhoaVien);
                return View(model);
            }

            if (await _context.Nganhs.AnyAsync(n => n.TenNganh == model.TenNganh))
            {
                ModelState.AddModelError("", "Tên ngành đã tồn tại.");
                ViewBag.KhoaVienList = new SelectList(_context.KhoaViens, "MaKhoaVien", "TenKhoaVien", model.MaKhoaVien);
                return View(model);
            }

            _context.Nganhs.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ------------------ EDIT ------------------
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var ng = await _context.Nganhs.FindAsync(id);
            if (ng == null) return NotFound();

            ViewBag.KhoaVienList = new SelectList(_context.KhoaViens, "MaKhoaVien", "TenKhoaVien", ng.MaKhoaVien);
            return View(ng);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Nganh model)
        {
            if (id != model.MaNganh) return NotFound();
            if (!ModelState.IsValid)
            {
                ViewBag.KhoaVienList = new SelectList(_context.KhoaViens, "MaKhoaVien", "TenKhoaVien", model.MaKhoaVien);
                return View(model);
            }

            try
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError("", "Có lỗi khi cập nhật dữ liệu.");
                ViewBag.KhoaVienList = new SelectList(_context.KhoaViens, "MaKhoaVien", "TenKhoaVien", model.MaKhoaVien);
                return View(model);
            }
        }

        // ------------------ DELETE ------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var ng = await _context.Nganhs.FindAsync(id);
            if (ng == null) return NotFound();

            _context.Nganhs.Remove(ng);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Xóa ngành thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}
