using aznews.Areas.Admin.Models; // để dùng DiemStatus

namespace aznews.Areas.GiangVien.Models.Diem
{
    public class LhpRowVM
    {
        public int MaLHP { get; set; }
        public string TenHP { get; set; } = "";
        public string HocKy { get; set; } = "";
        public string NamHoc { get; set; } = "";

        public int? LoaiLop { get; set; }      // 1=LT, 2=TH, 3=DA
        public string? TenNhom { get; set; }   // "1", "2" ... hoặc sẵn "LT1", "TH2"

        public aznews.Areas.Admin.Models.DiemStatus TrangThai { get; set; }
        public string RouteText { get; set; } = "";

        private static string MapLoai(int? loai)
            => loai == 1 ? "LT" : loai == 2 ? "TH" : loai == 3 ? "DA" : "";

        // Nếu TenNhom đã có chữ LT/TH/DA ở đầu thì dùng luôn;
        // nếu chỉ là số (vd "1") thì ghép tiền tố từ LoaiLop -> "LT1"
        public string TenNhomHienThi
        {
            get
            {
                var nhom = (TenNhom ?? "").Trim();
                if (string.IsNullOrEmpty(nhom)) return nhom;
                // đã có tiền tố chữ?
                if (char.IsLetter(nhom[0])) return nhom;
                var prefix = MapLoai(LoaiLop);
                return string.IsNullOrEmpty(prefix) ? nhom : prefix + nhom;
            }
        }

        public string TenDayDu
            => string.IsNullOrWhiteSpace(TenNhomHienThi)
               ? TenHP
               : $"{TenHP} {TenNhomHienThi}";
    }

}
