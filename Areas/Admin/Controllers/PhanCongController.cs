using aznews.Areas.Admin.Models.PhanCong;
using aznews.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace aznews.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PhanCongController : Controller
    {
        private readonly DataContext _db;

        public PhanCongController(DataContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? hk, string? nam, string? q)
        {
            // 1. Load danh sách năm học & học kỳ có trong DB để làm bộ lọc
            var namHocs = await _db.LopHocPhans.Select(x => x.NamHoc).Distinct().OrderByDescending(x => x).ToListAsync();
            var hocKys = await _db.LopHocPhans.Select(x => x.HocKy).Distinct().OrderBy(x => x).ToListAsync();

            // Default filters nếu null
            if (string.IsNullOrEmpty(nam) && namHocs.Any()) nam = namHocs.First();
            if (string.IsNullOrEmpty(hk) && hocKys.Any()) hk = hocKys.First();

            // 2. Query lớp học phần
            var query = _db.LopHocPhans
                .AsNoTracking()
                .Include(x => x.HocPhan)
                .Include(x => x.GiangVien)
                .Where(x => x.NamHoc == nam && x.HocKy == hk);

            if (!string.IsNullOrEmpty(q))
            {
                string k = q.Trim().ToLower();
                query = query.Where(x => 
                    x.HocPhan.TenHP.ToLower().Contains(k) || 
                    x.HocPhan.MaSoHP.ToLower().Contains(k) ||
                    x.MaLHP.ToString().Contains(k)
                );
            }

            var classes = await query
                .OrderBy(x => x.HocPhan.TenHP)
                .ThenBy(x => x.TenNhom)
                .Select(x => new PhanCongRowVM
                {
                    MaLHP = x.MaLHP,
                    MaSoHP = x.HocPhan.MaSoHP ?? "",
                    TenHP = x.HocPhan.TenHP ?? "",
                    TenNhom = x.TenNhom ?? "",
                    MaGV = x.MaGiangVien, // Int, có thể là ID dummy hoặc chuẩn
                    TenGV = x.GiangVien.HoTen,
                    SoSV = 0 // Tạm thời chưa count sinh viên để tối ưu query
                })
                .ToListAsync();

            // 3. Load tất cả giảng viên để fill dropdown
            var gvs = await _db.GiangViens
                .AsNoTracking()
                .Where(x => x.TrangThai == true) // Chỉ lấy GV đang hoạt động
                .OrderBy(x => x.HoTen) // Giả sử có TenStr hoặc sort theo HoTen
                .Select(x => new SimpleGiangVienVM
                {
                    MaGV = x.MaGiangVien,
                    MaSoGV = x.MaSoGV,
                    TenGV = x.HoTen
                })
                .ToListAsync();

            var vm = new PhanCongIndexVM
            {
                Classes = classes,
                AllGiangViens = gvs,
                SelectedHocKy = hk,
                SelectedNamHoc = nam
            };

            ViewBag.NamHocs = namHocs;
            ViewBag.HocKys = hocKys;
            ViewBag.Q = q;

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] UpdateAssignReq req)
        {
            if (req == null) return BadRequest("Invalid Data");

            var lop = await _db.LopHocPhans.FindAsync(req.MaLHP);
            if (lop == null) return NotFound("Không tìm thấy lớp học phần");

            // Kiểm tra GV có tồn tại không
            bool gvExists = await _db.GiangViens.AnyAsync(x => x.MaGiangVien == req.MaGV);
            if (!gvExists) return BadRequest("Giảng viên không tồn tại");

            // Update
            lop.MaGiangVien = req.MaGV;
            await _db.SaveChangesAsync();

            return Ok(new { success = true, message = "Cập nhật thành công" });
        }

        public class UpdateAssignReq
        {
            public int MaLHP { get; set; }
            public int MaGV { get; set; }
        }
    }
}
