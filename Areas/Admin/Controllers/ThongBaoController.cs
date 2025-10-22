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

        // ------------------ INDEX ------------------
        public async Task<IActionResult> Index(string? searchString, string? loai, int page = 1)
        {
            const int pageSize = 10;

            var query = _context.ThongBaos
                .Include(t => t.NguoiDung)
                .ThenInclude(nd => nd.VaiTro)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(loai) && loai != "Tất cả loại")
                query = query.Where(t => t.LoaiThongBao == loai);

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var keyword = searchString.Trim().ToLower();
                query = query.Where(t => t.TieuDe != null && t.TieuDe.ToLower().Contains(keyword));
            }

            int total = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(total / (double)pageSize);
            if (totalPages == 0) totalPages = 1;

            var list = await query
                .OrderByDescending(t => t.NgayDang)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;
            ViewData["SearchString"] = searchString;

            return View(list);
        }

        // ------------------ TOGGLE TRẠNG THÁI ------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleTrangThai(int id)
        {
            var tb = await _context.ThongBaos.FindAsync(id);
            if (tb == null)
                return NotFound();

            tb.TrangThai = !tb.TrangThai;
            _context.Update(tb);
            await _context.SaveChangesAsync();

            TempData["Success"] = tb.TrangThai
                ? "Thông báo đã được hiển thị!"
                : "Thông báo đã được ẩn!";

            return RedirectToAction(nameof(Index));
        }

        // ------------------ CREATE ------------------
        [HttpGet]
        public IActionResult Create()
        {
            var loaiList = _context.ThongBaos
                .Select(t => t.LoaiThongBao)
                .Where(l => !string.IsNullOrEmpty(l))
                .Distinct()
                .ToList();

            ViewBag.LoaiThongBaoList = new SelectList(loaiList);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ThongBao model, IFormFile? file)
        {
            if (!ModelState.IsValid)
            {
                var loaiList = _context.ThongBaos
                    .Select(t => t.LoaiThongBao)
                    .Where(l => !string.IsNullOrEmpty(l))
                    .Distinct()
                    .ToList();

                ViewBag.LoaiThongBaoList = new SelectList(loaiList);
                return View(model);
            }

            try
            {
                model.MaND = 1;
                model.NgayDang = DateTime.Now;

                // --- Xử lý file ---
                if (file != null && file.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    string originalFileName = Path.GetFileNameWithoutExtension(file.FileName);
                    string extension = Path.GetExtension(file.FileName);
                    string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                    string safeFileName = $"{originalFileName}_{timestamp}{extension}";

                    foreach (var c in Path.GetInvalidFileNameChars())
                        safeFileName = safeFileName.Replace(c, '_');

                    string filePath = Path.Combine(uploadsFolder, safeFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    model.TepDinhKem = safeFileName;
                }

                _context.ThongBaos.Add(model);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Thêm thông báo thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi lưu thông báo: " + ex.Message);
                var loaiList = _context.ThongBaos
                    .Select(t => t.LoaiThongBao)
                    .Where(l => !string.IsNullOrEmpty(l))
                    .Distinct()
                    .ToList();
                ViewBag.LoaiThongBaoList = new SelectList(loaiList);
                return View(model);
            }
        }

        // ------------------ DELETE ------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var tb = await _context.ThongBaos.FindAsync(id);
            if (tb == null) return NotFound();

            if (!string.IsNullOrEmpty(tb.TepDinhKem))
            {
                string filePath = Path.Combine(_env.WebRootPath, "uploads", tb.TepDinhKem);
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }

            _context.ThongBaos.Remove(tb);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa thông báo thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ------------------ EDIT ------------------
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var tb = await _context.ThongBaos.FindAsync(id);
            if (tb == null)
                return NotFound();

            var loaiList = _context.ThongBaos
                .Select(t => t.LoaiThongBao)
                .Where(l => !string.IsNullOrEmpty(l))
                .Distinct()
                .ToList();

            ViewBag.LoaiThongBaoList = new SelectList(loaiList, tb.LoaiThongBao);
            return View(tb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ThongBao model, string RemoveFile, IFormFile file)
        {
            var tb = await _context.ThongBaos.FindAsync(model.MaTB);
            if (tb == null) return NotFound();

            tb.TieuDe = model.TieuDe;
            tb.LoaiThongBao = model.LoaiThongBao;
            tb.NoiDung = model.NoiDung;

            // Xóa tệp nếu người dùng đánh dấu RemoveFile
            if (RemoveFile == "true")
            {
                if (!string.IsNullOrEmpty(tb.TepDinhKem))
                {
                    var path = Path.Combine(_env.WebRootPath, "uploads", tb.TepDinhKem);
                    if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                    tb.TepDinhKem = null;
                }
            }

            // Upload tệp mới nếu có
            if (file != null && file.Length > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                var path = Path.Combine(_env.WebRootPath, "uploads", fileName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                tb.TepDinhKem = fileName;
            }

            _context.Update(tb);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật thành công!";
            return RedirectToAction(nameof(Index));
        }


    }
}
