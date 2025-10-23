@echo off
setlocal EnableExtensions
chcp 65001 >nul

REM ---- Hiển thị banner nếu có
if exist "%~dp0banner.py" (
  py -3 "%~dp0banner.py" 2>nul || python "%~dp0banner.py" 2>nul
)

echo.
echo ===========================================
echo         DAY CODE LEN GITHUB (FLOW)
echo ===========================================
echo.

REM --- 1) Kiểm tra Git và thư mục repo ---
git --version >nul 2>&1 || (echo ❌ Chưa cài Git. Cài tại: https://git-scm.com/downloads & goto :END)
git rev-parse --is-inside-work-tree >nul 2>&1 || (echo ❌ Thư mục hiện tại không phải Git repo. & goto :END)

REM --- 2) Lấy tên nhánh hiện tại ---
for /f "tokens=*" %%i in ('git rev-parse --abbrev-ref HEAD') do set "CURBR=%%i"

REM --- 3) Nếu đang ở main/master thì buộc tạo/chuyển sang nhánh tính năng ---
if /I "%CURBR%"=="main"   goto :NEED_FEATURE
if /I "%CURBR%"=="master" goto :NEED_FEATURE
goto :HAVE_FEATURE

:NEED_FEATURE
echo ⚠️  Bạn đang ở nhánh "%CURBR%". Không được push trực tiếp vào main/master.
set /p NEWBR=👉 Nhập tên nhánh tính năng (ví dụ: feature-login): 
if "%NEWBR%"=="" (echo ❌ Chưa nhập tên nhánh. & goto :END)

REM --- Nếu nhánh đã tồn tại thì checkout, ngược lại tạo mới ---
git show-ref --verify --quiet refs/heads/%NEWBR%
if %ERRORLEVEL%==0 (
  echo 🔀 Chuyển sang nhánh có sẵn: %NEWBR%
  git checkout %NEWBR%
) else (
  echo 🌱 Tạo nhánh mới: %NEWBR%
  git checkout -b %NEWBR%
)
set "CURBR=%NEWBR%"

:HAVE_FEATURE
echo.
echo ✅ Đang làm việc trên nhánh: %CURBR%
echo.

REM --- 4) Nhập nội dung commit ---
set /p MSG=📝 Nhập nội dung commit: 
if "%MSG%"=="" set "MSG=Cập nhật mã nguồn"

REM --- 5) Thêm & Commit (nếu không có gì để commit thì bỏ qua thông báo) ---
echo 🔄 Đang chạy: git add .
git add .

echo 🧷 Đang chạy: git commit -m "%MSG%"
git commit -m "%MSG%"
if %ERRORLEVEL% NEQ 0 (
  echo Thông báo: Có thể không có thay đổi nào để commit. Tiếp tục thử push nếu cần.
)

REM --- 6) Push lên origin, tự thêm upstream (-u) nếu lần đầu ---
echo 📤 Đang push lên origin/%CURBR% ...
git rev-parse --abbrev-ref --symbolic-full-name @{u} >nul 2>&1
if %ERRORLEVEL%==0 (
  git push
) else (
  git push -u origin %CURBR%
)

if %ERRORLEVEL% NEQ 0 (
  echo ❌ Push thất bại. Kiểm tra mạng/quyền truy cập hoặc xung đột.
  goto :END
)

echo ✅ Push thành công lên origin/%CURBR%.
echo.

REM --- 7) Mở trang tạo Pull Request trên GitHub (nếu xác định được URL) ---
for /f "tokens=*" %%i in ('git remote get-url origin') do set "GITURL=%%i"

REM --- Chuyển SSH -> HTTPS nếu cần; bỏ đuôi .git ---
set "REPOURL=%GITURL%"
echo %REPOURL% | find "git@github.com:" >nul
if %ERRORLEVEL%==0 (
  set "REPOURL=https://github.com/%REPOURL:git@github.com:=%"
)
if /I "%REPOURL:~-4%"==".git" set "REPOURL=%REPOURL:~0,-4%"

REM --- Xác định nhánh chính trên remote: ưu tiên main, rơi về master ---
set "BASE=main"
git ls-remote --heads origin main >nul 2>&1 || set "BASE=master"

set "PRURL=%REPOURL%/compare/%BASE%...%CURBR%?expand=1"

echo 🔗 Mở trang tạo Pull Request:
echo %PRURL%
start "" "%PRURL%"

:END
echo.
pause
