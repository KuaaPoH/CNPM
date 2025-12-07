using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using aznews.Models;
using aznews.Models.Diem;
using aznews.Areas.Admin.Models; // DiemStatus

namespace aznews.Controllers
{
    // URL mặc định: /BangDiem/Index?namHoc=...&hocKy=...
    public class BangDiemController : Controller
    {
        private readonly DataContext _db;
        public BangDiemController(DataContext db) => _db = db;

        // TODO: thay bằng cơ chế lấy mã SV trong hệ thống của bạn (Session/Claims/…)
        private int? GetCurrentSinhVienId()
        {
            // Lấy MaSV từ session (khớp với AccountController)
            return HttpContext.Session.GetInt32("MaSV");
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? namHoc, string? hocKy)
        {
            var svId = GetCurrentSinhVienId();
            if (svId == null) return Unauthorized();

            // Chỉ lấy lớp học phần đã được admin duyệt
            var baseQuery =
                from d in _db.DiemLops.AsNoTracking()
                join lhp in _db.LopHocPhans.AsNoTracking() on d.MaLHP equals lhp.MaLHP
                join hp in _db.HocPhans.AsNoTracking() on lhp.MaHP equals hp.MaHP
                where d.MaSinhVien == svId.Value
                      && lhp.DiemStatus == DiemStatus.Approved
                select new BangDiemRowVM
                {
                    NamHoc = lhp.NamHoc ?? "",
                    HocKy = lhp.HocKy ?? "",
                    MaSoHP = hp.MaSoHP ?? "",
                    TenHP = hp.TenHP ?? "",
                    SoTC = hp.SoTinChi,          
                    DiemQT = d.DiemQT,
                    DiemThi = d.DiemThi,
                    DiemTong = d.DiemTong
                };


            // danh sách năm học cho dropdown
            var namHocs = await baseQuery.Select(x => x.NamHoc)
                                         .Distinct()
                                         .OrderByDescending(x => x)
                                         .ToListAsync();

            // lọc
            if (!string.IsNullOrWhiteSpace(namHoc))
                baseQuery = baseQuery.Where(x => x.NamHoc == namHoc);
            if (!string.IsNullOrWhiteSpace(hocKy))
                baseQuery = baseQuery.Where(x => x.HocKy == hocKy);

            var rows = await baseQuery
                .OrderByDescending(x => x.NamHoc)
                .ThenBy(x => x.HocKy)
                .ThenBy(x => x.MaSoHP)
                .ToListAsync();

            var vm = new BangDiemIndexVM
            {
                FilterNamHoc = namHoc,
                FilterHocKy = hocKy,
                Rows = rows,
                NamHocs = namHocs
            };

            return View(vm);
        }
    }
}
