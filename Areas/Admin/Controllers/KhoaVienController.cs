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

        public async Task<IActionResult> Index(string? searchString, int page = 1)
        {
            const int pageSize = 10;

            var query = _context.KhoaViens
                                .AsNoTracking()
                                .AsQueryable();

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
            var ten = model.TenKhoaVien?.Trim();
            var diachi = model.DiaChi?.Trim();
            var email = model.Email?.Trim();
            var sdt = model.DienThoai?.Trim();

            if (string.IsNullOrWhiteSpace(ten))
                ModelState.AddModelError(nameof(model.TenKhoaVien), "Tên khoa viện không được để trống.");

            if (ModelState.IsValid)
            {
 
                bool nameExist = await _context.KhoaViens
                    .AnyAsync(x => x.TenKhoaVien != null && x.TenKhoaVien.ToLower() == ten!.ToLower());

                if (nameExist)
                    ModelState.AddModelError(nameof(model.TenKhoaVien), "Tên khoa viện đã tồn tại.");

                if (!string.IsNullOrEmpty(email))
                {
                    bool emailExist = await _context.KhoaViens
                        .AnyAsync(x => x.Email != null && x.Email.ToLower() == email.ToLower());

                    if (emailExist)
                        ModelState.AddModelError(nameof(model.Email), "Email đã tồn tại.");
                }
            }

            if (!ModelState.IsValid)
                return View(model);
            model.TenKhoaVien = ten;
            model.DiaChi = diachi;
            model.Email = email;
            model.DienThoai = sdt;

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
