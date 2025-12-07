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

            // Hash mật khẩu
            if (!string.IsNullOrEmpty(model.MatKhau))
            {
                model.MatKhau = BCrypt.Net.BCrypt.HashPassword(model.MatKhau);
            }

            // Gán role mặc định nếu cần (ví dụ 2)
            model.MaVaiTro = 2; 

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
            // Không validate MatKhau bắt buộc ở Edit (nếu model có attribute Required thì cần cẩn thận)
            // Tốt nhất nên remove key khỏi ModelState nếu logic của bạn cho phép để trống
            if (string.IsNullOrEmpty(model.MatKhau)) 
            {
                ModelState.Remove("MatKhau");
            }

            if (!ModelState.IsValid) return View(model);

            var gv = await _db.GiangViens.FindAsync(model.MaGiangVien);
            if (gv == null) return NotFound();

            // Update info
            gv.MaSoGV = model.MaSoGV;
            gv.HoTen = model.HoTen;
            gv.Email = model.Email;
            gv.SoDienThoai = model.SoDienThoai;
            gv.TrangThai = model.TrangThai;
            // gv.MaVaiTro = 2; // Giữ nguyên hoặc set lại

            // Xử lý mật khẩu
            if (!string.IsNullOrWhiteSpace(model.MatKhau))
            {
                gv.MatKhau = BCrypt.Net.BCrypt.HashPassword(model.MatKhau);
            }
            // Nếu model.MatKhau rỗng -> Giữ nguyên gv.MatKhau cũ

            _db.Update(gv);
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
