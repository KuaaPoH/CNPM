using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using aznews.Areas.Admin.Models;
using aznews.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace aznews.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ThongBaoController : Controller
    {
        private readonly DataContext _context;
        private readonly IWebHostEnvironment _env;

        public ThongBaoController(DataContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ========== LIST ==========
        public async Task<IActionResult> Index(string? searchString, string? loai, int page = 1)
        {
            const int pageSize = 10;

            var query = _context.ThongBaos
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(loai) && loai != "Tất cả loại")
                query = query.Where(t => t.LoaiThongBao == loai);

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var kw = searchString.Trim().ToLower();
                query = query.Where(t =>
                    (t.TieuDe != null && t.TieuDe.ToLower().Contains(kw)) ||
                    (t.NoiDung != null && t.NoiDung.ToLower().Contains(kw)));
            }

            int total = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(total / (double)pageSize);
            if (totalPages == 0) totalPages = 1;
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var list = await query
                .OrderByDescending(t => t.NgayDang)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // list loại để filter
            ViewBag.ListLoai = await _context.ThongBaos
                .Where(x => x.LoaiThongBao != null && x.LoaiThongBao != "")
                .Select(x => x.LoaiThongBao)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;
            ViewData["SearchString"] = searchString;
            ViewData["Loai"] = loai;

            return View(list);
        }

        // ========== BẬT / TẮT ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleTrangThai(int id)
        {
            var tb = await _context.ThongBaos.FindAsync(id);
            if (tb == null) return NotFound();

            tb.TrangThai = !tb.TrangThai;
            _context.Update(tb);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ========== CREATE ==========
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.LoaiThongBaoList = new SelectList(
                _context.ThongBaos
                    .Where(x => x.LoaiThongBao != null && x.LoaiThongBao != "")
                    .Select(x => x.LoaiThongBao)
                    .Distinct()
                    .ToList()
            );
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ThongBao model, IFormFile? file)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.LoaiThongBaoList = new SelectList(
                    _context.ThongBaos
                        .Where(x => x.LoaiThongBao != null && x.LoaiThongBao != "")
                        .Select(x => x.LoaiThongBao)
                        .Distinct()
                        .ToList()
                );
                return View(model);
            }

            // mặc định: thông báo do admin (1) đăng
            model.MaVaiTro = 1;
            model.NgayDang = DateTime.Now;
            if (model.TrangThai == false) model.TrangThai = true;

            // upload file
            if (file != null && file.Length > 0)
            {
                string uploads = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

                string ext = Path.GetExtension(file.FileName);
                string name = Path.GetFileNameWithoutExtension(file.FileName);
                string finalName = $"{name}_{DateTime.Now:yyyyMMddHHmmss}{ext}";
                foreach (var c in Path.GetInvalidFileNameChars())
                    finalName = finalName.Replace(c, '_');

                string path = Path.Combine(uploads, finalName);
                using (var stream = new FileStream(path, FileMode.Create))
                    await file.CopyToAsync(stream);

                model.TepDinhKem = finalName;
            }

            _context.ThongBaos.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ========== EDIT ==========
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var tb = await _context.ThongBaos.FindAsync(id);
            if (tb == null) return NotFound();

            ViewBag.LoaiThongBaoList = new SelectList(
                _context.ThongBaos
                    .Where(x => x.LoaiThongBao != null && x.LoaiThongBao != "")
                    .Select(x => x.LoaiThongBao)
                    .Distinct()
                    .ToList(),
                tb.LoaiThongBao
            );

            return View(tb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ThongBao model, string? RemoveFile, IFormFile? file)
        {
            var tb = await _context.ThongBaos.FindAsync(model.MaTB);
            if (tb == null) return NotFound();

            tb.TieuDe = model.TieuDe;
            tb.NoiDung = model.NoiDung;
            tb.LoaiThongBao = model.LoaiThongBao;
            tb.TrangThai = model.TrangThai;
            tb.MaVaiTro = 1; // vẫn là admin đăng

            // xóa file cũ
            if (RemoveFile == "true" && !string.IsNullOrEmpty(tb.TepDinhKem))
            {
                string old = Path.Combine(_env.WebRootPath, "uploads", tb.TepDinhKem);
                if (System.IO.File.Exists(old)) System.IO.File.Delete(old);
                tb.TepDinhKem = null;
            }

            // upload file mới
            if (file != null && file.Length > 0)
            {
                string uploads = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

                string ext = Path.GetExtension(file.FileName);
                string name = Path.GetFileNameWithoutExtension(file.FileName);
                string finalName = $"{name}_{DateTime.Now:yyyyMMddHHmmss}{ext}";

                string path = Path.Combine(uploads, finalName);
                using (var stream = new FileStream(path, FileMode.Create))
                    await file.CopyToAsync(stream);

                tb.TepDinhKem = finalName;
            }

            _context.Update(tb);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ========== DELETE ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var tb = await _context.ThongBaos.FindAsync(id);
            if (tb == null) return NotFound();

            if (!string.IsNullOrEmpty(tb.TepDinhKem))
            {
                string path = Path.Combine(_env.WebRootPath, "uploads", tb.TepDinhKem);
                if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
            }

            _context.ThongBaos.Remove(tb);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
