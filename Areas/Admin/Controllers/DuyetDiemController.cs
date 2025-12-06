using aznews.Areas.Admin.Models;
using aznews.Areas.Admin.Models.DuyetDiem;
using aznews.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using aznews.Areas.Admin.Models.Diem;

namespace aznews.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DuyetDiemController : Controller
    {
        private readonly DataContext _db;
        public DuyetDiemController(DataContext db) => _db = db;

        // ========== Danh sách lớp ==========
        // q: tìm theo tên HP / mã HP / GV, status: lọc nếu được chọn
        // GET: /Admin/DuyetDiem
        public async Task<IActionResult> Index(string? q, DiemStatus? status, int page = 1)
        {
            const int pageSize = 10;

            var query = _db.LopHocPhans
                .AsNoTracking()
                .Include(x => x.HocPhan)
                .Include(x => x.GiangVien)
                // chỉ hiện LỚP CHỜ DUYỆT hoặc ĐÃ DUYỆT
                .Where(x => x.DiemStatus == DiemStatus.Submitted
                         || x.DiemStatus == DiemStatus.Approved)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var k = q.Trim().ToLower();
                query = query.Where(x =>
                    (x.HocPhan.TenHP ?? "").ToLower().Contains(k) ||
                    (x.HocPhan.MaSoHP ?? "").ToLower().Contains(k) ||
                    (x.GiangVien.HoTen ?? "").ToLower().Contains(k));
            }

            // nếu có truyền status thì lọc đúng status (vẫn nằm trong 2 nhóm trên)
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
                .Select(x => new DuyetRowVM
                {
                    MaLHP = x.MaLHP,
                    TenHP = x.HocPhan.TenHP!,
                    MaSoHP = x.HocPhan.MaSoHP!,
                    GV = x.GiangVien.HoTen!,
                    HocKy = x.HocKy!,
                    NamHoc = x.NamHoc!,
                    SoSV = _db.DiemLops.Count(d => d.MaLHP == x.MaLHP),
                    TrangThai = x.DiemStatus,
                    SubmittedAt = x.SubmittedAt,
                    SubmittedBy = x.SubmittedBy,
                    ApprovedAt = x.ApprovedAt,
                    ApprovedBy = x.ApprovedBy,
                    TenNhom = x.TenNhom
                })
                .ToListAsync();

            ViewBag.Q = q ?? "";
            ViewBag.Status = status;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;

            return View(list);
        }


        // GET: /Admin/DuyetDiem/Review/5
        public async Task<IActionResult> Review(int id)
        {
            var lhp = await _db.LopHocPhans
                .AsNoTracking()
                .Include(x => x.HocPhan)
                .Include(x => x.GiangVien)
                .FirstOrDefaultAsync(x => x.MaLHP == id);

            if (lhp == null) return NotFound();
            var vm = new Areas.Admin.Models.Diem.DiemManageVM
            {
                MaLHP = lhp.MaLHP,
                TenHP = lhp.HocPhan?.TenHP ?? "",
                HocKy = lhp.HocKy ?? "",
                NamHoc = lhp.NamHoc ?? "",

                TrangThai = (aznews.Areas.Admin.Models.Diem.DiemTrangThai)lhp.DiemStatus,

                Rows = await _db.DiemLops
                    .AsNoTracking()
                    .Where(d => d.MaLHP == id)
                    .Join(_db.SinhViens.AsNoTracking(),
                          d => d.MaSinhVien,
                          sv => sv.MaSinhVien,
                          (d, sv) => new aznews.Areas.Admin.Models.Diem.DiemRowVM
                          {
                              MaSinhVien = sv.MaSinhVien,
                              MaSoSV = sv.MaSoSV ?? "",
                              HoTen = sv.HoTen ?? "",
                              DiemQT = d.DiemQT,
                              DiemThi = d.DiemThi
                          })
                    .OrderBy(r => r.MaSoSV)
                    .ToListAsync()
            };


            return View("Review", vm);
        }

        // ========== Duyệt ==========
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var lhp = await _db.LopHocPhans.FirstOrDefaultAsync(x => x.MaLHP == id);
            if (lhp == null) return NotFound();
            if (lhp.DiemStatus != DiemStatus.Submitted)
            {
                TempData["Error"] = "Chỉ duyệt lớp đang ở trạng thái ‘Đã gửi đề nghị chốt’.";
                return RedirectToAction(nameof(Review), new { id });
            }

            await SnapshotAuditAsync(id, "APPROVE", User?.Identity?.Name ?? "ADMIN");

            lhp.DiemStatus = DiemStatus.Approved;
            lhp.ApprovedAt = DateTime.Now;
            lhp.ApprovedBy = User?.Identity?.Name ?? "ADMIN";
            lhp.DiemNote = null;

            _db.Update(lhp);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Đã phê duyệt và chốt điểm.";
            return RedirectToAction(nameof(Review), new { id });
        }

        // ========== Từ chối ==========
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string? note)
        {
            var lhp = await _db.LopHocPhans.FirstOrDefaultAsync(x => x.MaLHP == id);
            if (lhp == null) return NotFound();
            if (lhp.DiemStatus != DiemStatus.Submitted)
            {
                TempData["Error"] = "Chỉ từ chối lớp đang ở trạng thái ‘Đã gửi đề nghị chốt’.";
                return RedirectToAction(nameof(Review), new { id });
            }

            await SnapshotAuditAsync(id, "REJECT", User?.Identity?.Name ?? "ADMIN", (note ?? "").Trim());

            lhp.DiemStatus = DiemStatus.Editable;   // trả về GV sửa
            lhp.DiemNote = (note ?? "").Trim();
            lhp.SubmittedAt = null;
            lhp.SubmittedBy = null;

            _db.Update(lhp);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Đã trả về cho giảng viên sửa.";
            return RedirectToAction(nameof(Review), new { id });
        }

        // ========== Ghi log snapshot (tuỳ chọn) ==========
        private async Task SnapshotAuditAsync(int maLHP, string action, string by, string? reason = null)
        {
            var rows = await _db.DiemLops
                .Where(x => x.MaLHP == maLHP)
                .Select(x => new DiemLopAudit
                {
                    MaLHP = x.MaLHP,
                    MaSinhVien = x.MaSinhVien,
                    OldQT = null,
                    OldThi = null,
                    NewQT = x.DiemQT,
                    NewThi = x.DiemThi,
                    ActionType = action,
                    ChangedBy = by,
                    ChangedAt = DateTime.Now,
                    // Nếu bảng audit có cột Reason thì để dòng dưới; không có thì bỏ
                    Reason = reason
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
