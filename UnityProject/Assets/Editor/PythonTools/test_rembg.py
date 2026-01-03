#!/usr/bin/env python
# -*- coding: utf-8 -*-
"""
测试 rembg 是否正常工作
"""

import sys

print("=" * 60)
print("Testing rembg installation...")
print("=" * 60)

# 测试导入
try:
    print("\n1. Testing rembg import...")
    from rembg import remove
    print("   ✓ rembg imported successfully")
except ImportError as e:
    print(f"   ✗ Failed to import rembg: {e}")
    print("   Please install rembg: pip install rembg")
    sys.exit(1)
except Exception as e:
    print(f"   ✗ Error importing rembg: {e}")
    sys.exit(1)

# 测试基本功能
try:
    print("\n2. Testing rembg.remove() function...")
    from io import BytesIO
    from PIL import Image
    import numpy as np
    
    # 创建一个简单的测试图片（100x100 的红色图片）
    print("   Creating test image...")
    test_image = Image.new('RGB', (100, 100), color='red')
    
    # 转换为字节
    img_bytes = BytesIO()
    test_image.save(img_bytes, format='PNG')
    img_bytes.seek(0)
    input_bytes = img_bytes.read()
    
    print(f"   Input image size: {len(input_bytes)} bytes")
    
    # 尝试去除背景
    print("   Calling rembg.remove()...")
    print("   (This may take a moment on first run as it downloads the model...)")
    output_bytes = remove(input_bytes)
    
    print(f"   ✓ rembg.remove() executed successfully")
    print(f"   Output image size: {len(output_bytes)} bytes")
    
    # 验证输出
    result_image = Image.open(BytesIO(output_bytes))
    print(f"   Result image size: {result_image.size}")
    print(f"   Result image mode: {result_image.mode}")
    
    print("\n" + "=" * 60)
    print("✓ rembg is working correctly!")
    print("=" * 60)
    
except Exception as e:
    print(f"\n   ✗ Error testing rembg: {e}")
    import traceback
    traceback.print_exc()
    print("\n" + "=" * 60)
    print("✗ rembg test failed!")
    print("=" * 60)
    sys.exit(1)

