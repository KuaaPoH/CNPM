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
    public class LopHanhChinhController : Controller
    {
        private readonly DataContext _context;

        public LopHanhChinhController(DataContext context)
        {
            _context = context;
        }

        // ------------------ INDEX ------------------
        public async Task<IActionResult> Index(string? searchString, int page = 1)
        {
            const int pageSize = 10;

            var query = _context.LopHanhChinhs
                                .Include(l => l.Nganh)
                                .AsNoTracking()
                                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var k = searchString.Trim().ToLower();
                query = query.Where(l =>
                    (l.TenLopHC != null && l.TenLopHC.ToLower().Contains(k)) ||
                    (l.KhoaHoc != null && l.KhoaHoc.ToLower().Contains(k)) ||
                    (l.Nganh != null && l.Nganh.TenNganh.ToLower().Contains(k))
                );
            }

            int total = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(total / (double)pageSize);
            if (totalPages == 0) totalPages = 1;
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var list = await query
                .OrderBy(l => l.MaLopHC)
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
            ViewBag.NganhList = new SelectList(_context.Nganhs, "MaNganh", "TenNganh");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LopHanhChinh model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.NganhList = new SelectList(_context.Nganhs, "MaNganh", "TenNganh", model.MaNganh);
                return View(model);
            }

            // Kiểm tra trùng tên lớp
            if (await _context.LopHanhChinhs.AnyAsync(l => l.TenLopHC == model.TenLopHC))
            {
                ModelState.AddModelError("", "Tên lớp hành chính đã tồn tại.");
                ViewBag.NganhList = new SelectList(_context.Nganhs, "MaNganh", "TenNganh", model.MaNganh);
                return View(model);
            }

            _context.LopHanhChinhs.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ------------------ EDIT ------------------
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var lop = await _context.LopHanhChinhs.FindAsync(id);
            if (lop == null) return NotFound();

            ViewBag.NganhList = new SelectList(_context.Nganhs, "MaNganh", "TenNganh", lop.MaNganh);
            return View(lop);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LopHanhChinh model)
        {
            if (id != model.MaLopHC) return NotFound();
            if (!ModelState.IsValid)
            {
                ViewBag.NganhList = new SelectList(_context.Nganhs, "MaNganh", "TenNganh", model.MaNganh);
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
                ViewBag.NganhList = new SelectList(_context.Nganhs, "MaNganh", "TenNganh", model.MaNganh);
                return View(model);
            }
        }

        // ------------------ DELETE ------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var lop = await _context.LopHanhChinhs.FindAsync(id);
            if (lop == null) return NotFound();

            _context.LopHanhChinhs.Remove(lop);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Xóa lớp hành chính thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}
