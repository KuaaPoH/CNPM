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
*File được cập nhật tự động bởi Gemini Assistant.*
