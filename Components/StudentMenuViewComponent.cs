using aznews.Models;
using Microsoft.AspNetCore.Mvc;

namespace aznews.Components
{
    [ViewComponent(Name = "StudentMenu")]
    public class StudentMenuViewComponent : ViewComponent
    {
        private readonly DataContext _context;
        public StudentMenuViewComponent(DataContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            // Lấy danh sách menu dành cho Sinh viên (Role = 3)
            // Giả sử bảng AdminMenus dùng chung cho toàn hệ thống
            // Trạng thái = true (đang hoạt động)
            var items = _context.AdminMenus
                .Where(m => (m.IsActive) && m.MaVaiTro == 3)
                .OrderBy(m => m.ItemOrder)
                .ToList();

            return View(items);
        }
    }
}
