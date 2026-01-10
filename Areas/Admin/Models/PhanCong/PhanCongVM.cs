namespace aznews.Areas.Admin.Models.PhanCong
{
    public class PhanCongRowVM
    {
        public int MaLHP { get; set; }
        public string MaSoHP { get; set; } = "";
        public string TenHP { get; set; } = "";
        public string TenNhom { get; set; } = ""; // N01, TH1...
        public int? MaGV { get; set; } // Giảng viên hiện tại
        public string? TenGV { get; set; } // Tên GV hiện tại (để hiển thị nếu cần)
        public int SoSV { get; set; }
    }

    public class PhanCongIndexVM
    {
        public List<PhanCongRowVM> Classes { get; set; } = new();
        
        // Dùng để render dropdown trong View
        public List<SimpleGiangVienVM> AllGiangViens { get; set; } = new();

        public string? SelectedHocKy { get; set; }
        public string? SelectedNamHoc { get; set; }
    }

    public class SimpleGiangVienVM
    {
        public int MaGV { get; set; }
        public string TenGV { get; set; } = "";
        public string MaSoGV { get; set; } = "";
    }
}
