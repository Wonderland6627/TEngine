# AI Asset Processor - Python Tools

## 简介

这个工具用于处理 AI 生成的图片资产，支持以下功能：

- **缩放和裁剪**：支持中心裁剪、等比缩放、拉伸三种模式
- **去背景**：使用 rembg 库一键去除图片背景
- **去水印**：使用 OpenCV 的 inpaint 功能去除水印
- **格式转换**：支持 PNG、JPG、WebP 格式输出

## 安装依赖

### 1. 确保已安装 Python

推荐使用 Python 3.9 或更高版本。

### 2. 安装依赖库

**方法一：使用安装脚本（推荐）**

```bash
cd Assets/Editor/PythonTools
python install_dependencies.py
```

**方法二：使用 requirements.txt**

```bash
cd Assets/Editor/PythonTools
pip install -r requirements.txt
```

**方法三：手动安装**

```bash
pip install Pillow>=10.0.0 numpy>=1.24.0 onnxruntime>=1.15.0 rembg>=2.0.50 opencv-python>=4.8.0
```

**重要提示**：
- `onnxruntime` 是 rembg 的必需依赖，如果缺少会导致导入失败
- 如果遇到 "No module named 'onnxruntime'" 错误，请先安装：`pip install onnxruntime`

### 3. 注意事项

- **rembg** 首次运行时会自动下载模型文件（约 170MB），请确保网络连接正常
- 如果不需要去背景功能，可以不安装 rembg
- 如果不需要去水印功能，可以不安装 opencv-python

## 使用方法

### 通过 Unity Editor 使用

1. 在 Unity 中打开菜单：`PirateCat > AI Asset Processor`
2. 将图片拖拽到左侧拖拽区域
3. 设置处理参数
4. 点击 "Execute Process" 按钮

### 命令行使用

```bash
python process_image.py \
  --input "input.png" \
  --output "output.png" \
  --mode centercrop \
  --width 200 \
  --height 200 \
  --format png \
  --remove-bg \
  --inpaint
```

## 参数说明

- `--input`: 输入图片路径（必需）
- `--output`: 输出图片路径（必需）
- `--mode`: 处理模式
  - `centercrop`: 中心裁剪
  - `uniformscale`: 等比缩放
  - `stretch`: 拉伸
- `--width`: 目标宽度（必需）
- `--height`: 目标高度（必需）
- `--format`: 输出格式（png/jpg/webp）
- `--remove-bg`: 启用去背景功能
- `--inpaint`: 启用去水印功能

## 故障排除

### Python 未找到

如果 Unity 无法找到 Python，请确保：
1. Python 已添加到系统 PATH 环境变量
2. 或者手动指定 Python 路径（需要修改 C# 代码）

### 库未安装错误

如果提示库未安装：
1. 检查 Python 版本：`python --version`
2. 确认 pip 可用：`pip --version`
3. 重新安装依赖：`pip install -r requirements.txt`

### rembg 模型下载失败

如果 rembg 模型下载失败：
1. 检查网络连接
2. 可以手动下载模型并放置到指定目录
3. 或使用代理

### rembg 去背景功能不生效

如果 rembg 去背景功能没有效果：

1. **测试 rembg 是否正常工作**：
   ```bash
   cd Assets/Editor/PythonTools
   python test_rembg.py
   ```
   如果测试失败，请检查：
   - rembg 是否正确安装：`pip show rembg`
   - Python 版本是否兼容（需要 Python 3.8+）
   - 是否有足够的磁盘空间（模型文件约 170MB）

2. **检查 Unity 控制台输出**：
   - 打开 Unity Console 窗口
   - 查看 `[Python Output]` 和 `[Python Error]` 日志
   - 确认是否有 "rembg library loaded successfully" 消息
   - 确认是否有 "BACKGROUND REMOVAL ENABLED" 消息

3. **常见问题**：
   - **首次使用**：rembg 首次运行时会自动下载模型，可能需要几分钟时间
   - **网络问题**：如果下载失败，可以手动下载模型文件
   - **输出格式**：确保输出格式为 PNG（PNG 支持透明通道，JPG 不支持）
   - **图片格式**：某些图片格式可能不被 rembg 支持，尝试转换为 PNG 后再处理

4. **手动测试 rembg**：
   ```python
   from rembg import remove
   from PIL import Image
   from io import BytesIO
   
   # 读取图片
   with open('test.png', 'rb') as f:
       input_bytes = f.read()
   
   # 去除背景
   output_bytes = remove(input_bytes)
   
   # 保存结果
   with open('output.png', 'wb') as f:
       f.write(output_bytes)
   ```

## 技术说明

- **去背景**：使用 rembg 库，基于深度学习模型
- **去水印**：使用 OpenCV 的 inpaint 算法，当前实现为简化版本（检测右下角区域和白色区域）
- **图片处理**：使用 Pillow 库进行缩放和裁剪操作

## 许可证

请遵循各依赖库的许可证要求。

