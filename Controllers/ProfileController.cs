using aznews.Models;
using aznews.Areas.Admin.Models; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace aznews.Controllers
{
    public class ProfileController : Controller
    {
        private readonly DataContext _db;
        private readonly IWebHostEnvironment _env;

        public ProfileController(DataContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        private async Task<SinhVien?> GetCurrentStudent()
        {
            var maSV = HttpContext.Session.GetInt32("MaSV");
            if (!maSV.HasValue) return null;

            return await _db.SinhViens
                .Include(x => x.LopHanhChinh)
                .FirstOrDefaultAsync(x => x.MaSinhVien == maSV);
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var sv = await GetCurrentStudent();
            if (sv == null) return RedirectToAction("Login", "Account");

            var vm = new SinhVienProfileVM
            {
                MaSoSV = sv.MaSoSV,
                HoTen = sv.HoTen,
                NgaySinh = sv.NgaySinh,
                GioiTinh = sv.GioiTinh,
                Email = sv.Email,
                SoDienThoai = sv.SoDienThoai,
                DiaChi = sv.DiaChi,
                CurrentAvatar = sv.AnhDaiDien ?? "default-sv.jpg",
                LopHanhChinh = sv.LopHanhChinh?.TenLopHC ?? "Chưa phân lớp"
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateInfo(SinhVienProfileVM model)
        {
            var sv = await GetCurrentStudent();
            if (sv == null) return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(model.HoTen)) 
                ModelState.AddModelError("HoTen", "Họ tên không được trống");

            if (!ModelState.IsValid)
            {
                model.MaSoSV = sv.MaSoSV;
                model.LopHanhChinh = sv.LopHanhChinh?.TenLopHC ?? "";
                model.CurrentAvatar = sv.AnhDaiDien;
                return View("Index", model);
            }

            sv.HoTen = model.HoTen;
            sv.NgaySinh = model.NgaySinh;
            sv.GioiTinh = model.GioiTinh;
            sv.Email = model.Email;
            sv.SoDienThoai = model.SoDienThoai;
            sv.DiaChi = model.DiaChi;

            if (model.AvatarFile != null)
            {
                string ext = Path.GetExtension(model.AvatarFile.FileName);
                string fileName = $"SV_{sv.MaSoSV}_{DateTime.Now.Ticks}{ext}";
                string path = Path.Combine(_env.WebRootPath, "admin", "Imgs", fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await model.AvatarFile.CopyToAsync(stream);
                }

                sv.AnhDaiDien = fileName;
                HttpContext.Session.SetString("Avatar", fileName);
            }

            _db.Update(sv);
            await _db.SaveChangesAsync();

            HttpContext.Session.SetString("FullName", sv.HoTen);

            model.SuccessMessage = "Cập nhật hồ sơ thành công!";
            model.CurrentAvatar = sv.AnhDaiDien;
            model.MaSoSV = sv.MaSoSV;
            model.LopHanhChinh = sv.LopHanhChinh?.TenLopHC ?? "";

            return View("Index", model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePass(SinhVienProfileVM model)
        {
            var sv = await GetCurrentStudent();
            if (sv == null) return RedirectToAction("Login", "Account");

            if (string.IsNullOrEmpty(model.CurrentPass) || string.IsNullOrEmpty(model.NewPass))
            {
                model.ErrorMessage = "Vui lòng nhập đầy đủ thông tin mật khẩu.";
            }
            else
            {
                bool isCorrect = false;
                if (sv.MatKhau.StartsWith("$2")) 
                    isCorrect = BCrypt.Net.BCrypt.Verify(model.CurrentPass, sv.MatKhau);
                else 
                    isCorrect = (sv.MatKhau == model.CurrentPass);

                if (!isCorrect)
                {
                    model.ErrorMessage = "Mật khẩu hiện tại không đúng.";
                }
                else
                {
                    sv.MatKhau = BCrypt.Net.BCrypt.HashPassword(model.NewPass);
                    _db.Update(sv);
                    await _db.SaveChangesAsync();
                    model.SuccessMessage = "Đổi mật khẩu thành công!";
                }
            }

            model.HoTen = sv.HoTen;
            model.MaSoSV = sv.MaSoSV;
            model.CurrentAvatar = sv.AnhDaiDien;
            model.LopHanhChinh = sv.LopHanhChinh?.TenLopHC ?? "";
            
            return View("Index", model);
        }
    }
}
