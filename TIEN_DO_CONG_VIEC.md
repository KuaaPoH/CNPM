# NHẬT KÝ TIẾN ĐỘ CÔNG VIỆC VÀ PHÁT TRIỂN DỰ ÁN (WORK LOG)

File này lưu trữ chi tiết quá trình phát triển dự án "Hệ thống Quản lý Đào tạo" (aznews).

---

## 1. TỔNG QUAN DỰ ÁN
*   **Tên dự án:** aznews (University Training Management).
*   **Công nghệ:** ASP.NET Core 8.0 MVC, Entity Framework Core.
*   **Kiến trúc:** Phân quyền 3 vai trò (Admin, Giảng viên, Sinh viên).
*   **Trạng thái:** **BETA STAGE (Đang hoàn thiện các tính năng chuyên sâu)**.

---

## 2. CHI TIẾT CÔNG VIỆC ĐÃ HOÀN THÀNH (Ngày 06/12/2025 - 08/12/2025)
*(Xem các bản ghi cũ trong file này để biết chi tiết về Bảo mật, UI/UX ban đầu và Quản lý Sinh viên/Giảng viên)*

---

## 3. CHI TIẾT CÔNG VIỆC ĐÃ HOÀN THÀNH (Ngày 10/01/2026)

### A. QUẢN TRỊ HỆ THỐNG (ADMIN AREA)
1.  **Phân công giảng dạy (Teaching Assignment):**
    *   Tạo module mới: `PhanCongController`, `PhanCongVM`.
    *   Cho phép Admin phân công giảng viên trực tiếp trên danh sách lớp học phần bằng AJAX (Real-time update).
    *   Tích hợp thanh tìm kiếm và lọc theo Học kỳ/Năm học.
2.  **Nâng cấp Dashboard Admin (Executive Dashboard):**
    *   Tích hợp **Bộ lọc Toàn cầu (Global Filter)**: Lọc dữ liệu toàn trang theo Năm học/Học kỳ.
    *   Thêm khu vực **Cảnh báo học vụ**: Liệt kê các lớp có tỷ lệ rớt cao (>30%) kèm thanh tiến trình trực quan.
    *   Tối ưu hóa biểu đồ (ApexCharts & ECharts): Chỉnh sửa Legend, vị trí hiển thị và hỗ trợ responsive.
3.  **Hồ sơ cá nhân (Admin Profile):**
    *   Tích hợp giao diện Profile hiện đại từ template.
    *   Thêm tính năng **Upload Ảnh đại diện** (Avatar) lưu vào thư mục hệ thống và Database.
    *   Cập nhật Header: Hiển thị Avatar động và lời chào người dùng từ Session.

### B. KHU VỰC GIẢNG VIÊN (LECTURER AREA)
1.  **Dashboard Giảng viên (Workstation):**
    *   Xây dựng trang Dashboard tập trung: Hiển thị KPI lớp đang dạy, lớp chưa chốt điểm.
    *   Tích hợp biểu đồ tròn (Donut Chart) theo dõi tiến độ nhập điểm.
    *   Lối tắt nhanh đến các chức năng: Nhập điểm, Danh sách lớp, Thời khóa biểu.
2.  **Đồng bộ Giao diện Nhập điểm:**
    *   Làm mới trang **Danh sách lớp dạy** và **Nhập điểm**: Sử dụng layout Card, tiêu đề in hoa, bảng kẻ ô chuyên nghiệp.
    *   Tính năng: Tự động tính điểm tổng kết khi nhập điểm thành phần, nút "Gửi duyệt điểm" nổi bật.

### C. CỔNG THÔNG TIN SINH VIÊN (STUDENT PORTAL)
1.  **Dashboard Sinh viên (Integrated Dashboard):**
    *   Tích hợp Dashboard vào trang chủ hệ thống (`/Home`) dành riêng cho sinh viên sau khi đăng nhập.
    *   Tính năng: Hiển thị **GPA Tích lũy (Hệ 4)**, Tín chỉ tích lũy, Tín chỉ nợ.
    *   Biểu đồ: Xu hướng học tập (GPA) qua các kỳ học.
    *   Bảng điểm mini: 5 môn học vừa có điểm mới nhất.
2.  **Hồ sơ Sinh viên (Student Profile):**
    *   Xây dựng trang `/Profile`: Cho phép sinh viên xem thông tin, cập nhật Email/SĐT và thay đổi Ảnh đại diện.
    *   Tính năng **Đổi mật khẩu**: Hỗ trợ BCrypt bảo mật.
3.  **Hệ thống Layout:**
    *   Tạo `_LayoutStudent.cshtml` riêng biệt, tối ưu cho trải nghiệm Portal.

### D. CỐT LÕI HỆ THỐNG & FIX BUG (CORE & FIXES)
1.  **Xử lý Xung đột Namespace:** Fix lỗi `CS0118` bằng cách đổi tên Namespace Area Sinh viên thành `SinhVienArea` để không trùng với Model `SinhVien`.
2.  **Cập nhật Database Schema:**
    *   Bảng `Admin`: Thêm cột `Image` lưu Avatar.
    *   Bảng `SinhVien`: Thêm cột `MaLopHC` liên kết với bảng Lớp hành chính.
    *   Model C#: Cập nhật các **Navigation Properties** (`LopHocPhan`, `LopHanhChinh`) để tối ưu truy vấn EF Core.
3.  **UI/UX Fixes:**
    *   Khắc phục lỗi ghi đè cấu hình Datatable (ẩn "entries per page").
    *   Điều chỉnh vị trí ghi chú biểu đồ (ECharts) tránh đè dữ liệu.

---

## 4. TRẠNG THÁI HIỆN TẠI
*   Hệ thống đã hoàn thiện luồng dữ liệu chính: **Admin phân công -> Giảng viên nhập điểm -> Sinh viên xem kết quả**.
*   Giao diện đồng bộ 100% theo phong cách chuyên nghiệp (NiceAdmin).
*   Các tính năng Profile và Dashboard hoạt động ổn định trên cả 3 vai trò.

## 5. HƯỚNG PHÁT TRIỂN TIẾP THEO
*   Hoàn thiện chức năng **Đăng ký tín chỉ** cho sinh viên.
*   Xây dựng module **Thời khóa biểu** (Lịch học/Lịch dạy) chi tiết.
*   Xuất báo cáo kết quả học tập ra file PDF có đóng dấu số (giả lập).
