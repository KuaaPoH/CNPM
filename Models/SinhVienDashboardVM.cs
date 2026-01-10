using aznews.Areas.Admin.Models;

namespace aznews.Models
{
    public class SinhVienDashboardVM
    {
        public string HoTen { get; set; } = "";
        public string MaSV { get; set; } = "";
        public string LopHanhChinh { get; set; } = "";
        public string Avatar { get; set; } = "";

        public double GPA_TichLuy { get; set; }
        public int TinChi_TichLuy { get; set; }
        public int TinChi_No { get; set; }
        public int MonDangHoc { get; set; }

        public List<DiemLopHocPhanVM> DiemMoiNhat { get; set; } = new();
        public List<GPAPerSemester> BieudoHocTap { get; set; } = new();
    }

    public class DiemLopHocPhanVM
    {
        public string TenHP { get; set; } = "";
        public int SoTinChi { get; set; }
        public double? DiemCC { get; set; }
        public double? DiemGK { get; set; }
        public double? DiemCK { get; set; }
        public double? DiemTong { get; set; }
        public string DiemChu { get; set; } = "";
        public double DiemHe4 { get; set; }
    }

    public class GPAPerSemester
    {
        public string HocKyNam { get; set; } = "";
        public double GPA { get; set; }
    }
}
