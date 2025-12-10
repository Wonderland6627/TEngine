@echo off
chcp 65001 >nul
echo ========================================
echo 拆分 levels.json 脚本
echo ========================================
echo.

cd /d "%~dp0"

python split_levels.py

if %errorlevel% neq 0 (
    echo.
    echo 错误: 脚本执行失败！
    echo 请确保已安装 Python 并添加到系统 PATH 中
    pause
    exit /b %errorlevel%
)

echo.
echo ========================================
echo 拆分完成！
echo ========================================
pause

