using aznews.Areas.Admin.Models;
using aznews.Areas.Admin.Models.Diem;
using aznews.Areas.GiangVien.Models.Diem;
using aznews.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace aznews.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    public class DiemController : Controller
    {
        private readonly DataContext _db;
        public DiemController(DataContext db) => _db = db;
        private static decimal? TryParseDecimalOrNull(string? s)
        {
            s = (s ?? "").Trim();
            if (string.IsNullOrEmpty(s)) return null;

            // chấp nhận cả dấu chấm và dấu phẩy
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
                return v;

            if (decimal.TryParse(s, NumberStyles.Any, new CultureInfo("vi-VN"), out v))
                return v;

            return null;
        }

        private async Task AddAuditAsync(
        int maLHP, int maSV,
        decimal? oldQT, decimal? newQT,
        decimal? oldThi, decimal? newThi,
        string by, string actionType)
        {
            _db.DiemLopAudits.Add(new DiemLopAudit
            {
                MaLHP = maLHP,
                MaSinhVien = maSV,
                OldQT = oldQT,
                NewQT = newQT,
                OldThi = oldThi,
                NewThi = newThi,
                ChangedBy = by,
                ChangedAt = DateTime.Now,
                ActionType = actionType
            });
            // chưa SaveChanges ở đây để gộp transaction với thao tác điểm
        }
        // Lấy MaGiangVien hiện tại từ Session
        private async Task<int?> GetCurrentGiangVienIdAsync()
        {
            // Ưu tiên session lưu sẵn MaGiangVien
            var gvIdStr = HttpContext.Session.GetString("MaGiangVien");
            if (int.TryParse(gvIdStr, out var gvId)) return gvId;

            // Fallback: nếu hệ thống lưu "UserName" = MaSoGV
            var username = HttpContext.Session.GetString("UserName");
            if (!string.IsNullOrWhiteSpace(username))
            {
                var gv = await _db.GiangViens.AsNoTracking()
                            .FirstOrDefaultAsync(x => x.MaSoGV == username);
                if (gv != null) return gv.MaGiangVien;
            }
            return null;
        }

        // ====== 1) Danh sách lớp học phần mà GV phụ trách ======
        // /GiangVien/Diem?hk=&nam=&q=&page=1
        public async Task<IActionResult> Index(string? hk, string? nam, string? q, int page = 1)
        {
            const int pageSize = 10;
            var gvId = await GetCurrentGiangVienIdAsync();
            if (gvId == null) return Unauthorized();

            var query = _db.LopHocPhans
                .AsNoTracking()
                .Include(x => x.HocPhan)
                .Where(x => x.MaGiangVien == gvId.Value);

            if (!string.IsNullOrWhiteSpace(hk)) query = query.Where(x => x.HocKy == hk);
            if (!string.IsNullOrWhiteSpace(nam)) query = query.Where(x => x.NamHoc == nam);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var k = q.Trim().ToLower();
                query = query.Where(x =>
                    (x.HocPhan != null && (x.HocPhan.TenHP ?? "").ToLower().Contains(k)) ||
                    (x.TenNhom ?? "").ToLower().Contains(k) ||
                    x.MaLHP.ToString().Contains(k) ||
                    (x.MaHP.ToString().ToLower().Contains(k)));
            }

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
                .Select(x => new LhpRowVM
                {
                    MaLHP = x.MaLHP,
                    TenHP = x.HocPhan.TenHP ?? "",
                    HocKy = x.HocKy,
                    NamHoc = x.NamHoc,
                    LoaiLop = (int?)x.LoaiLop,   // nếu cột là int (1/2/3)
                    TenNhom = x.TenNhom,         // có thể là "1" hoặc "LT1"
                    RouteText = "",                 // nếu không dùng có thể bỏ field này
                    TrangThai = x.DiemStatus
                })
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.HK = hk ?? "";
            ViewBag.NAM = nam ?? "";
            ViewBag.Q = q ?? "";
            return View(list); // bạn có thể làm view theo UI giảng viên của bạn
        }

        // ====== 2) Màn hình quản lý nhập điểm 1 lớp học phần ======
        // /GiangVien/Diem/Manage/5
        [HttpGet]
        public async Task<IActionResult> Manage(int id)
        {
            var gvId = await GetCurrentGiangVienIdAsync();
            if (gvId == null) return Unauthorized();

            var lhp = await _db.LopHocPhans
                .Include(x => x.HocPhan)
                .FirstOrDefaultAsync(x => x.MaLHP == id && x.MaGiangVien == gvId.Value);
            if (lhp == null) return NotFound();

            var rows = await (from dkl in _db.DangKyLops
                              join sv in _db.SinhViens on dkl.MaSinhVien equals sv.MaSinhVien
                              join d in _db.DiemLops
                                    .Where(t => t.MaLHP == id)
                                    on new { dkl.MaLHP, dkl.MaSinhVien } equals new { d.MaLHP, d.MaSinhVien }
                                    into gj
                              from d in gj.DefaultIfEmpty()
                              where dkl.MaLHP == id
                              orderby sv.HoTen
                              select new DiemRowVM
                              {
                                  MaSinhVien = sv.MaSinhVien,
                                  MaSoSV = sv.MaSoSV,
                                  HoTen = sv.HoTen,
                                  DiemQT = d != null ? d.DiemQT : null,
                                  DiemThi = d != null ? d.DiemThi : null
                              }).ToListAsync();

            var vm = new DiemManageVM
            {
                MaLHP = lhp.MaLHP,
                TenHP = lhp.HocPhan?.TenHP ?? "",
                HocKy = lhp.HocKy,
                NamHoc = lhp.NamHoc,
                TrangThai = (Areas.Admin.Models.Diem.DiemTrangThai)((byte)lhp.DiemStatus),
                Rows = rows
            };

            return View(vm); // làm view bảng nhập (2 cột QT/Thi) + nút Lưu + Import + Gửi duyệt
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(int maLHP, List<DiemRowVM> rows)
        {
            var gvId = await GetCurrentGiangVienIdAsync();
            if (gvId == null) return Unauthorized();

            // Lấy thông tin GV để ghi audit "by"
            var gv = await _db.GiangViens.FindAsync(gvId.Value);
            var by = gv?.HoTen ?? $"GV#{gvId.Value}";
            var now = DateTime.Now;

            var lhp = await _db.LopHocPhans
                .FirstOrDefaultAsync(x => x.MaLHP == maLHP && x.MaGiangVien == gvId.Value);
            if (lhp == null) return NotFound();

            if (lhp.DiemStatus != DiemStatus.Editable)
            {
                TempData["Error"] = "Lớp đã gửi duyệt hoặc đã chốt. Không thể sửa.";
                return RedirectToAction(nameof(Manage), new { id = maLHP });
            }

            foreach (var r in rows ?? new List<DiemRowVM>())
            {
                if (r.DiemQT.HasValue && (r.DiemQT < 0 || r.DiemQT > 10)) continue;
                if (r.DiemThi.HasValue && (r.DiemThi < 0 || r.DiemThi > 10)) continue;

                var diem = await _db.DiemLops
                    .FirstOrDefaultAsync(x => x.MaLHP == maLHP && x.MaSinhVien == r.MaSinhVien);

                if (diem == null)
                {
                    _db.DiemLops.Add(new Areas.Admin.Models.DiemLop
                    {
                        MaLHP = maLHP,
                        MaSinhVien = r.MaSinhVien,
                        DiemQT = r.DiemQT,
                        DiemThi = r.DiemThi,
                        UpdatedAt = now,            
                        UpdatedBy = by,
                        IsActive = true
                    });
                }
                else
                {
                    diem.DiemQT = r.DiemQT;
                    diem.DiemThi = r.DiemThi;
                    diem.UpdatedAt = now;           
                    diem.UpdatedBy = by;
                    _db.DiemLops.Update(diem);
                }
            }

            await _db.SaveChangesAsync();

            // Ghi audit (CHỈ định nghĩa 1 lần biến 'by', KHÔNG khai báo lại 'user')
            await SnapshotAuditAsync(maLHP, "Edit", by);

            TempData["Success"] = "Đã lưu điểm.";
            return RedirectToAction(nameof(Manage), new { id = maLHP });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportCsv(int maLHP, IFormFile? file)
        {
            var gvId = await GetCurrentGiangVienIdAsync();
            if (gvId == null) return Unauthorized();

            var gv = await _db.GiangViens.FindAsync(gvId.Value);
            var by = gv?.HoTen ?? $"GV#{gvId.Value}";
            var now = DateTime.Now;
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn tệp CSV.";
                return RedirectToAction(nameof(Manage), new { id = maLHP });
            }

            var lhp = await _db.LopHocPhans
                .FirstOrDefaultAsync(x => x.MaLHP == maLHP && x.MaGiangVien == gvId.Value);
            if (lhp == null) return NotFound();

            if (lhp.DiemStatus != DiemStatus.Editable)
            {
                TempData["Error"] = "Lớp đã gửi duyệt hoặc đã chốt. Không thể import.";
                return RedirectToAction(nameof(Manage), new { id = maLHP });
            }

            int imported = 0;
            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream, Encoding.UTF8, true);

            string? line;
            bool headerSkipped = false;

            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (!headerSkipped) { headerSkipped = true; continue; }
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(',');
                if (parts.Length < 3) continue;

                // ---- Cột 1: MaSinhVien (số) hoặc MaSoSV (chuỗi) ----
                int maSV;
                var cell = parts[0].Trim();

                if (!int.TryParse(cell, out maSV))
                {
                    // Không phải số -> coi là MaSoSV, tra lấy MaSinhVien
                    var sv = await _db.SinhViens
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.MaSoSV == cell);
                    if (sv == null) continue;            // không có SV này -> bỏ qua dòng
                    maSV = sv.MaSinhVien;
                }

                // ---- Cột điểm: chấp nhận "." hoặc ","; rỗng -> null; range 0..10 ----
                decimal? diemQt = TryParseDecimalOrNull(parts[1]);
                decimal? diemThi = TryParseDecimalOrNull(parts[2]);
                if (diemQt is < 0 or > 10) diemQt = null;
                if (diemThi is < 0 or > 10) diemThi = null;

                var diem = await _db.DiemLops
                    .FirstOrDefaultAsync(x => x.MaLHP == maLHP && x.MaSinhVien == maSV);

                if (diem == null)
                {
                    _db.DiemLops.Add(new aznews.Areas.Admin.Models.DiemLop
                    {
                        MaLHP = maLHP,
                        MaSinhVien = maSV,
                        DiemQT = diemQt,
                        DiemThi = diemThi,
                        UpdatedAt = now,            // <-- thêm
                        UpdatedBy = by,
                        IsActive = true
                    });
                }
                else
                {
                    diem.DiemQT = diemQt;
                    diem.DiemThi = diemThi;
                    diem.UpdatedAt = now;           // <-- thêm
                    diem.UpdatedBy = by;
                    _db.DiemLops.Update(diem);
                }

                imported++;
            }

            await _db.SaveChangesAsync();
            await SnapshotAuditAsync(maLHP, "Import", by);  
            TempData["Success"] = $"Import thành công {imported} dòng.";
            return RedirectToAction(nameof(Manage), new { id = maLHP });
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int id, string? note)
        {
            var gvId = await GetCurrentGiangVienIdAsync();
            if (gvId == null) return Unauthorized();

            var lhp = await _db.LopHocPhans
                .FirstOrDefaultAsync(x => x.MaLHP == id && x.MaGiangVien == gvId.Value);
            if (lhp == null) return NotFound();

            if (lhp.DiemStatus != DiemStatus.Editable)
            {
                TempData["Error"] = "Chỉ có thể gửi khi lớp đang ở trạng thái 'Có thể sửa'.";
                return RedirectToAction(nameof(Index));
            }

            var gv = await _db.GiangViens.FindAsync(gvId.Value);
            var by = gv?.HoTen ?? $"GV#{gvId.Value}";

            lhp.DiemStatus = DiemStatus.Submitted;
            lhp.DiemNote = (note ?? "").Trim();
            lhp.SubmittedAt = DateTime.Now;
            lhp.SubmittedBy = by;

            _db.Update(lhp);
            await _db.SaveChangesAsync();

            await SnapshotAuditAsync(id, "Submit", by, lhp.DiemNote);

            TempData["Success"] = "Đã gửi đề nghị chốt điểm.";
            return RedirectToAction(nameof(Index));
        }


        // GET: /GiangVien/Diem/DownloadTemplate/5
        [HttpGet]
        public async Task<IActionResult> DownloadTemplate(int id) // id = MaLHP
        {
            // Lấy danh sách sinh viên trong lớp học phần theo MaSoSV
            var rows = await (from d in _db.DiemLops
                              join s in _db.SinhViens on d.MaSinhVien equals s.MaSinhVien
                              where d.MaLHP == id
                              orderby s.MaSoSV
                              select new { s.MaSoSV })
                             .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("MaSoSV,DiemQT,DiemThi"); // header

            if (rows.Count > 0)
            {
                foreach (var r in rows)
                {
                    // Để trống 2 cột điểm để GV điền
                    sb.AppendLine($"{r.MaSoSV},,");
                }
            }
            else
            {
                // Nếu chưa có dữ liệu sinh viên cho lớp, vẫn trả vài dòng mẫu
                sb.AppendLine("SV001,,");
                sb.AppendLine("SV002,,");
                sb.AppendLine("SV003,,");
            }

            var csv = sb.ToString();

            // Xuất CSV kèm BOM để Excel nhận đúng UTF-8
            var preamble = Encoding.UTF8.GetPreamble();
            var body = Encoding.UTF8.GetBytes(csv);
            var bytes = new byte[preamble.Length + body.Length];
            Buffer.BlockCopy(preamble, 0, bytes, 0, preamble.Length);
            Buffer.BlockCopy(body, 0, bytes, preamble.Length, body.Length);

            var fileName = $"template_diem_lhp_{id}.csv";
            return File(bytes, "text/csv; charset=utf-8", fileName);
        }
        private async Task SnapshotAuditAsync(int maLHP, string action, string by, string? reason = null)
        {
            var rows = await _db.DiemLops
                .Where(x => x.MaLHP == maLHP)
                .Select(x => new DiemLopAudit
                {
                    MaLHP = x.MaLHP,
                    MaSinhVien = x.MaSinhVien,
                    // nếu sau này bạn muốn lưu "trước/sau", ở đây có thể tra OldQT/OldThi từ bảng audit cuối cùng.
                    OldQT = null,
                    OldThi = null,
                    NewQT = x.DiemQT,
                    NewThi = x.DiemThi,
                    ActionType = action,          // "Edit" | "Import" | "Submit"
                    Reason = reason,
                    ChangedBy = by,
                    ChangedAt = DateTime.Now
                })
                .ToListAsync();

            if (rows.Count > 0)
            {
                _db.DiemLopAudits.AddRange(rows);
                await _db.SaveChangesAsync();
            }
        }

    }
}
