namespace aznews.Models.Diem
{
    public class BangDiemRowVM
    {
        public string NamHoc { get; set; } = "";
        public string HocKy { get; set; } = "";
        public string MaSoHP { get; set; } = "";
        public string TenHP { get; set; } = "";
        public int SoTC { get; set; }

        public decimal? DiemQT { get; set; }
        public decimal? DiemThi { get; set; }
        public decimal? DiemTong { get; set; }
    }

    public class BangDiemIndexVM
    {
        public string? FilterNamHoc { get; set; }
        public string? FilterHocKy { get; set; }

        public IList<BangDiemRowVM> Rows { get; set; } = new List<BangDiemRowVM>();

        public IEnumerable<string> NamHocs { get; set; } = Enumerable.Empty<string>();
        public IEnumerable<string> HocKys { get; set; } = new[] { "HK1", "HK2", "Hè" };
    }
}
