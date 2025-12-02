using aznews.Areas.Admin.Models;
using aznews.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace aznews.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DuyetDiemController : Controller
    {
        private readonly DataContext _db;
        public DuyetDiemController(DataContext db) => _db = db;

        // ========== Danh sách lớp gửi duyệt ==========
        // q: tìm theo tên HP / mã HP / GV
        public async Task<IActionResult> Index(string? q, DiemStatus? status, int page = 1)
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
                    (x.HocPhan.TenHP ?? "").ToLower().Contains(k) ||
                    (x.HocPhan.MaSoHP ?? "").ToLower().Contains(k) ||
                    (x.GiangVien.HoTen ?? "").ToLower().Contains(k));
            }

            if (status.HasValue)
                query = query.Where(x => x.DiemStatus == status.Value);

            var total = await query.CountAsync();
            var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
            page = Math.Clamp(page, 1, totalPages);

            var list = await query
                .OrderByDescending(x => x.SubmittedAt) // lớp mới gửi lên trước
                .ThenBy(x => x.MaHP)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    x.MaLHP,
                    TenHP = x.HocPhan.TenHP,
                    MaSoHP = x.HocPhan.MaSoHP,
                    GV = x.GiangVien.HoTen,
                    x.HocKy,
                    x.NamHoc,
                    x.DiemStatus,
                    x.SubmittedAt,
                    x.SubmittedBy,
                    x.ApprovedAt,
                    x.ApprovedBy,
                    x.DiemNote
                })
                .ToListAsync();

            ViewBag.Q = q ?? "";
            ViewBag.Status = status;
            ViewBag.Page = page; ViewBag.TotalPages = totalPages; ViewBag.PageSize = pageSize;

            return View(list);
        }

        // ========== Xem chi tiết lớp (điểm SV) ==========
        public async Task<IActionResult> Review(int id)
        {
            var lhp = await _db.LopHocPhans
                .Include(x => x.HocPhan)
                .Include(x => x.GiangVien)
                .FirstOrDefaultAsync(x => x.MaLHP == id);

            if (lhp == null) return NotFound();

            var rows = await (from sv in _db.SinhViens
                              join d in _db.DiemLops.Where(t => t.MaLHP == id)
                                  on sv.MaSinhVien equals d.MaSinhVien into gj
                              from d in gj.DefaultIfEmpty()
                              orderby sv.MaSinhVien
                              select new
                              {
                                  sv.MaSinhVien,
                                  sv.MaSoSV,
                                  sv.HoTen,
                                  DiemQT = d.DiemQT,
                                  DiemThi = d.DiemThi,
                                  DiemTong = (d.DiemQT.HasValue || d.DiemThi.HasValue)
                                         ? Math.Round(((d.DiemQT ?? 0m) + (d.DiemThi ?? 0m)) / 2m, 2)
                                         : (decimal?)null
                              }).ToListAsync();

            ViewBag.LHP = lhp;
            return View(rows);
        }

        // ========== Phê duyệt ==========
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var lhp = await _db.LopHocPhans.FirstOrDefaultAsync(x => x.MaLHP == id);
            if (lhp == null) return NotFound();

            if (lhp.DiemStatus != DiemStatus.Submitted)
            {
                TempData["Error"] = "Chỉ duyệt được lớp đang ở trạng thái 'Đã gửi đề nghị chốt'.";
                return RedirectToAction(nameof(Review), new { id });
            }

            // (Tuỳ chọn) Snapshot audit
            // await SnapshotAuditAsync(id, "APPROVE", User.Identity?.Name ?? "ADMIN", lhp.DiemNote);

            lhp.DiemStatus = DiemStatus.Approved;
            lhp.ApprovedAt = DateTime.Now;
            lhp.ApprovedBy = User?.Identity?.Name ?? "ADMIN";
            lhp.DiemNote = null;

            _db.Update(lhp);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Đã phê duyệt và chốt điểm.";
            return RedirectToAction(nameof(Review), new { id });
        }

        // ========== Từ chối (trả về cho GV sửa) ==========
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string? note)
        {
            var lhp = await _db.LopHocPhans.FirstOrDefaultAsync(x => x.MaLHP == id);
            if (lhp == null) return NotFound();

            if (lhp.DiemStatus != DiemStatus.Submitted)
            {
                TempData["Error"] = "Chỉ từ chối được lớp đang ở trạng thái 'Đã gửi đề nghị chốt'.";
                return RedirectToAction(nameof(Review), new { id });
            }

            // (Tuỳ chọn) Audit
            // await SnapshotAuditAsync(id, "REJECT", User.Identity?.Name ?? "ADMIN", note);

            // Trả về cho GV sửa
            lhp.DiemStatus = DiemStatus.Editable;   // cho phép sửa lại
            lhp.DiemNote = (note ?? "").Trim();
            lhp.SubmittedAt = null;                 // buộc gửi lại
            lhp.SubmittedBy = null;

            _db.Update(lhp);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Đã trả về cho giảng viên sửa.";
            return RedirectToAction(nameof(Review), new { id });
        }

        // ========== (Tuỳ chọn) Hàm snapshot audit ==========
        
        //private async Task SnapshotAuditAsync(int maLHP, string action, string by, string? note)
        //{
        //    var rows = await _db.DiemLops
        //        .Where(x => x.MaLHP == maLHP)
        //        .Select(x => new DiemLopAudit
        //        {
        //            MaLHP = x.MaLHP,
        //            MaSinhVien = x.MaSinhVien,
        //            DiemQT = x.DiemQT,
        //            DiemThi = x.DiemThi,
        //            DiemTong = (x.DiemQT.HasValue || x.DiemThi.HasValue)
        //                    ? Math.Round(((x.DiemQT ?? 0m) + (x.DiemThi ?? 0m)) / 2m, 2)
        //                    : (decimal?)null,
        //            Action = action,
        //            ActionBy = by,
        //            ActionAt = DateTime.Now,
        //            Note = note
        //        })
        //        .ToListAsync();

        //    if (rows.Count > 0)
        //    {
        //        _db.DiemLopAudits.AddRange(rows);
        //        await _db.SaveChangesAsync();
        //    }
        //}
        
    }
}
