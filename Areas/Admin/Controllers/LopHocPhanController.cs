using System.Text.RegularExpressions;
using aznews.Areas.Admin.Models;
using aznews.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace aznews.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LopHocPhanController : Controller
    {
        private readonly DataContext _db;
        public LopHocPhanController(DataContext db) => _db = db;

        // ================== INDEX ==================
        public async Task<IActionResult> Index(string? q, string? hk, string? nam, byte? loai, int page = 1)
        {
            const int pageSize = 10;

            var query = _db.LopHocPhans
                .AsNoTracking()
                .Include(x => x.HocPhan)
                .Include(x => x.GiangVien)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var k = q.Trim().ToLower();
                query = query.Where(x =>
                    (x.HocPhan != null &&
                        (((x.HocPhan.TenHP ?? "").ToLower().Contains(k)) ||
                         ((x.HocPhan.MaSoHP ?? "").ToLower().Contains(k)))) ||
                    (x.GiangVien != null &&
                        (((x.GiangVien.HoTen ?? "").ToLower().Contains(k)) ||
                         ((x.GiangVien.MaSoGV ?? "").ToLower().Contains(k)))) ||
                    ((x.TenNhom ?? "").ToLower().Contains(k)) ||
                    x.MaLHP.ToString().Contains(k) ||
                    x.MaHP.ToString().Contains(k)
                );
            }

            if (!string.IsNullOrWhiteSpace(hk)) query = query.Where(x => x.HocKy == hk);
            if (!string.IsNullOrWhiteSpace(nam)) query = query.Where(x => x.NamHoc == nam);
            if (loai.HasValue) query = query.Where(x => (byte)x.LoaiLop == loai.Value);

            var total = await query.CountAsync();
            var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
            page = Math.Clamp(page, 1, totalPages);

            var list = await query
                .OrderByDescending(x => x.NamHoc)
                .ThenBy(x => x.HocKy)
                .ThenBy(x => x.MaHP)
                .ThenBy(x => x.TenNhom)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            // Lấy filter từ DB — chạy hoàn toàn trên IQueryable
            ViewBag.HocKys = await _db.LopHocPhans
                .Select(x => x.HocKy)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            ViewBag.NamHocs = await _db.LopHocPhans
                .Select(x => x.NamHoc)
                .Distinct()
                .OrderByDescending(x => x)
                .ToListAsync();


            ViewBag.Page = page; ViewBag.TotalPages = totalPages; ViewBag.PageSize = pageSize;
            ViewBag.Q = q ?? ""; ViewBag.HK = hk ?? ""; ViewBag.NAM = nam ?? ""; ViewBag.LOAI = loai;

            return View(list);
        }

        // ================== CREATE ==================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await FillDropdowns();
            return View(new LopHocPhan
            {
                LoaiLop = LopLoai.LT,
                TenNhom = "LT1",
                TrangThai = true
            });
        }

        // Tạo N lớp theo Loại (LT/TH/DA) và auto TenNhom LT1..LTN (hoặc TH/DA)
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LopHocPhan model, int soLop = 1)
        {
            if (!ModelState.IsValid || soLop < 1)
            {
                if (soLop < 1) ModelState.AddModelError(nameof(soLop), "Số lớp phải ≥ 1.");
                await FillDropdowns();
                return View(model);
            }
            if (string.IsNullOrWhiteSpace(model.HocKy) || string.IsNullOrWhiteSpace(model.NamHoc))
            {
                ModelState.AddModelError("", "Học kỳ và Năm học là bắt buộc.");
                await FillDropdowns();
                return View(model);
            }

            string prefix = model.LoaiLop switch
            {
                LopLoai.LT => "LT",
                LopLoai.TH => "TH",
                LopLoai.DO_AN => "DA",
                _ => "N"
            };

            // Lấy số thứ tự hiện có cao nhất của nhóm cùng MaHP+HK+NH+Loai
            var existedNames = await _db.LopHocPhans.AsNoTracking()
                .Where(x => x.MaHP == model.MaHP
                            && x.HocKy == model.HocKy
                            && x.NamHoc == model.NamHoc
                            && x.LoaiLop == model.LoaiLop)
                .Select(x => x.TenNhom)
                .ToListAsync();

            int MaxIndex(IEnumerable<string?> names)
            {
                var re = new Regex(@"(\d+)$", RegexOptions.Compiled);
                int max = 0;
                foreach (var n in names.Where(s => !string.IsNullOrWhiteSpace(s) && s!.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
                {
                    var m = re.Match(n!);
                    if (m.Success && int.TryParse(m.Groups[1].Value, out int k) && k > max) max = k;
                }
                return max;
            }

            int start = MaxIndex(existedNames) + 1;

            var batch = new List<LopHocPhan>();
            for (int i = 0; i < soLop; i++)
            {
                var ten = $"{prefix}{start + i}";

                // tránh trùng tuyệt đối
                bool tr = await _db.LopHocPhans.AnyAsync(x =>
                    x.MaHP == model.MaHP &&
                    x.MaGiangVien == model.MaGiangVien &&
                    x.HocKy == model.HocKy &&
                    x.NamHoc == model.NamHoc &&
                    x.LoaiLop == model.LoaiLop &&
                    x.TenNhom == ten);

                if (!tr)
                {
                    batch.Add(new LopHocPhan
                    {
                        MaHP = model.MaHP,
                        MaGiangVien = model.MaGiangVien,
                        HocKy = model.HocKy,
                        NamHoc = model.NamHoc,
                        LoaiLop = model.LoaiLop,
                        TenNhom = ten,
                        TrangThai = model.TrangThai,
                        MaLopCha = model.MaLopCha
                    });
                }
            }

            if (batch.Count == 0)
            {
                ModelState.AddModelError("", "Không tạo được lớp vì trùng dữ liệu.");
                await FillDropdowns();
                return View(model);
            }

            _db.LopHocPhans.AddRange(batch);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Đã tạo {batch.Count} lớp {prefix}{start}…{prefix}{start + batch.Count - 1}.";
            return RedirectToAction(nameof(Index));
        }

        // ================== EDIT ==================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var lhp = await _db.LopHocPhans.FindAsync(id);
            if (lhp == null) return NotFound();
            await FillDropdowns();
            return View(lhp);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(LopHocPhan model)
        {
            if (!ModelState.IsValid)
            {
                await FillDropdowns();
                return View(model);
            }

            bool trung = await _db.LopHocPhans.AnyAsync(x =>
                x.MaLHP != model.MaLHP &&
                x.MaHP == model.MaHP &&
                x.MaGiangVien == model.MaGiangVien &&
                x.HocKy == model.HocKy &&
                x.NamHoc == model.NamHoc &&
                x.LoaiLop == model.LoaiLop &&
                x.TenNhom == model.TenNhom);

            if (trung)
            {
                ModelState.AddModelError("", "Đã tồn tại lớp trùng (Học phần, GV, Học kỳ, Năm học, Loại, Nhóm).");
                await FillDropdowns();
                return View(model);
            }

            _db.Update(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ================== DELETE ==================
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var lhp = await _db.LopHocPhans.FindAsync(id);
            if (lhp == null) return NotFound();
            _db.LopHocPhans.Remove(lhp);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ================== HELPERS ==================
        private static IEnumerable<string> BuildAcademicYears()
        {
            // Sinh danh sách năm học quanh năm hiện tại
            int y = DateTime.Now.Year;
            for (int i = y - 1; i <= y + 4; i++)
                yield return $"{i}-{i + 1}";
        }

        private async Task FillDropdowns()
        {
            var hps = await _db.HocPhans.AsNoTracking()
                .OrderBy(x => x.TenHP)
                .Select(x => new { x.MaHP, Ten = x.MaHP + " - " + x.TenHP })
                .ToListAsync();

            var gvs = await _db.GiangViens.AsNoTracking()
                .OrderBy(x => x.HoTen)
                .Select(x => new { x.MaGiangVien, Ten = x.MaSoGV + " - " + x.HoTen })
                .ToListAsync();

            var lopChas = await _db.LopHocPhans.AsNoTracking()
                .OrderByDescending(x => x.NamHoc).ThenBy(x => x.HocKy)
                .Select(x => new { x.MaLHP, Ten = $"{x.MaLHP}: {x.MaHP}-{x.HocKy}-{x.NamHoc} ({x.TenNhom})" })
                .ToListAsync();

            var namHocs = BuildAcademicYears()
                .Union(await _db.LopHocPhans.Select(x => x.NamHoc).Distinct().ToListAsync())
                .Distinct().OrderByDescending(x => x).ToList();

            ViewBag.HocPhanList = new SelectList(hps, "MaHP", "Ten");
            ViewBag.GiangVienList = new SelectList(gvs, "MaGiangVien", "Ten");
            ViewBag.LopChaList = new SelectList(lopChas, "MaLHP", "Ten");
            ViewBag.HocKyList = new SelectList(new[] { "HK1", "HK2", "Hè" });
            ViewBag.NamHocList = new SelectList(namHocs);

            ViewBag.LoaiList = new SelectList(new[]
            {
                new { Id = (byte)LopLoai.LT,    Ten = "Lý thuyết" },
                new { Id = (byte)LopLoai.TH,    Ten = "Thực hành" },
                new { Id = (byte)LopLoai.DO_AN, Ten = "Đồ án" }
            }, "Id", "Ten");
        }
    }
}
