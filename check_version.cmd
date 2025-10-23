@echo off
setlocal EnableExtensions
chcp 65001 >nul

REM ---- Hiển thị banner nếu có banner.py
if exist "%~dp0banner.py" (
  py -3 "%~dp0banner.py" 2>nul || python "%~dp0banner.py" 2>nul
)

echo.
echo ===============================
echo   KIỂM TRA PHIÊN BẢN DỰ ÁN
echo ===============================
echo.

REM ---- Kiểm tra Git đã cài hay chưa
git --version >nul 2>&1 || (
  echo ❌ Chưa cài Git. Vui lòng cài tại: https://git-scm.com/downloads
  goto :END
)

REM ---- Kiểm tra thư mục hiện tại có phải Git repo không
git rev-parse --is-inside-work-tree >nul 2>&1 || (
  echo ❌ Thư mục hiện tại không phải Git repo.
  goto :END
)

REM ---- Chuyển sang nhánh main nếu nhánh này tồn tại ở local
git show-ref --verify --quiet refs/heads/main && git checkout main >nul 2>&1

REM ---- Lấy thông tin mới nhất từ GitHub
echo 🔄 Đang kiểm tra cập nhật từ GitHub...
git fetch origin >nul 2>&1

REM ---- So sánh HEAD local với commit mới nhất trên origin/main
for /f %%i in ('git rev-parse HEAD') do set "LOCAL=%%i"
for /f %%i in ('git rev-parse origin/main') do set "REMOTE=%%i"

if "%LOCAL%"=="%REMOTE%" (
  echo ✅ Bạn đang dùng phiên bản mới nhất.
) else (
  echo ⚠️ Mã nguồn trên máy đang cũ vui lòng cập nhật lên phiên bản mới.
  choice /C YN /N /M "👉 Cập nhật ngay? Y/N: "
  if errorlevel 2 (
    echo ⏭  Bỏ qua cập nhật.
  ) else (
    echo ⬇️  Đang cập nhật...
    git pull --ff-only origin main
    if errorlevel 1 (
      echo ❌ Cập nhật thất bại.
      echo 💡 Gợi ý xử lý:
      echo    - Nếu có thay đổi CHƯA commit: chạy git stash rồi thử lại.
      echo    - Nếu bị xung đột: giải quyết conflict, commit rồi thử lại.
    ) else (
      echo ✅ Đã cập nhật thành công lên phiên bản mới nhất.
    )
  )
)

:END
echo.
pause
