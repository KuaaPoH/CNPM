using aznews.Areas.Admin.Models;
using aznews.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace aznews.Controllers
{
    public class AccountController : Controller
    {
        private readonly DataContext _db;
        public AccountController(DataContext db) => _db = db;

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ.";
                return View();
            }

            username = username.Trim();

            var admin = await _db.Admins
           .Include(a => a.VaiTro)
           .FirstOrDefaultAsync(a => a.TenDangNhap == username && a.MatKhau == password && a.TrangThai);

            if (admin != null)
            {
                HttpContext.Session.SetString("UserName", admin.TenDangNhap);
                HttpContext.Session.SetInt32("Role", admin.MaVaiTro);
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }


            // 2) Giảng viên: MaSoGV + MatKhau, TrangThai = 1
            var gv = await _db.GiangViens
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaSoGV == username
                                          && x.MatKhau == password
                                          && x.TrangThai);

            if (gv != null)
            {
                HttpContext.Session.SetInt32("Role", gv.MaVaiTro);       // = 2
                HttpContext.Session.SetInt32("MaGV", gv.MaGiangVien);
                HttpContext.Session.SetString("UserName", gv.MaSoGV);
                HttpContext.Session.SetString("FullName", gv.HoTen ?? "");
                return RedirectToAction("Index", "Home", new { area = "GiangVien" });
            }

            // 3) Sinh viên: MaSoSV + MatKhau, TrangThai = 1
            var sv = await _db.SinhViens
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaSoSV == username
                                          && x.MatKhau == password
                                          && x.TrangThai);

            if (sv != null)
            {
                HttpContext.Session.SetInt32("Role", sv.MaVaiTro);       // = 3
                HttpContext.Session.SetInt32("MaSV", sv.MaSinhVien);
                HttpContext.Session.SetString("UserName", sv.MaSoSV);
                HttpContext.Session.SetString("FullName", sv.HoTen ?? "");
                return RedirectToAction("Index", "Home");               // giao diện ngoài
            }

            ViewBag.Error = "Sai tài khoản hoặc mật khẩu.";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
