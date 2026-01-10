using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using aznews.Areas.Admin.Models;
using aznews.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace aznews.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SinhVienController : Controller
    {
        private readonly DataContext _db;
        private readonly IWebHostEnvironment _env;

        public SinhVienController(DataContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // ========== INDEX ==========
        public async Task<IActionResult> Index(string? q, int page = 1)
        {
            const int pageSize = 10;

            var query = _db.SinhViens
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var kw = q.Trim().ToLower();
                query = query.Where(x =>
                    (x.MaSoSV != null && x.MaSoSV.ToLower().Contains(kw)) ||
                    (x.HoTen != null && x.HoTen.ToLower().Contains(kw)) ||
                    (x.Email != null && x.Email.ToLower().Contains(kw)));
            }

            var total = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);
            if (totalPages == 0) totalPages = 1;
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var list = await query
                .OrderBy(x => x.HoTen)
                .ThenBy(x => x.MaSoSV)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.Q = q ?? string.Empty;

            return View(list);
        }

        // ========== CREATE ==========
        [HttpGet]
        public IActionResult Create()
        {
            var sv = new SinhVien { MaVaiTro = 3, TrangThai = true };
            return View(sv);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SinhVien model, IFormFile? avatar)
        {
            if (string.IsNullOrWhiteSpace(model.MaSoSV))
                ModelState.AddModelError(nameof(model.MaSoSV), "Mã sinh viên không được để trống.");
            if (string.IsNullOrWhiteSpace(model.HoTen))
                ModelState.AddModelError(nameof(model.HoTen), "Họ tên không được để trống.");

            if (!string.IsNullOrWhiteSpace(model.MaSoSV))
            {
                bool dup = await _db.SinhViens.AnyAsync(x => x.MaSoSV == model.MaSoSV);
                if (dup) ModelState.AddModelError(nameof(model.MaSoSV), "Mã sinh viên đã tồn tại.");
            }

            if (!ModelState.IsValid) return View(model);

            model.MaVaiTro = 3;

            if (!string.IsNullOrEmpty(model.MatKhau))
            {
                model.MatKhau = BCrypt.Net.BCrypt.HashPassword(model.MatKhau);
            }

            if (avatar != null && avatar.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploads);

                var fileName = Path.GetFileNameWithoutExtension(avatar.FileName);
                var ext = Path.GetExtension(avatar.FileName);
                var safe = $"{fileName}_{DateTime.Now:yyyyMMddHHmmss}{ext}";
                foreach (var c in Path.GetInvalidFileNameChars()) safe = safe.Replace(c, '_');

                var full = Path.Combine(uploads, safe);
                using (var stream = new FileStream(full, FileMode.Create))
                    await avatar.CopyToAsync(stream);

                model.AnhDaiDien = safe;
            }

            _db.SinhViens.Add(model);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Thêm sinh viên thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ========== EDIT ==========
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var sv = await _db.SinhViens.FindAsync(id);
            if (sv == null) return NotFound();
            return View(sv);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SinhVien model, string? RemoveAvatar, IFormFile? avatar)
        {
            var sv = await _db.SinhViens.FindAsync(model.MaSinhVien);
            if (sv == null) return NotFound();

            if (string.IsNullOrWhiteSpace(model.MaSoSV))
                ModelState.AddModelError(nameof(model.MaSoSV), "Mã sinh viên không được để trống.");
            if (string.IsNullOrWhiteSpace(model.HoTen))
                ModelState.AddModelError(nameof(model.HoTen), "Họ tên không được để trống.");

            if (!string.IsNullOrWhiteSpace(model.MaSoSV))
            {
                bool dup = await _db.SinhViens.AnyAsync(x => x.MaSoSV == model.MaSoSV && x.MaSinhVien != model.MaSinhVien);
                if (dup) ModelState.AddModelError(nameof(model.MaSoSV), "Mã sinh viên đã tồn tại.");
            }

            if (!ModelState.IsValid) return View(model);

            sv.MaSoSV = model.MaSoSV;
            sv.HoTen = model.HoTen;
            sv.NgaySinh = model.NgaySinh;
            sv.GioiTinh = model.GioiTinh;
            sv.Email = model.Email;
            sv.SoDienThoai = model.SoDienThoai;
            sv.DiaChi = model.DiaChi;
            sv.TrangThai = model.TrangThai;
            sv.MaVaiTro = 3;

            if (!string.IsNullOrWhiteSpace(model.MatKhau))
            {
                sv.MatKhau = BCrypt.Net.BCrypt.HashPassword(model.MatKhau);
            }

            if (RemoveAvatar == "true" && !string.IsNullOrEmpty(sv.AnhDaiDien))
            {
                var old = Path.Combine(_env.WebRootPath, "uploads", sv.AnhDaiDien);
                if (System.IO.File.Exists(old)) System.IO.File.Delete(old);
                sv.AnhDaiDien = null;
            }

            if (avatar != null && avatar.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploads);

                var fileName = Path.GetFileNameWithoutExtension(avatar.FileName);
                var ext = Path.GetExtension(avatar.FileName);
                var safe = $"{fileName}_{DateTime.Now:yyyyMMddHHmmss}{ext}";
                foreach (var c in Path.GetInvalidFileNameChars()) safe = safe.Replace(c, '_');

                var full = Path.Combine(uploads, safe);
                using (var stream = new FileStream(full, FileMode.Create))
                    await avatar.CopyToAsync(stream);

                sv.AnhDaiDien = safe;
            }

            _db.Update(sv);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Cập nhật sinh viên thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ========== TOGGLE TRẠNG THÁI ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleTrangThai(int id)
        {
            var sv = await _db.SinhViens.FindAsync(id);
            if (sv == null) return NotFound();

            sv.TrangThai = !sv.TrangThai;

            _db.SinhViens.Update(sv);
            await _db.SaveChangesAsync();

            TempData["Success"] = sv.TrangThai ? "Đã bật hoạt động!" : "Đã tắt hoạt động!";
            return RedirectToAction(nameof(Index));
        }


        // ========== DELETE ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var sv = await _db.SinhViens.FindAsync(id);
            if (sv == null) return NotFound();

            if (!string.IsNullOrEmpty(sv.AnhDaiDien))
            {
                var path = Path.Combine(_env.WebRootPath, "uploads", sv.AnhDaiDien);
                if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
            }

            _db.SinhViens.Remove(sv);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Xoá sinh viên thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
