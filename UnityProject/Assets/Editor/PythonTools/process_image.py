#!/usr/bin/env python
# -*- coding: utf-8 -*-
"""
AI 图片处理脚本
支持：缩放、裁剪、去背景、去水印、格式转换
"""

# 禁用输出缓冲，确保输出能实时显示
import sys
import os

# 设置无缓冲输出
if hasattr(sys.stdout, 'reconfigure'):
    sys.stdout.reconfigure(line_buffering=True)
    sys.stderr.reconfigure(line_buffering=True)

import argparse
from PIL import Image
import numpy as np

try:
    from rembg import remove
    REMBG_AVAILABLE = True
    print("rembg library loaded successfully")
except ImportError as e:
    REMBG_AVAILABLE = False
    print(f"Warning: rembg library not installed. Error: {e}")
    print("Please install rembg: pip install rembg")
except Exception as e:
    REMBG_AVAILABLE = False
    print(f"Warning: Failed to load rembg library. Error: {e}")

try:
    import cv2
    CV2_AVAILABLE = True
except ImportError:
    CV2_AVAILABLE = False
    print("Warning: OpenCV library not installed. Inpaint feature will be disabled.")


def center_crop(image, target_width, target_height):
    """
    中心裁剪图片
    """
    # 确保保持原始模式（特别是 RGBA）
    original_mode = image.mode
    width, height = image.size
    
    # 计算裁剪区域
    left = (width - target_width) // 2
    top = (height - target_height) // 2
    right = left + target_width
    bottom = top + target_height
    
    # 如果目标尺寸大于原图，先缩放
    if target_width > width or target_height > height:
        # 计算缩放比例，取较大的比例以确保能覆盖目标尺寸
        scale = max(target_width / width, target_height / height)
        new_width = int(width * scale)
        new_height = int(height * scale)
        image = image.resize((new_width, new_height), Image.Resampling.LANCZOS)
        width, height = image.size
        left = (width - target_width) // 2
        top = (height - target_height) // 2
        right = left + target_width
        bottom = top + target_height
    
    cropped = image.crop((left, top, right, bottom))
    # 确保保持原始模式
    if cropped.mode != original_mode:
        cropped = cropped.convert(original_mode)
    return cropped


def uniform_scale(image, target_width, target_height):
    """
    等比缩放图片
    """
    # 确保保持原始模式（特别是 RGBA）
    original_mode = image.mode
    width, height = image.size
    
    # 计算缩放比例，保持宽高比
    scale = min(target_width / width, target_height / height)
    new_width = int(width * scale)
    new_height = int(height * scale)
    
    scaled = image.resize((new_width, new_height), Image.Resampling.LANCZOS)
    # 确保保持原始模式
    if scaled.mode != original_mode:
        scaled = scaled.convert(original_mode)
    return scaled


def stretch_resize(image, target_width, target_height):
    """
    拉伸图片到目标尺寸
    """
    # 确保保持原始模式（特别是 RGBA）
    original_mode = image.mode
    stretched = image.resize((target_width, target_height), Image.Resampling.LANCZOS)
    # 确保保持原始模式
    if stretched.mode != original_mode:
        stretched = stretched.convert(original_mode)
    return stretched


def remove_background(image):
    """
    使用 rembg 去除背景
    """
    if not REMBG_AVAILABLE:
        print("Error: rembg library not available. Cannot remove background.")
        return image
    
    try:
        print("Starting background removal with rembg...")
        from io import BytesIO
        
        # 将 PIL Image 转换为字节
        img_bytes = BytesIO()
        # rembg 需要 RGB 格式的输入（不支持 RGBA）
        # 如果输入是 RGBA，需要先转换为 RGB
        # 注意：rembg 会识别图片中的背景区域，所以我们需要确保背景是明显的
        if image.mode == 'RGBA':
            # 对于 RGBA 图片，我们需要移除 alpha 通道
            # 创建一个白色背景的 RGB 图片（白色背景更容易被 rembg 识别为背景）
            print(f"Converting RGBA to RGB with white background for rembg processing...")
            rgb_image = Image.new('RGB', image.size, (255, 255, 255))
            # 将 RGBA 图片合成到白色背景上（透明部分会变成白色）
            rgb_image.paste(image, mask=image.split()[3])
            image_to_process = rgb_image
        elif image.mode != 'RGB':
            print(f"Converting {image.mode} to RGB for rembg processing...")
            image_to_process = image.convert('RGB')
        else:
            print(f"Using RGB image directly for rembg processing...")
            image_to_process = image
            
        image_to_process.save(img_bytes, format='PNG')
        img_bytes.seek(0)
        input_bytes = img_bytes.read()
        
        print(f"Input image size: {len(input_bytes)} bytes")
        print(f"Input image mode: {image_to_process.mode}")
        print(f"Original image mode: {image.mode}")
        
        # 使用 rembg 去除背景
        # rembg 的 remove 函数接受字节数据并返回去除背景后的字节数据
        print("Calling rembg.remove()...")
        output_bytes = remove(input_bytes)
        print(f"Output image size: {len(output_bytes)} bytes")
        
        # 将结果转换回 PIL Image
        result_image = Image.open(BytesIO(output_bytes))
        print(f"Background removed successfully. Result size: {result_image.size}")
        print(f"Result image mode before conversion: {result_image.mode}")
        
        # rembg 返回的图片应该是 RGBA 格式，确保透明通道存在
        if result_image.mode != 'RGBA':
            print(f"Converting from {result_image.mode} to RGBA...")
            result_image = result_image.convert('RGBA')
        
        # 验证 alpha 通道是否存在
        if result_image.mode == 'RGBA':
            alpha_channel = result_image.split()[3]
            # 检查是否有透明像素
            alpha_array = np.array(alpha_channel)
            transparent_pixels = np.sum(alpha_array < 255)
            total_pixels = alpha_array.size
            transparent_percent = transparent_pixels * 100 / total_pixels if total_pixels > 0 else 0
            print(f"Transparent pixels: {transparent_pixels}/{total_pixels} ({transparent_percent:.1f}%)")
            
            # 检查 alpha 通道的值范围
            alpha_min = alpha_array.min()
            alpha_max = alpha_array.max()
            alpha_mean = alpha_array.mean()
            print(f"Alpha channel range: {alpha_min} - {alpha_max}, mean: {alpha_mean:.1f}")
            
            # 如果透明像素很少，可能是 rembg 没有正确识别背景
            if transparent_percent < 1.0:
                print("WARNING: Very few transparent pixels detected. rembg may not have removed the background correctly.")
        else:
            print(f"WARNING: Result image is not RGBA mode: {result_image.mode}")
        
        print(f"Final image mode: {result_image.mode}")
        return result_image
    except Exception as e:
        print(f"Error removing background: {e}")
        import traceback
        traceback.print_exc()
        print("Returning original image without background removal")
        return image


def inpaint_watermark(image):
    """
    使用 OpenCV 去除水印（简单实现）
    假设水印在图片右下角或通过颜色检测
    保持图片的原始模式（特别是RGBA）
    """
    if not CV2_AVAILABLE:
        print("Error: OpenCV library not available. Cannot inpaint watermark.")
        return image
    
    try:
        # 保存原始模式
        original_mode = image.mode
        print(f"Inpainting watermark - Original mode: {original_mode}")
        
        # 如果图片是RGBA，需要先处理RGB通道，然后保留alpha通道
        if original_mode == 'RGBA':
            # 分离RGB和Alpha通道
            rgb_image = image.convert('RGB')
            alpha_channel = image.split()[3]
            
            # 将RGB转换为OpenCV格式
            img_array = np.array(rgb_image)
            img_cv = cv2.cvtColor(img_array, cv2.COLOR_RGB2BGR)
        else:
            # 非RGBA图片，直接转换
            img_array = np.array(image.convert('RGB'))
            img_cv = cv2.cvtColor(img_array, cv2.COLOR_RGB2BGR)
            alpha_channel = None
        
        # 创建掩码（检测水印区域）
        # 方法1: 检测右下角区域（假设水印在右下角）
        height, width = img_cv.shape[:2]
        mask = np.zeros((height, width), dtype=np.uint8)
        
        # 检测右下角区域（可根据实际情况调整）
        watermark_region_height = int(height * 0.1)
        watermark_region_width = int(width * 0.3)
        mask[height - watermark_region_height:, width - watermark_region_width:] = 255
        
        # 方法2: 通过颜色检测（检测接近白色或半透明的区域）
        # 这里简化处理，实际应用中可以根据具体水印特征调整
        gray = cv2.cvtColor(img_cv, cv2.COLOR_BGR2GRAY)
        _, thresh = cv2.threshold(gray, 240, 255, cv2.THRESH_BINARY)
        mask = cv2.bitwise_or(mask, thresh)
        
        # 使用 OpenCV 的 inpaint 函数
        result = cv2.inpaint(img_cv, mask, 3, cv2.INPAINT_TELEA)
        
        # 转换回 PIL Image
        result_rgb = cv2.cvtColor(result, cv2.COLOR_BGR2RGB)
        result_image = Image.fromarray(result_rgb)
        
        # 如果原图是RGBA，恢复alpha通道
        if original_mode == 'RGBA' and alpha_channel is not None:
            print("Restoring alpha channel after watermark removal")
            result_image = result_image.convert('RGBA')
            # 将alpha通道合并回去
            r, g, b = result_image.split()[:3]
            result_image = Image.merge('RGBA', (r, g, b, alpha_channel))
        
        print(f"Inpainting completed - Final mode: {result_image.mode}")
        return result_image
    except Exception as e:
        print(f"Error inpainting watermark: {e}")
        import traceback
        traceback.print_exc()
        return image


def process_image(input_path, output_path, mode, width, height, format_type, remove_bg, inpaint):
    """
    处理图片的主函数
    """
    try:
        # 读取图片
        print(f"Loading image from: {input_path}")
        # 先打开图片查看原始模式
        temp_image = Image.open(input_path)
        print(f"Original image mode: {temp_image.mode}")
        print(f"Original image format: {temp_image.format}")
        
        # 转换为 RGBA（如果原图没有透明通道，alpha 通道会是 255）
        image = temp_image.convert('RGBA')
        original_size = image.size
        print(f"Original size: {original_size[0]}x{original_size[1]}")
        print(f"Converted to RGBA mode: {image.mode}")
        
        # 去背景（如果启用）
        if remove_bg:
            print("=" * 50)
            print("BACKGROUND REMOVAL ENABLED")
            print("=" * 50)
            image = remove_background(image)
            print("=" * 50)
        
        # 去水印（如果启用）
        if inpaint:
            print("Removing watermark...")
            image = inpaint_watermark(image)
        
        # 根据模式处理图片
        print(f"Processing mode: {mode}, Target size: {width}x{height}")
        if mode == 'centercrop':
            image = center_crop(image, width, height)
        elif mode == 'uniformscale':
            image = uniform_scale(image, width, height)
        elif mode == 'stretch':
            image = stretch_resize(image, width, height)
        else:
            print(f"Unknown mode: {mode}, using center crop")
            image = center_crop(image, width, height)
        
        print(f"Processed size: {image.size[0]}x{image.size[1]}")
        
        # 保存图片
        print(f"Saving image to: {output_path}")
        
        # 确保输出目录存在
        output_dir = os.path.dirname(output_path)
        if output_dir and not os.path.exists(output_dir):
            os.makedirs(output_dir)
        
        # 根据格式保存
        if format_type == 'png':
            # 确保 PNG 格式正确保存透明通道
            print(f"Saving PNG - Current image mode: {image.mode}")
            if image.mode == 'RGBA':
                # 直接保存 RGBA，保留透明通道
                # 使用 compress_level=1 确保透明通道正确保存
                image.save(output_path, 'PNG', compress_level=1)
                print(f"Saved as RGBA PNG with transparency")
                
                # 验证保存的文件
                verify_image = Image.open(output_path)
                print(f"Verified saved image mode: {verify_image.mode}")
                if verify_image.mode == 'RGBA':
                    verify_alpha = np.array(verify_image.split()[3])
                    verify_transparent = np.sum(verify_alpha < 255)
                    verify_total = verify_alpha.size
                    print(f"Verified transparent pixels in saved file: {verify_transparent}/{verify_total} ({verify_transparent*100/verify_total:.1f}%)")
            else:
                # 如果不是 RGBA，转换为 RGBA 再保存
                print(f"Converting {image.mode} to RGBA before saving...")
                rgba_image = image.convert('RGBA')
                rgba_image.save(output_path, 'PNG', compress_level=1)
                print(f"Converted to RGBA and saved with transparency")
        elif format_type == 'jpg' or format_type == 'jpeg':
            # JPG 不支持透明通道，需要转换为 RGB
            if image.mode == 'RGBA':
                rgb_image = Image.new('RGB', image.size, (255, 255, 255))
                rgb_image.paste(image, mask=image.split()[3])  # 使用 alpha 通道作为掩码
                image = rgb_image
            image.save(output_path, 'JPEG', quality=90, optimize=True)
        elif format_type == 'webp':
            image.save(output_path, 'WEBP', quality=90, method=6)
        else:
            image.save(output_path, 'PNG', optimize=True)
        
        print("Image processing completed successfully!")
        return True
        
    except Exception as e:
        print(f"Error processing image: {e}")
        import traceback
        traceback.print_exc()
        return False


def main():
    parser = argparse.ArgumentParser(description='Process AI-generated images')
    parser.add_argument('--input', required=True, help='Input image path')
    parser.add_argument('--output', required=True, help='Output image path')
    parser.add_argument('--mode', default='centercrop', 
                       choices=['centercrop', 'uniformscale', 'stretch'],
                       help='Processing mode')
    parser.add_argument('--width', type=int, required=True, help='Target width')
    parser.add_argument('--height', type=int, required=True, help='Target height')
    parser.add_argument('--format', default='png', 
                       choices=['png', 'jpg', 'jpeg', 'webp'],
                       help='Output format')
    parser.add_argument('--remove-bg', action='store_true', 
                       help='Remove background using rembg')
    parser.add_argument('--inpaint', action='store_true', 
                       help='Remove watermark using inpaint')
    
    args = parser.parse_args()
    
    # 检查输入文件是否存在
    if not os.path.exists(args.input):
        print(f"Error: Input file not found: {args.input}")
        sys.exit(1)
    
    # 处理图片
    success = process_image(
        args.input,
        args.output,
        args.mode,
        args.width,
        args.height,
        args.format,
        args.remove_bg,
        args.inpaint
    )
    
    sys.exit(0 if success else 1)


if __name__ == '__main__':
    main()

