using System;
using System.Linq;
using System.Threading.Tasks;
using aznews.Areas.Admin.Models;
using aznews.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace aznews.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MenuSVController : Controller
    {
        private readonly DataContext _db;
        public MenuSVController(DataContext db) => _db = db;

        
        public async Task<IActionResult> Index(string? q, int page = 1)
        {
            const int pageSize = 10;

            var query = _db.AdminMenus
                           .AsNoTracking()
                           .Where(x => x.MaVaiTro == 3);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var k = q.Trim().ToLower();
                query = query.Where(x =>
                    (x.ItemName ?? "").ToLower().Contains(k) ||
                    (x.ControllerName ?? "").ToLower().Contains(k) ||
                    (x.ActionName ?? "").ToLower().Contains(k) ||
                    (x.Icon ?? "").ToLower().Contains(k));
            }

            var total = await query.CountAsync();
            var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
            page = Math.Clamp(page, 1, totalPages);

            var list = await query
                .OrderBy(x => x.ParentLevel)
                .ThenBy(x => x.ItemLevel)
                .ThenBy(x => x.ItemOrder)
                .ThenBy(x => x.ItemName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;
            ViewBag.Q = q ?? string.Empty;

            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await BuildParentOptions();
            return View(new AdminMenu
            {
                AreaName = "Sinhvien",
                ControllerName = "Home",
                ActionName = "Index",
                ItemLevel = 0,
                ParentLevel = 0,
                ItemOrder = 1,
                IsActive = true,
                MaVaiTro = 3
            });
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminMenu model)
        {
            // Chuẩn hoá
            model.AreaName = "Sinhvien";
            model.MaVaiTro = 3;
            model.ItemName = (model.ItemName ?? "").Trim();
            model.ControllerName = (model.ControllerName ?? "Home").Trim();
            model.ActionName = (model.ActionName ?? "Index").Trim();
            model.Icon = (model.Icon ?? "").Trim();
            model.IdName = (model.IdName ?? "").Trim();

            if (string.IsNullOrWhiteSpace(model.ItemName))
                ModelState.AddModelError(nameof(AdminMenu.ItemName), "Tên menu không được để trống.");

            if (model.ParentLevel == model.AdminMenuID && model.ParentLevel != 0)
                ModelState.AddModelError(nameof(AdminMenu.ParentLevel), "Mục không thể chọn chính nó làm cấp cha.");

            if (!ModelState.IsValid)
            {
                await BuildParentOptions();
                return View(model);
            }

            _db.AdminMenus.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        
        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            var item = await _db.AdminMenus.FindAsync(id);
            if (item == null || item.MaVaiTro != 3) return NotFound();

            await BuildParentOptions(id);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AdminMenu model, string? q, int page = 1)
        {
            var item = await _db.AdminMenus.FindAsync(model.AdminMenuID);
            if (item == null || item.MaVaiTro != 3) return NotFound();

            item.ItemName = (model.ItemName ?? "").Trim();
            item.ItemLevel = model.ItemLevel;
            item.ParentLevel = model.ParentLevel;
            item.AreaName = "Sinhvien";
            item.ControllerName = (model.ControllerName ?? "Home").Trim();
            item.ActionName = (model.ActionName ?? "Index").Trim();
            item.Icon = (model.Icon ?? "").Trim();
            item.IdName = (model.IdName ?? "").Trim();
            item.ItemOrder = model.ItemOrder;
            item.IsActive = model.IsActive;
            item.ItemTarget = string.IsNullOrWhiteSpace(model.ItemTarget) ? null : model.ItemTarget.Trim();
            item.MaVaiTro = 3;

            if (string.IsNullOrWhiteSpace(item.ItemName))
                ModelState.AddModelError(nameof(AdminMenu.ItemName), "Tên menu không được để trống.");

            if (item.ParentLevel == item.AdminMenuID && item.ParentLevel != 0)
                ModelState.AddModelError(nameof(AdminMenu.ParentLevel), "Mục không thể chọn chính nó làm cấp cha.");

            if (!ModelState.IsValid)
            {
                await BuildParentOptions(item.AdminMenuID);
                return View(item);
            }

            _db.Update(item);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { q, page });
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id, string? q, int page = 1)
        {
            var item = await _db.AdminMenus.FindAsync(id);
            if (item == null || item.MaVaiTro != 3) return NotFound();

            _db.AdminMenus.Remove(item);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { q, page });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(long id, string? q, int page = 1)
        {
            var item = await _db.AdminMenus.FindAsync(id);
            if (item == null || item.MaVaiTro != 3) return NotFound();

            item.IsActive = !item.IsActive;
            _db.Update(item);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { q, page });
        }

        private async Task BuildParentOptions(long? excludeId = null)
        {
            var parents = await _db.AdminMenus
                .AsNoTracking()
                .Where(x => x.MaVaiTro == 3 && (excludeId == null || x.AdminMenuID != excludeId.Value))
                .OrderBy(x => x.ParentLevel)
                .ThenBy(x => x.ItemLevel)
                .ThenBy(x => x.ItemOrder)
                .ThenBy(x => x.ItemName)
                .Select(x => new { x.AdminMenuID, Ten = x.ItemName })
                .ToListAsync();

            var selectItems = parents
                .Select(p => new SelectListItem { Value = p.AdminMenuID.ToString(), Text = p.Ten ?? $"#{p.AdminMenuID}" })
                .ToList();

            selectItems.Insert(0, new SelectListItem { Value = "0", Text = "— Không có —" });

            ViewBag.ParentOptions = new SelectList(selectItems, "Value", "Text");
        }
    }
}
