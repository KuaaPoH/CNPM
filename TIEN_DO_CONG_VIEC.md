# NHẬT KÝ TIẾN ĐỘ CÔNG VIỆC VÀ PHÁT TRIỂN DỰ ÁN (WORK LOG)

File này lưu trữ chi tiết quá trình phát triển dự án "Hệ thống Quản lý Đào tạo" (aznews).

---

## 1. TỔNG QUAN DỰ ÁN
*   **Tên dự án:** aznews (University Training Management).
*   **Công nghệ:** ASP.NET Core 8.0 MVC, Entity Framework Core.
*   **Kiến trúc:** Phân quyền 3 vai trò (Admin, Giảng viên, Sinh viên) qua Areas.
*   **Trạng thái:** **RELEASE CANDIDATE (Sẵn sàng sử dụng)**.

---

## 2. CHI TIẾT CÔNG VIỆC ĐÃ HOÀN THÀNH (Ngày 06/12/2025)

### A. BẢO MẬT & HỆ THỐNG (SECURITY & CORE)
1.  **Mã hóa Mật khẩu (Password Hashing):**
    *   Tích hợp thư viện `BCrypt.Net-Next`.
    *   **Migration:** Đã chạy script chuyển đổi toàn bộ mật khẩu cũ (text thuần) sang mã hash BCrypt trong Database.
    *   **Login Logic:** Cập nhật `AccountController` để xác thực bằng `BCrypt.Verify`. Có cơ chế fallback (hỗ trợ mật khẩu cũ chưa hash để tránh lỗi hệ thống).
    *   **User Management:** Cập nhật `SinhVienController` và `GiangVienController` (Admin Area) để tự động hash mật khẩu khi **Thêm mới** hoặc **Sửa** (nếu người dùng nhập mật khẩu mới).

2.  **Phân quyền & Kiểm soát truy cập (Authorization):**
    *   Tạo `Filters/AuthenticationFilter.cs`: Kiểm tra Session Role chặt chẽ.
    *   **Logic chặn:** Admin chỉ vào được Area Admin, GV chỉ vào được Area GiangVien. Truy cập sai vùng sẽ tự động chuyển hướng về trang **Login**.
    *   **Global Filter:** Đăng ký Filter toàn cục, bảo vệ tất cả các trang (trừ Login).

### B. GIAO DIỆN & TRẢI NGHIỆM NGƯỜI DÙNG (UI/UX)

#### 1. Đồng bộ Giao diện (Consistency)
*   **Sinh viên:**
    *   Làm mới hoàn toàn `_Layout.cshtml` theo template "NiceAdmin" (giống Admin).
    *   Sửa `StudentMenu` Component: Thêm thẻ `<aside>`, logic active menu, icon chuẩn.
    *   Khắc phục các lỗi CSS: Nền xanh dương (do `site.css`), Footer quá cao (đã xóa Copyright).
*   **Admin & Giảng viên:**
    *   Cập nhật Header: Hiển thị đúng **Tên người dùng** (lấy từ Session) và nút **Đăng xuất** hoạt động đúng.

#### 2. Dashboard & Thống kê (Admin Dashboard)
*   **KPI Cards:** Hiển thị 4 chỉ số quan trọng (Tổng SV, Tổng GV, Tổng Lớp, **Lớp chờ duyệt điểm**).
*   **Biểu đồ:**
    *   **Trạng thái nhập điểm (Pie Chart):** Tỉ lệ lớp Đang nhập / Chờ duyệt / Đã chốt.
    *   **Phổ điểm đào tạo (Bar Chart):** Thống kê phân bố điểm (Giỏi, Khá, TB, Rớt) dựa trên dữ liệu thực tế.
*   **Danh sách cần xử lý:** Bảng "Vừa gửi duyệt" được thiết kế lại đẹp mắt (Avatar, Badge trạng thái vàng, Nút bo tròn).

#### 3. Trang Login (Đăng nhập)
*   **Redesign:** Chuyển sang giao diện **Split-Screen** (Màn hình chia đôi).
    *   **Trái:** Ảnh nền trường học chất lượng cao, sáng sủa.
    *   **Phải:** Form đăng nhập hiện đại, đổ bóng nổi bật, bo góc mềm mại.
*   **Fix lỗi:** Sửa lỗi Razor syntax (`@media`), lỗi mất ảnh nền, căn chỉnh vị trí giữa màn hình.

#### 4. Tiện ích khác
*   **Real-time Clock:** Tích hợp đồng hồ hiển thị ngày giờ theo thời gian thực (Javascript) trên Header của cả 3 giao diện.
*   **Bảng điểm Sinh viên:** Thiết kế lại giao diện bảng điểm chi tiết: Card trắng, Badge màu cho điểm số, chữ in đậm dễ đọc.

---

## 3. TRẠNG THÁI HIỆN TẠI
*   Hệ thống hoạt động ổn định, không còn lỗi biên dịch.
*   Giao diện đồng nhất, chuyên nghiệp.
*   Bảo mật ở mức tốt.

## 4. HƯỚNG PHÁT TRIỂN TIẾP THEO (Gợi ý)
*   Thêm chức năng **Đổi mật khẩu** (User tự đổi).
*   Thêm chức năng **Quên mật khẩu** (Gửi email).
*   Xuất báo cáo điểm ra Excel/PDF.

---

## 5. CHI TIẾT CÔNG VIỆC ĐÃ HOÀN THÀNH (Ngày 08/12/2025)

### A. Sửa lỗi & Cải thiện UI/UX chung
1.  **Khắc phục `InvalidOperationException` cho Layout:**
    *   Đã thêm `@RenderSection("Scripts", required: false)` vào `/Areas/Admin/Views/Shared/_LayoutAdmin.cshtml`.
    *   Đã thêm `@RenderSection("Scripts", required: false)` vào `/Areas/GiangVien/Views/Shared/_Layout_GiangVien.cshtml`.
2.  **Điều chỉnh màu nền giao diện Admin:**
    *   Thay đổi màu nền `body` trong `wwwroot/admin/css/style.css` từ `#f6f9ff` thành `#e6ecf3` (xám xanh nhẹ) để có độ tương phản tốt hơn với các thẻ trắng.
3.  **Cải thiện giao diện trang Đăng nhập:**
    *   Đồng bộ kiểu chữ và màu chữ của `body` với giao diện admin ("Open Sans", `#444444`).
    *   Thay đổi tiêu đề `h3` "Đăng Nhập" thành màu `#012970` và font "Nunito".
    *   Điều chỉnh bố cục chính của form đăng nhập: giữ hình ảnh chiếm 50% chiều rộng và phần form chiếm 50% của tổng chiều rộng `col-lg-12` được căn phải.
    *   Tách "Ghi nhớ tôi" và "Quên mật khẩu?" thành hai dòng riêng biệt và căn giữa.
    *   Xóa dòng "Chưa có tài khoản? Liên hệ Admin".

### B. Cải thiện chức năng Quản lý Điểm và Danh sách Sinh viên (Khu vực Giảng viên)
1.  **Thêm nút "Quay lại" cho các trang Giảng viên:**
    *   Thêm nút "Quay lại" vào trang Danh sách Sinh viên (`Areas/GiangVien/Views/SinhVien/DanhSach.cshtml`), liên kết đến `GiangVien/LopDay/Index`.
    *   Thêm nút "Quay lại" vào trang Điểm danh (`Areas/GiangVien/Views/DiemDanh/Index.cshtml`), liên kết đến `GiangVien/LopDay/Index`.
    *   Điều chỉnh vị trí các nút "Quay lại" này xuống cuối trang theo yêu cầu.
2.  **Đồng bộ giao diện Danh sách Sinh viên với Danh sách Lớp dạy:**
    *   Tái cấu trúc bố cục trang Danh sách Sinh viên (`Areas/GiangVien/Views/SinhVien/DanhSach.cshtml`) để giống với `GiangVien/LopDay/Index.cshtml`, bao gồm:
        *   Tiêu đề `h4` với kiểu dáng nhất quán.
        *   Nút "Quay lại" được đặt ở đầu trang, bên cạnh tiêu đề.
        *   Thêm form tìm kiếm sinh viên theo tên/mã số.
        *   Điều chỉnh kiểu bảng.
3.  **Triển khai chức năng tìm kiếm Sinh viên:**
    *   Cập nhật `DanhSach` action method trong `Areas/GiangVien/Controllers/SinhVienController.cs` để chấp nhận tham số tìm kiếm (`q`) và `lopId`.
    *   Thực hiện lọc dữ liệu sinh viên dựa trên truy vấn tìm kiếm.
    *   Sửa lỗi biên dịch (`IIncludableQueryable`) trong `SinhVienController.cs` để đảm bảo chức năng tìm kiếm hoạt động ổn định.