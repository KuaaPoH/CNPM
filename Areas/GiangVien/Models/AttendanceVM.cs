using System;
using System.Collections.Generic;
using aznews.Areas.Admin.Models;

namespace aznews.Areas.GiangVien.Models
{
    // Đại diện CHI TIẾT 1 DÒNG sinh viên trong bảng điểm danh
    public class AttendanceRowVM
    {
        public int MaSinhVien { get; set; }          // Khóa SV dùng để lưu DB
        public string MaSoSV { get; set; } = string.Empty; // Hiển thị
        public string HoTen { get; set; } = string.Empty;   // Hiển thị

        // Trạng thái điểm danh của SV tại NGÀY đang chọn
        // Dùng enum AttendanceStatus (0= Có mặt, 1= Có phép, 2= Muộn, 3= Vắng)
        public AttendanceStatus TrangThai { get; set; } = AttendanceStatus.CoMat;
    }

    // Gói dữ liệu cho TOÀN BỘ màn “điểm danh” của 1 lớp và 1 ngày
    public class AttendanceVM
    {
        public int MaLHP { get; set; }               // Lớp học phần đang điểm danh
        public DateTime Ngay { get; set; }           // Ngày điểm danh

        // Danh sách các dòng (mỗi dòng tương ứng 1 SV trong lớp)
        public List<AttendanceRowVM> Entries { get; set; } = new();
    }
}
