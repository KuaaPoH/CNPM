using System;
using System.Linq;
using System.Threading.Tasks;
using aznews.Areas.Admin.Models;
using aznews.Areas.GiangVien.Models;
using aznews.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace aznews.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    public class DiemDanhController : Controller
    {
        private readonly DataContext _db;
        public DiemDanhController(DataContext db) => _db = db;

        // GET: /GiangVien/DiemDanh?lopId=...&ngay=yyyy-MM-dd
        public async Task<IActionResult> Index(int lopId, DateTime? ngay)
        {
            var date = (ngay ?? DateTime.Today).Date;

            // Lớp + thông tin học phần
            var lop = await _db.LopHocPhans
                .Include(x => x.HocPhan)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaLHP == lopId);
            if (lop == null) return NotFound();

            // Danh sách SV đang học lớp (từ DangKyLop)
            var svQuery = _db.DangKyLops
                .Where(d => d.MaLHP == lopId && d.TrangThai == 1)
                .Include(d => d.SinhVien)
                .Select(d => d.SinhVien!);

            var svs = await svQuery
                .OrderBy(s => s.HoTen)
                .ToListAsync();

            // Lấy điểm danh đã có cho ngày này
            var ddDict = await _db.DiemDanhs
                .Where(x => x.MaLHP == lopId && x.Ngay == date)
                .ToDictionaryAsync(k => k.MaSinhVien, v => v.TrangThai);

            var vm = new AttendanceVM
            {
                MaLHP = lopId,
                Ngay = date,
                Entries = svs.Select(s => new AttendanceRowVM
                {
                    MaSinhVien = s.MaSinhVien,
                    MaSoSV = s.MaSoSV,
                    HoTen = s.HoTen,
                    TrangThai = ddDict.TryGetValue(s.MaSinhVien, out var st)
                                ? st
                                : AttendanceStatus.CoMat // mặc định có mặt
                }).ToList()
            };

            ViewBag.Lop = lop;
            return View(vm);
        }

        // POST: /GiangVien/DiemDanh/Luu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Luu(AttendanceVM vm)
        {
            var date = vm.Ngay.Date;

            // Lấy danh sách hiện có để upsert
            var exist = await _db.DiemDanhs
                .Where(x => x.MaLHP == vm.MaLHP && x.Ngay == date)
                .ToListAsync();

            foreach (var row in vm.Entries)
            {
                var st = row.TrangThai; // 0=CoMat nếu không tick gì
                var current = exist.FirstOrDefault(x => x.MaSinhVien == row.MaSinhVien);

                if (current == null)
                {
                    // chỉ insert khi không phải mặc định? -> Ta vẫn insert để cố định kết quả buổi đó
                    _db.DiemDanhs.Add(new DiemDanh
                    {
                        MaLHP = vm.MaLHP,
                        Ngay = date,
                        MaSinhVien = row.MaSinhVien,
                        TrangThai = st
                    });
                }
                else
                {
                    current.TrangThai = st; 
                    _db.DiemDanhs.Update(current);
                }
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã lưu điểm danh.";
            return RedirectToAction(nameof(Index), new { lopId = vm.MaLHP, ngay = date.ToString("yyyy-MM-dd") });
        }
    }
}
