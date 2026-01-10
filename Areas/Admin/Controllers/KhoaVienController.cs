using System;
using System.IO;
using System.Linq;
using System.Text;
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

        // --- EXPORT CSV (Không cần thư viện) ---
        public async Task<IActionResult> Export()
        {
            var list = await _context.KhoaViens.AsNoTracking().ToListAsync();
            var sb = new StringBuilder();

            // 1. Header
            sb.AppendLine("TenKhoaVien,DiaChi,DienThoai,Email");

            // 2. Data
            foreach (var item in list)
            {
                // Xử lý dấu phẩy trong nội dung (nếu có) để không bị lỗi cột
                // Nếu chuỗi chứa dấu phẩy hoặc dấu ngoặc kép, cần bao quanh bởi dấu ngoặc kép
                string Escape(string? s) 
                {
                    if (string.IsNullOrEmpty(s)) return "";
                    if (s.Contains(",") || s.Contains("\"") || s.Contains("\n"))
                    {
                        return "\"" + s.Replace("\"", "\"\"") + "\"";
                    }
                    return s;
                }

                sb.AppendLine($"{Escape(item.TenKhoaVien)},{Escape(item.DiaChi)},{Escape(item.DienThoai)},{Escape(item.Email)}");
            }

            // 3. Return File với Encoding UTF-8 có BOM (để Excel hiển thị đúng tiếng Việt)
            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return File(bytes, "text/csv", $"DS_KhoaVien_{DateTime.Now:yyyyMMdd}.csv");
        }

        // --- IMPORT CSV ---
        [HttpPost]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn file .csv";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                int count = 0;
                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    // Bỏ qua dòng tiêu đề (Header)
                    if (!reader.EndOfStream) await reader.ReadLineAsync();

                    while (!reader.EndOfStream)
                    {
                        var line = await reader.ReadLineAsync();
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        // Tách chuỗi cơ bản (Lưu ý: Cách này đơn giản, nếu nội dung có dấu phẩy phức tạp cần regex)
                        // Giả định file CSV chuẩn đơn giản: Ten,DiaChi,SDT,Email
                        var values = line.Split(',');

                        if (values.Length >= 1)
                        {
                            // Loại bỏ dấu ngoặc kép bao quanh nếu có
                            string Clean(string s) => s.Trim().Trim('"').Replace("\"\"", "\"");

                            string ten = Clean(values[0]);
                            string diaChi = values.Length > 1 ? Clean(values[1]) : "";
                            string sdt = values.Length > 2 ? Clean(values[2]) : "";
                            string email = values.Length > 3 ? Clean(values[3]) : "";

                            if (string.IsNullOrWhiteSpace(ten)) continue;

                            // Check trùng
                            if (!await _context.KhoaViens.AnyAsync(x => x.TenKhoaVien == ten))
                            {
                                _context.KhoaViens.Add(new KhoaVien 
                                {
                                    TenKhoaVien = ten,
                                    DiaChi = diaChi,
                                    DienThoai = sdt,
                                    Email = email
                                });
                                count++;
                            }
                        }
                    }
                }

                if (count > 0)
                {
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Đã nhập thành công {count} dòng.";
                }
                else
                {
                    TempData["Warning"] = "Không có dữ liệu mới.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi đọc file: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
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

            int total = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(total / (double)pageSize);
            if (totalPages == 0) totalPages = 1;
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var tblist = await query
                .OrderBy(x => x.MaKhoaVien)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;
            ViewBag.Total = total;
            ViewData["SearchString"] = searchString; 

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
