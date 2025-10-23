using Microsoft.AspNetCore.Mvc;

namespace aznews.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    public class HomeController : Controller
    {
        // GET: /GiangVien/Home/Index
        public IActionResult Index()
        {
            return View();
        }
    }
}
