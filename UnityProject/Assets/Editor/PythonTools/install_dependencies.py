#!/usr/bin/env python
# -*- coding: utf-8 -*-
"""
安装所有必需的依赖
"""

import subprocess
import sys

def install_package(package):
    """安装 Python 包"""
    try:
        print(f"\n正在安装 {package}...")
        subprocess.check_call([sys.executable, "-m", "pip", "install", package])
        print(f"[OK] {package} 安装成功")
        return True
    except subprocess.CalledProcessError as e:
        print(f"[FAIL] {package} 安装失败: {e}")
        return False

def check_package(package_name, import_name=None):
    """检查包是否已安装"""
    if import_name is None:
        import_name = package_name
    
    try:
        __import__(import_name)
        return True
    except ImportError:
        return False

def main():
    print("=" * 60)
    print("Image Asset Processor - 依赖安装脚本")
    print("=" * 60)
    
    # 需要安装的包列表
    packages = [
        ("Pillow", "PIL"),
        ("numpy", "numpy"),
        ("opencv-python", "cv2"),
        ("onnxruntime", "onnxruntime"),
        ("rembg", "rembg"),
    ]
    
    print("\n检查已安装的包...")
    missing_packages = []
    
    for package_name, import_name in packages:
        if check_package(package_name, import_name):
            print(f"[OK] {package_name} 已安装")
        else:
            print(f"[MISS] {package_name} 未安装")
            missing_packages.append(package_name)
    
    if not missing_packages:
        print("\n" + "=" * 60)
        print("所有依赖已安装！")
        print("=" * 60)
        return
    
    print(f"\n需要安装 {len(missing_packages)} 个包...")
    
    # 安装缺失的包
    failed_packages = []
    for package in missing_packages:
        if not install_package(package):
            failed_packages.append(package)
    
    print("\n" + "=" * 60)
    if failed_packages:
        print("安装完成，但有部分包安装失败：")
        for pkg in failed_packages:
            print(f"  - {pkg}")
        print("\n请手动安装失败的包：")
        print(f"pip install {' '.join(failed_packages)}")
    else:
        print("所有依赖安装成功！")
    print("=" * 60)
    
    # 最终验证
    print("\n最终验证...")
    all_ok = True
    for package_name, import_name in packages:
        if check_package(package_name, import_name):
            print(f"[OK] {package_name} 可用")
        else:
            print(f"[FAIL] {package_name} 不可用")
            all_ok = False
    
    if all_ok:
        print("\n[OK] 所有依赖验证通过！")
    else:
        print("\n[FAIL] 部分依赖验证失败，请检查安装")

if __name__ == '__main__':
    main()

