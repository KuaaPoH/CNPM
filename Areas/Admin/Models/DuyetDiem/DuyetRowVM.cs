namespace aznews.Areas.Admin.Models.DuyetDiem
{
    public class DuyetRowVM
    {
        public int MaLHP { get; set; }

        public string TenHP { get; set; } = "";
        public string? MaSoHP { get; set; }          // mã học phần
        public string? TenNhom { get; set; }         // LT1/TH2/DA3...
        public string HocKy { get; set; } = "";
        public string NamHoc { get; set; } = "";

        public string? GV { get; set; }              // tên giảng viên
              
        public int SoSV { get; set; }             // số sinh viên trong lớp HP

        public DiemStatus TrangThai { get; set; }

        public DateTime? SubmittedAt { get; set; }
        public string? SubmittedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; }
        public string? GhiChu { get; set; }          // LHP.DiemNote
    }
}
