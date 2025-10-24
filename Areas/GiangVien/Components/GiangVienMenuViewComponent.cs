using System.Linq;
using System.Threading.Tasks;
using aznews.Models;                       // DataContext
using aznews.Areas.Admin.Models;           // AdminMenu entity
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace aznews.Areas.GiangVien.Components
{
    // Tên component => "GiangVienMenu" (bỏ hậu tố ViewComponent)
    public class GiangVienMenuViewComponent : ViewComponent
    {
        private readonly DataContext _db;
        public GiangVienMenuViewComponent(DataContext db) => _db = db;

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Lấy menu dành cho giảng viên (MaVaiTro = 2), đang active
            var items = await _db.AdminMenus
                .Where(m => (m.IsActive ?? false) && m.MaVaiTro == 2)
                .OrderBy(m => m.ItemLevel)
                .ThenBy(m => m.ParentLevel)
                .ThenBy(m => m.ItemOrder)
                .ToListAsync();

            return View(items);
        }
    }
}
