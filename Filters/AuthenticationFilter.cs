using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace aznews.Filters
{
    public class AuthenticationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var path = context.HttpContext.Request.Path.Value?.ToLower() ?? "";
            
            // Bỏ qua trang Login để tránh vòng lặp vô tận
            if (path.StartsWith("/account/login") || path.StartsWith("/account/logout"))
            {
                return;
            }

            var session = context.HttpContext.Session;
            var roleId = session.GetInt32("Role");

            // 1. Kiểm tra đăng nhập
            if (roleId == null)
            {
                // Chưa đăng nhập -> Chuyển về Login
                context.Result = new RedirectToActionResult("Login", "Account", new { area = "" });
                return;
            }

            // 2. Kiểm tra quyền truy cập Area (Authorization)
            var area = context.RouteData.Values["area"]?.ToString();
            
            if (string.Equals(area, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                if (roleId != 1) // Không phải Admin
                {
                    // Đá về Login thay vì Forbid
                    context.Result = new RedirectToActionResult("Login", "Account", new { area = "" });
                }
            }
            else if (string.Equals(area, "GiangVien", StringComparison.OrdinalIgnoreCase))
            {
                if (roleId != 2) // Không phải Giảng viên
                {
                    context.Result = new RedirectToActionResult("Login", "Account", new { area = "" });
                }
            }
            else
            {
                // Area rỗng (Sinh viên / Chung)
                // Tuỳ logic: Có muốn cấm Admin/GV ra trang sinh viên không?
                // Nếu muốn Admin cũng xem được trang chủ thì bỏ qua.
                // Nếu muốn SV chỉ được ở đây:
                // if (roleId != 3) { ... }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Không làm gì sau khi action chạy
        }
    }
}
