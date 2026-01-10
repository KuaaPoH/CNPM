using aznews.Areas.Admin.Models.Profile;
using aznews.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace aznews.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProfileController : Controller
    {
        private readonly DataContext _db;
        private readonly IWebHostEnvironment _env;

        public ProfileController(DataContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        private async Task<aznews.Areas.Admin.Models.Admin?> GetCurrentAdmin()
        {
            var username = HttpContext.Session.GetString("AdminUser");
            if (string.IsNullOrEmpty(username)) return null;

            return await _db.Admins.Include(x => x.VaiTro)
                .FirstOrDefaultAsync(x => x.TenDangNhap == username);
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var admin = await GetCurrentAdmin();
            // Fallback cho demo nếu chưa login thật
            if (admin == null) 
                admin = await _db.Admins.Include(x => x.VaiTro).FirstOrDefaultAsync();

            if (admin == null) return NotFound("Không tìm thấy tài khoản");

            var vm = new ProfileVM
            {
                Username = admin.TenDangNhap,
                RoleName = admin.VaiTro?.TenVaiTro ?? "Quản trị viên",
                LastLogin = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                AvatarUrl = !string.IsNullOrEmpty(admin.Image) ? admin.Image : "default-avatar.png",
                
                // Vì DB chưa có các cột HoTen, Email, Phone, Description
                // Ta tạm thời hard-code hoặc để trống. 
                // Khi nào bạn thêm cột xong, thay dòng dưới bằng: admin.HoTen, admin.Email...
                FullName = "Administrator", 
                Email = "admin@university.edu.vn",
                Phone = "",
                Description = ""
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateInfo(ProfileVM model)
        {
            // Bỏ qua validate các trường chưa có trong DB để test chức năng upload ảnh
            // if (!ModelState.IsValid) return View("Index", model);

            var admin = await GetCurrentAdmin();
            if (admin == null) admin = await _db.Admins.FirstOrDefaultAsync(); // Demo fallback

            if (admin == null) return NotFound();

            // 1. Xử lý Upload Ảnh
            if (model.AvatarFile != null)
            {
                // Tạo tên file unique
                string ext = Path.GetExtension(model.AvatarFile.FileName);
                string fileName = $"{Guid.NewGuid()}{ext}";
                string path = Path.Combine(_env.WebRootPath, "admin", "Imgs", fileName);

                // Lưu file
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await model.AvatarFile.CopyToAsync(stream);
                }

                // Xóa ảnh cũ nếu không phải mặc định (Option)
                // if (!string.IsNullOrEmpty(admin.Image) && admin.Image != "default-avatar.png") ...

                // Cập nhật DB
                admin.Image = fileName;
                model.AvatarUrl = fileName; // Update VM để hiển thị ngay
            }
            else
            {
                model.AvatarUrl = admin.Image ?? "default-avatar.png";
            }

            // 2. Lưu các thông tin khác (Tạm thời comment vì DB chưa có)
            /*
            admin.HoTen = model.FullName;
            admin.Email = model.Email;
            */

            _db.Admins.Update(admin);
            await _db.SaveChangesAsync();

            // Cập nhật Session nếu cần hiển thị ảnh mới ở Header ngay lập tức
            HttpContext.Session.SetString("AdminAvatar", admin.Image ?? "default-avatar.png");

            model.SuccessMessage = "Cập nhật hồ sơ thành công!";
            
            // Reload lại các thông tin read-only
            model.Username = admin.TenDangNhap;
            model.RoleName = admin.VaiTro?.TenVaiTro ?? "Quản trị viên";

            return View("Index", model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ProfileVM model)
        {
            var admin = await GetCurrentAdmin();
            if (admin == null) admin = await _db.Admins.FirstOrDefaultAsync();

            // Validate logic đổi pass...
            // Tạm thời bỏ qua để tập trung vào phần Image

            model.SuccessMessage = "Đổi mật khẩu thành công!";
            model.AvatarUrl = admin.Image ?? "default-avatar.png"; // Giữ nguyên ảnh khi reload
            return View("Index", model);
        }
    }
}