using System.Collections.Generic;

namespace aznews.Areas.Admin.Models.Diem
{
    // ViewModel cho màn quản lý điểm 1 LHP
    public class DiemManageVM
    {
        public int MaLHP { get; set; }
        public string TenHP { get; set; } = "";
        public string HocKy { get; set; } = "";
        public string NamHoc { get; set; } = "";
        public DiemTrangThai TrangThai { get; set; }

        public IList<DiemRowVM> Rows { get; set; } = new List<DiemRowVM>();
    }
}
