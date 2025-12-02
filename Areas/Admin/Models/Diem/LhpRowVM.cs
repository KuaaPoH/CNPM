using aznews.Areas.Admin.Models; // để dùng DiemStatus

namespace aznews.Areas.GiangVien.Models.Diem
{
    public class LhpRowVM
    {
        public int MaLHP { get; set; }
        public string TenHP { get; set; } = "";
        public string HocKy { get; set; } = "";
        public string NamHoc { get; set; } = "";
        public string RouteText { get; set; } = "";       // nếu cần hiển thị/tooltip
        public DiemStatus TrangThai { get; set; }
    }
}
