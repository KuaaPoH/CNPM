using aznews.Areas.GiangVien.Models.Home;
using aznews.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using aznews.Areas.Admin.Models;

namespace aznews.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    public class HomeController : Controller
    {
        private readonly DataContext _db;
        public HomeController(DataContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Lấy ID Giảng viên từ Session
            // Lưu ý: Key session phải khớp với lúc Login.
            // Giả sử Login set "MaGiangVien" (int) hoặc tìm theo Username
            var maGvObj = HttpContext.Session.GetInt32("MaGiangVien");
            
            // Demo fallback: Nếu chưa login đúng vai trò, lấy GV đầu tiên để test giao diện
            int maGV = maGvObj ?? await _db.GiangViens.Select(x => x.MaGiangVien).FirstOrDefaultAsync();

            var gv = await _db.GiangViens.FindAsync(maGV);
            if (gv == null) return Content("Không tìm thấy thông tin giảng viên.");

            // 2. Query dữ liệu của GV này
            // Lấy các lớp trong năm học/kỳ học mới nhất (hoặc lấy tất cả các lớp ĐANG MỞ)
            // Ở đây lấy tất cả các lớp mà GV này được phân công
            var lops = await _db.LopHocPhans
                .AsNoTracking()
                .Include(x => x.HocPhan)
                .Where(x => x.MaGiangVien == maGV)
                .OrderByDescending(x => x.NamHoc)
                .ThenByDescending(x => x.HocKy)
                .ToListAsync();

            // Tính toán KPI
            var vm = new GiangVienDashboardVM
            {
                HoTenGV = gv.HoTen,
                MaSoGV = gv.MaSoGV,
                LopDangDay = lops.Count,
                // Tổng SV: Cần query bảng DangKyLop hoặc DiemLop để đếm. Tạm thời để 0 hoặc đếm sau
                TongSinhVien = 0, 
                LopChuaNhapDiem = lops.Count(x => x.DiemStatus != DiemStatus.Approved),

                CountEditable = lops.Count(x => x.DiemStatus == DiemStatus.Editable),
                CountSubmitted = lops.Count(x => x.DiemStatus == DiemStatus.Submitted),
                CountApproved = lops.Count(x => x.DiemStatus == DiemStatus.Approved),

                LopPhuTrach = lops.Take(5).ToList() // Lấy 5 lớp gần nhất hiển thị
            };

            // Nếu muốn chính xác số SV, cần count từ bảng DiemLop/DangKy
            // var lopIds = lops.Select(x => x.MaLHP).ToList();
            // vm.TongSinhVien = await _db.DiemLops.CountAsync(x => lopIds.Contains(x.MaLHP));

            return View(vm);
        }
    }
}