using System.Linq;
using System.Threading.Tasks;
using aznews.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace aznews.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    public class SinhVienController : Controller
    {
        private readonly DataContext _db;
        public SinhVienController(DataContext db) => _db = db;

        // /GiangVien/SinhVien/DanhSach?lopId=...
        public async Task<IActionResult> DanhSach(int lopId, int page = 1, string q = null)
        {
            const int pageSize = 20;

            var lop = await _db.LopHocPhans
                .Include(x => x.HocPhan)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaLHP == lopId);
            if (lop == null) return NotFound();

            var query = _db.DangKyLops
                .AsNoTracking()
                .Where(d => d.MaLHP == lopId); // Initial filter

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(d => d.SinhVien!.MaSoSV.Contains(q) || d.SinhVien.HoTen.Contains(q));
            }

            // Now apply Include
            var finalQuery = query.Include(d => d.SinhVien);

            int total = await finalQuery.CountAsync(); // Count on finalQuery
            int totalPages = (int)System.Math.Ceiling(total / (double)pageSize);
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var list = await finalQuery // Query from finalQuery
                .OrderBy(d => d.SinhVien!.HoTen)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Lop = lop;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Q = q; // Pass search query to view

            return View(list);
        }
    }
}
