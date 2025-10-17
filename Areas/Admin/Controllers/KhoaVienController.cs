using System;
using System.Linq;
using System.Threading.Tasks;
using aznews.Areas.Admin.Models;
using aznews.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace aznews.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class KhoaVienController : Controller
    {
        private readonly DataContext _context;

        public KhoaVienController(DataContext context)
        {
            _context = context;
        }


        // GET: Admin/KhoaVien
        // ?searchString=...&page=1
        public async Task<IActionResult> Index(string? searchString, int page = 1)
        {
            const int pageSize = 10;

            var query = _context.KhoaViens
                                .AsNoTracking()
                                .AsQueryable();

            // Tìm kiếm (không phân biệt hoa/thường, bỏ khoảng trắng thừa)
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var k = searchString.Trim().ToLower();
                query = query.Where(x =>
                    (x.TenKhoaVien != null && x.TenKhoaVien.ToLower().Contains(k)) ||
                    (x.Email != null && x.Email.ToLower().Contains(k)) ||
                    (x.DiaChi != null && x.DiaChi.ToLower().Contains(k)) ||
                    (x.DienThoai != null && x.DienThoai.ToLower().Contains(k)));
            }

            // Đếm & tính trang
            int total = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(total / (double)pageSize);
            if (totalPages == 0) totalPages = 1;
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            // Lấy dữ liệu trang hiện tại
            var tblist = await query
                .OrderBy(x => x.MaKhoaVien)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Gửi sang View
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;
            ViewBag.Total = total;
            ViewData["SearchString"] = searchString; // để giữ giá trị ô tìm kiếm

            return View(tblist);
        }




        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KhoaVien model)
        {
            if (!ModelState.IsValid) return View(model);

            if (await _context.KhoaViens.AnyAsync(k => k.TenKhoaVien == model.TenKhoaVien))
            {
                ModelState.AddModelError("", "Tên khoa viện đã tồn tại.");
                return View(model);
            }

            _context.KhoaViens.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var kv = await _context.KhoaViens.FirstOrDefaultAsync(k => k.MaKhoaVien == id);
            if (kv == null) return NotFound();
            return View(kv);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, KhoaVien model)
        {
            if (id != model.MaKhoaVien) return NotFound();
            if (!ModelState.IsValid) return View(model);

            try
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError("", "Có lỗi khi cập nhật dữ liệu.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var kv = await _context.KhoaViens.FindAsync(id);
            if (kv == null) return NotFound();

            _context.KhoaViens.Remove(kv);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Xóa khoa viện thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}
