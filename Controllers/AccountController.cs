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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            var u = await _db.NguoiDungs.AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.TenDangNhap == username &&
                    x.MatKhau == password &&
                    (x.TrangThai ?? true));

            if (u == null)
            {
                ViewBag.Error = "Sai tài khoản hoặc mật khẩu.";
                return View();
            }

            HttpContext.Session.SetString("UserName", u.TenDangNhap);
            HttpContext.Session.SetInt32("Role", u.MaVaiTro);

            return u.MaVaiTro switch
            {
                1 => RedirectToAction("Index", "Home", new { area = "Admin" }),
                2 => RedirectToAction("Index", "Home", new { area = "GiangVien" }),
                3 => RedirectToAction("Index", "Home", new { area = "" }),
                _ => RedirectToAction(nameof(Login))
            };
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }
    }
}
