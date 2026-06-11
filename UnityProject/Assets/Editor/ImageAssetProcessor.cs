using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;
using System.Text;

namespace GameLogic.Editor
{
    /// <summary>
    /// 图片资产处理器 - Unity Editor 窗口
    /// 用于处理图片资产（去背景/缩放/裁剪/格式转换）
    /// </summary>
    public class ImageAssetProcessor : EditorWindow
    {
        private Texture2D previewTexture;
        private string inputImagePath = "";
        private string outputName = "processed_image";
        private ExportFormat exportFormat = ExportFormat.PNG;
        private ProcessMode processMode = ProcessMode.UniformScale; // 默认等比缩放
        private Vector2Int targetSize = new Vector2Int(200, 200);
        // 处理选项开关
        private bool enableFormatChange = true;
        private bool enableResize = true;
        private bool enableRemBg = true;
        private bool enableInpaint = false;
        
        // 原图信息
        private string originalImageFormat = "";
        private Vector2Int originalImageSize = Vector2Int.zero;
        
        private Vector2 scrollPosition;
        private Vector2 outputScrollPosition;
        private const float PREVIEW_SIZE = 300f;
        private const float DRAG_AREA_HEIGHT = 150f;
        private Vector2Int currentImageSize = Vector2Int.zero;
        private System.Collections.Generic.List<string> pythonOutput = new System.Collections.Generic.List<string>();
        private bool showOutputLog = true; // 默认展开
        
        // 异步处理相关
        private Process currentProcess = null;
        private System.Text.StringBuilder processOutput = new System.Text.StringBuilder();
        private System.Text.StringBuilder processError = new System.Text.StringBuilder();
        private bool isProcessing = false;
        private string currentProgressMessage = "";
        private float currentProgress = 0f;
        private string outputPath = ""; // 临时存储输出路径，用于异步处理
        
        // 常量
        private static readonly string[] ERROR_KEYWORDS = { "error", "exception", "traceback", "no module", "failed", "cannot" };
        private static readonly string[] IMAGE_EXTENSIONS = { ".png", ".jpg", ".jpeg", ".webp" };
        
        private enum ExportFormat
        {
            PNG,
            JPG,
            WebP
        }
        
        private enum ProcessMode
        {
            CenterCrop,    // 中心裁剪
            UniformScale,  // 等比缩放
            Stretch        // 拉伸
        }
        
        [MenuItem("PirateCat/图片处理工具")]
        public static void ShowWindow()
        {
            ImageAssetProcessor window = GetWindow<ImageAssetProcessor>("Image Asset Processor");
            window.minSize = new Vector2(650, 550);
            window.Show();
        }
        
        private void OnGUI()
        {
            // 如果正在处理，更新进度
            if (isProcessing && currentProcess != null)
            {
                UpdateProcessProgress();
            }
            
            EditorGUILayout.BeginVertical();
            
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));
            
            // 左侧区域：拖拽区和预览
            DrawLeftPanel();
            
            // 右侧区域：参数设置
            DrawRightPanel();
            
            EditorGUILayout.EndHorizontal();
            
            // 输出日志区域（占据整个窗口宽度）
            DrawOutputLogPanel();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawLeftPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(350));
            
            // 拖拽区域
            EditorGUILayout.LabelField("Drag & Drop Image Here (拖拽图片到这里)", EditorStyles.boldLabel);
            Rect dragArea = GUILayoutUtility.GetRect(0, DRAG_AREA_HEIGHT, GUILayout.ExpandWidth(true));
            GUI.Box(dragArea, string.IsNullOrEmpty(inputImagePath) ? "Drop image file here (拖拽图片文件到这里)" : Path.GetFileName(inputImagePath), EditorStyles.helpBox);
            
            HandleDragAndDrop(dragArea);
            
            // 显示文件路径
            if (!string.IsNullOrEmpty(inputImagePath))
            {
                EditorGUILayout.HelpBox($"Selected (已选择): {inputImagePath}", MessageType.Info);
            }
            
            EditorGUILayout.Space(10);
            
            // 预览区域
            EditorGUILayout.LabelField("Preview (预览)", EditorStyles.boldLabel);
            if (previewTexture != null)
            {
                Rect previewRect = GUILayoutUtility.GetRect(PREVIEW_SIZE, PREVIEW_SIZE);
                
                // 计算等比缩放的显示区域
                Rect displayRect = CalculateAspectRatioRect(previewRect, previewTexture.width, previewTexture.height);
                
                // 绘制背景框
                EditorGUI.DrawRect(previewRect, new Color(0.2f, 0.2f, 0.2f, 1f));
                
                // 绘制预览图片（保持比例）
                GUI.DrawTexture(displayRect, previewTexture, ScaleMode.ScaleToFit, true);
                
                // 显示图片尺寸
                if (currentImageSize.x > 0 && currentImageSize.y > 0)
                {
                    EditorGUILayout.LabelField($"Size (尺寸): {currentImageSize.x} x {currentImageSize.y}", EditorStyles.miniLabel);
                }
            }
            else
            {
                GUILayout.Box("No preview (无预览)", GUILayout.Height(PREVIEW_SIZE), GUILayout.Width(PREVIEW_SIZE));
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawRightPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            EditorGUILayout.LabelField("Settings (设置)", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            // Output Name
            EditorGUILayout.LabelField("Output Name (输出名称):");
            outputName = EditorGUILayout.TextField(outputName);
            
            EditorGUILayout.Space(10);
            
            // Export Format
            EditorGUILayout.BeginHorizontal();
            enableFormatChange = EditorGUILayout.Toggle(enableFormatChange, GUILayout.Width(15));
            EditorGUILayout.LabelField("Export Format (导出格式):", GUILayout.Width(150));
            GUI.enabled = enableFormatChange;
            exportFormat = DrawEnumPopupWithChinese(exportFormat, new string[] { "PNG", "JPG", "WebP" });
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            if (!enableFormatChange && !string.IsNullOrEmpty(originalImageFormat))
            {
                EditorGUILayout.HelpBox($"Will use original format: {originalImageFormat}", MessageType.Info);
            }
            
            EditorGUILayout.Space(10);
            
            // Process Mode
            EditorGUILayout.BeginHorizontal();
            enableResize = EditorGUILayout.Toggle(enableResize, GUILayout.Width(15));
            EditorGUILayout.LabelField("Process Mode (处理模式):", GUILayout.Width(150));
            GUI.enabled = enableResize;
            processMode = DrawEnumPopupWithChinese(processMode, new string[] { "CenterCrop (中心裁剪)", "UniformScale (等比缩放)", "Stretch (拉伸)" });
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            if (!enableResize && originalImageSize.x > 0 && originalImageSize.y > 0)
            {
                EditorGUILayout.HelpBox($"Will keep original size: {originalImageSize.x} x {originalImageSize.y}", MessageType.Info);
            }
            
            EditorGUILayout.Space(10);
            
            // Target Size
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(15); // 对齐开关位置
            EditorGUILayout.LabelField("Target Size (目标尺寸):", GUILayout.Width(150));
            GUI.enabled = enableResize;
            targetSize = EditorGUILayout.Vector2IntField("", targetSize);
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            
            // AI Options
            EditorGUILayout.LabelField("AI Options (AI 选项):", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            enableRemBg = EditorGUILayout.Toggle(enableRemBg, GUILayout.Width(15));
            EditorGUILayout.LabelField("RemBg (去背景)", GUILayout.Width(150));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            enableInpaint = EditorGUILayout.Toggle(enableInpaint, GUILayout.Width(15));
            EditorGUILayout.LabelField("Inpaint (去水印)", GUILayout.Width(150));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(20);
            
            // 执行按钮（在所有选项下方）
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            GUI.enabled = !string.IsNullOrEmpty(inputImagePath) && File.Exists(inputImagePath);
            if (GUILayout.Button("Execute Process (执行处理)", GUILayout.Width(250), GUILayout.Height(35)))
            {
                pythonOutput.Clear(); // 清除旧的输出
                ProcessImage();
            }
            GUI.enabled = true;
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawOutputLogPanel()
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // 折叠/展开按钮
            showOutputLog = EditorGUILayout.Foldout(showOutputLog, $"Python Output Log (Python 输出日志) ({pythonOutput.Count} lines)", true);
            
            if (showOutputLog && pythonOutput.Count > 0)
            {
                EditorGUILayout.Space(5);
                outputScrollPosition = EditorGUILayout.BeginScrollView(outputScrollPosition, GUILayout.Height(250));
                
                foreach (string line in pythonOutput)
                {
                    EditorGUILayout.SelectableLabel(line, EditorStyles.textArea, GUILayout.Height(18));
                }
                
                EditorGUILayout.EndScrollView();
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Clear Log (清除日志)", GUILayout.Width(120)))
                {
                    pythonOutput.Clear();
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
            else if (showOutputLog && pythonOutput.Count == 0)
            {
                EditorGUILayout.HelpBox("No output yet. Process an image to see Python output here.", MessageType.Info);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void HandleDragAndDrop(Rect dropArea)
        {
            Event currentEvent = Event.current;
            
            if (currentEvent.type == EventType.DragUpdated || currentEvent.type == EventType.DragPerform)
            {
                if (dropArea.Contains(currentEvent.mousePosition))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    
                    if (currentEvent.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        
                        foreach (string path in DragAndDrop.paths)
                        {
                            if (IsImageFile(path))
                            {
                                inputImagePath = path;
                                // 更新输出名称为输入文件名（不含扩展名）
                                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(path);
                                outputName = fileNameWithoutExt;
                                LoadPreview(path);
                                break;
                            }
                        }
                        
                        currentEvent.Use();
                    }
                }
            }
        }
        
        private bool IsImageFile(string path)
        {
            string ext = Path.GetExtension(path).ToLower();
            return System.Array.IndexOf(IMAGE_EXTENSIONS, ext) >= 0;
        }
        
        private string GetFormatFromExtension(string extension)
        {
            switch (extension.ToLower())
            {
                case ".png":
                    return "PNG";
                case ".jpg":
                case ".jpeg":
                    return "JPG";
                case ".webp":
                    return "WebP";
                default:
                    return "PNG";
            }
        }
        
        private void LoadPreview(string path)
        {
            if (previewTexture != null)
            {
                DestroyImmediate(previewTexture);
            }
            
            byte[] imageData = File.ReadAllBytes(path);
            previewTexture = new Texture2D(2, 2);
            previewTexture.LoadImage(imageData);
            
            // 记录当前图片尺寸和格式
            if (previewTexture != null)
            {
                currentImageSize = new Vector2Int(previewTexture.width, previewTexture.height);
                originalImageSize = currentImageSize;
                originalImageFormat = GetFormatFromExtension(Path.GetExtension(path));
            }
        }
        
        private void ProcessImage()
        {
            if (string.IsNullOrEmpty(inputImagePath) || !File.Exists(inputImagePath))
            {
                EditorUtility.DisplayDialog("Error", "Please select a valid image file.", "OK");
                return;
            }
            
            // 查找 Python 可执行文件
            string pythonPath = FindPythonExecutable();
            if (string.IsNullOrEmpty(pythonPath))
            {
                EditorUtility.DisplayDialog("Error", 
                    "Python not found! Please install Python and add it to PATH, or specify the path manually.", 
                    "OK");
                return;
            }
            
            // 获取 Python 脚本路径
            string scriptPath = Path.Combine(Application.dataPath, "Editor", "PythonTools", "process_image.py");
            if (!File.Exists(scriptPath))
            {
                EditorUtility.DisplayDialog("Error", 
                    $"Python script not found at: {scriptPath}\nPlease ensure process_image.py exists.", 
                    "OK");
                return;
            }
            
            // 准备输出路径 - 与输入图片路径相同
            string inputDir = Path.GetDirectoryName(inputImagePath);
            
            // 根据开关决定输出格式
            ExportFormat finalFormat = enableFormatChange ? exportFormat : GetFormatFromString(originalImageFormat);
            string outputExtension = GetExtensionFromFormat(finalFormat);
            string outputFileName = $"{outputName}{outputExtension}";
            string outputPath = Path.Combine(inputDir, outputFileName);
            
            // 确保输出目录存在
            if (!Directory.Exists(inputDir))
            {
                Directory.CreateDirectory(inputDir);
            }
            
            // 根据开关决定目标尺寸
            Vector2Int finalSize = enableResize ? targetSize : originalImageSize;
            if (finalSize.x <= 0 || finalSize.y <= 0)
            {
                finalSize = originalImageSize;
            }
            
            // 构建命令行参数
            StringBuilder args = new StringBuilder();
            args.Append($"\"{scriptPath}\" ");
            args.Append($"--input \"{inputImagePath}\" ");
            args.Append($"--output \"{outputPath}\" ");
            
            // 如果启用尺寸调整，应用处理模式；否则使用 uniformscale 保持原尺寸
            if (enableResize)
            {
                args.Append($"--mode {processMode.ToString().ToLower()} ");
            }
            else
            {
                args.Append($"--mode uniformscale "); // 保持原尺寸
            }
            
            args.Append($"--width {finalSize.x} ");
            args.Append($"--height {finalSize.y} ");
            args.Append($"--format {finalFormat.ToString().ToLower()} ");
            
            // 根据开关决定是否应用去背景
            if (enableRemBg)
            {
                args.Append("--remove-bg ");
            }
            
            // 根据开关决定是否应用去水印
            if (enableInpaint)
            {
                args.Append("--inpaint ");
            }
            
            // 执行 Python 脚本
            UnityEngine.Debug.Log($"[ImageAssetProcessor] Executing: {pythonPath} {args.ToString()}");
            
            // 构建完整的命令行参数，添加 -u 参数禁用输出缓冲
            string fullArgs = $"-u {args.ToString()}";
            
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = pythonPath,
                Arguments = fullArgs,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            
            // 设置环境变量，禁用 Python 输出缓冲
            startInfo.EnvironmentVariables["PYTHONUNBUFFERED"] = "1";
            
            // 保存输出路径供异步处理使用
            this.outputPath = outputPath;
            
            // 开始异步处理
            StartAsyncProcess(startInfo, outputPath);
        }
        
        
        private void StartAsyncProcess(ProcessStartInfo startInfo, string outputPath)
        {
            try
            {
                isProcessing = true;
                currentProgress = 0f;
                currentProgressMessage = "Starting Python process...";
                processOutput.Clear();
                processError.Clear();
                
                // 启动进程
                currentProcess = Process.Start(startInfo);
                
                // 注册更新回调
                EditorApplication.update += UpdateProcessProgress;
                
                // 异步读取标准输出
                currentProcess.OutputDataReceived += (sender, e) => {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        string line = e.Data;
                        lock (processOutput)
                        {
                            processOutput.AppendLine(line);
                        }
                        
                        // 实时添加到窗口输出列表
                        string capturedLine = line;
                        EditorApplication.delayCall += () => {
                            pythonOutput.Add($"[OUT] {capturedLine}");
                            Repaint();
                        };
                        
                        // 更新进度信息（根据输出内容判断进度）
                        UpdateProgressFromOutput(line);
                        
                        // 实时输出到 Unity 控制台
                        UnityEngine.Debug.Log($"[Python] {line}");
                    }
                };
                
                // 异步读取标准错误
                currentProcess.ErrorDataReceived += (sender, e) => {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        string line = e.Data;
                        lock (processError)
                        {
                            processError.AppendLine(line);
                        }
                        
                        // 实时添加到窗口输出列表
                        string capturedLine = line;
                        bool isError = IsErrorLine(line);
                        EditorApplication.delayCall += () => {
                            pythonOutput.Add(isError ? $"[ERROR] {capturedLine}" : $"[WARN] {capturedLine}");
                            Repaint();
                        };
                        
                        // 实时输出到 Unity 控制台
                        if (isError)
                            UnityEngine.Debug.LogError($"[Python Error] {line}");
                        else
                            UnityEngine.Debug.LogWarning($"[Python Warning] {line}");
                    }
                };
                
                // 开始异步读取
                currentProcess.BeginOutputReadLine();
                currentProcess.BeginErrorReadLine();
            }
            catch (System.Exception ex)
            {
                isProcessing = false;
                EditorApplication.update -= UpdateProcessProgress;
                EditorUtility.ClearProgressBar();
                UnityEngine.Debug.LogError($"[ImageAssetProcessor] Exception: {ex.Message}");
                EditorUtility.DisplayDialog("Error", 
                    $"Failed to execute Python script:\n{ex.Message}", 
                    "OK");
            }
        }
        
        private void UpdateProcessProgress()
        {
            if (currentProcess == null)
                return;
                
            if (currentProcess.HasExited)
            {
                FinishProcess(this.outputPath);
                return;
            }
            
            // 更新进度条
            EditorUtility.DisplayProgressBar("Processing Image (处理图片)", 
                currentProgressMessage, 
                currentProgress);
            
            // 强制刷新窗口以显示实时日志
            Repaint();
        }
        
        private void UpdateProgressFromOutput(string line)
        {
            string lineLower = line.ToLower();
            
            // 根据输出内容更新进度
            if (lineLower.Contains("loading image"))
            {
                currentProgressMessage = "Loading image... (加载图片...)";
                currentProgress = 0.1f;
            }
            else if (lineLower.Contains("removing background") || lineLower.Contains("background removal"))
            {
                currentProgressMessage = "Removing background... (去除背景...)";
                currentProgress = 0.3f;
            }
            else if (lineLower.Contains("removing watermark") || lineLower.Contains("inpainting"))
            {
                currentProgressMessage = "Removing watermark... (去除水印...)";
                currentProgress = 0.5f;
            }
            else if (lineLower.Contains("processing mode") || lineLower.Contains("resize") || lineLower.Contains("crop"))
            {
                currentProgressMessage = "Resizing/Cropping image... (调整尺寸/裁剪...)";
                currentProgress = 0.7f;
            }
            else if (lineLower.Contains("saving image"))
            {
                currentProgressMessage = "Saving image... (保存图片...)";
                currentProgress = 0.9f;
            }
            else if (lineLower.Contains("completed successfully"))
            {
                currentProgressMessage = "Processing completed! (处理完成!)";
                currentProgress = 1.0f;
            }
        }
        
        private void FinishProcess(string outputPath)
        {
            // 移除更新回调
            EditorApplication.update -= UpdateProcessProgress;
            
            // 清除进度条
            EditorUtility.ClearProgressBar();
            
            isProcessing = false;
            
            // 获取最终输出
            string output, error;
            lock (processOutput)
            {
                output = processOutput.ToString();
            }
            lock (processError)
            {
                error = processError.ToString();
            }
            
            int exitCode = currentProcess != null ? currentProcess.ExitCode : -1;
            
            // 清理进程
            if (currentProcess != null)
            {
                currentProcess.Close();
                currentProcess.Dispose();
                currentProcess = null;
            }
            
            // 输出汇总信息
            if (!string.IsNullOrEmpty(output))
            {
                UnityEngine.Debug.Log($"[Python Output Summary]\n{output}");
            }
            
            if (!string.IsNullOrEmpty(error))
            {
                UnityEngine.Debug.LogError($"[Python Error Summary]\n{error}");
            }
            
            // 处理结果
            if (exitCode == 0)
            {
                if (File.Exists(outputPath))
                {
                    AssetDatabase.Refresh();
                    EditorUtility.DisplayDialog("Success", 
                        $"Image processed successfully!\nOutput: {outputPath}", 
                        "OK");
                    
                    // 更新预览
                    LoadPreview(outputPath);
                }
                else
                {
                    EditorUtility.DisplayDialog("Warning", 
                        "Process completed but output file not found.", 
                        "OK");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Error", 
                    $"Process failed with exit code: {exitCode}\nCheck console for details.", 
                    "OK");
            }
        }
        
        
        private string FindPythonExecutable()
        {
            string pyenvPython = FindPythonFromPyenv();
            if (!string.IsNullOrEmpty(pyenvPython))
            {
                UnityEngine.Debug.Log($"[ImageAssetProcessor] Found Python via pyenv: {pyenvPython}");
                return pyenvPython;
            }

            // 常见的 Python 路径（作为回退）
            string[] possiblePaths = new string[]
            {
                "python",
                "python3",
                @"C:\Python39\python.exe",
                @"C:\Python310\python.exe",
                @"C:\Python311\python.exe",
                @"C:\Python312\python.exe",
                @"C:\Program Files\Python39\python.exe",
                @"C:\Program Files\Python310\python.exe",
                @"C:\Program Files\Python311\python.exe",
                @"C:\Program Files\Python312\python.exe",
                @"C:\Users\" + System.Environment.UserName + @"\AppData\Local\Programs\Python\Python39\python.exe",
                @"C:\Users\" + System.Environment.UserName + @"\AppData\Local\Programs\Python\Python310\python.exe",
                @"C:\Users\" + System.Environment.UserName + @"\AppData\Local\Programs\Python\Python311\python.exe",
                @"C:\Users\" + System.Environment.UserName + @"\AppData\Local\Programs\Python\Python312\python.exe"
            };
            
            foreach (string path in possiblePaths)
            {
                if (!CanRunPython(path))
                {
                    continue;
                }

                UnityEngine.Debug.Log($"[ImageAssetProcessor] Found Python at: {path}");
                return path;
            }
            
            return null;
        }

        private string FindPythonFromPyenv()
        {
            string pyenvRoot = GetPyenvRoot();
            if (string.IsNullOrEmpty(pyenvRoot))
            {
                return null;
            }

            string pyenvBat = Path.Combine(pyenvRoot, "bin", "pyenv.bat");
            string pyenvWhichPython = TryGetPyenvWhichPython(pyenvBat);
            if (!string.IsNullOrEmpty(pyenvWhichPython) && CanRunPython(pyenvWhichPython))
            {
                return pyenvWhichPython;
            }

            string[] pyenvCandidates = new string[]
            {
                Path.Combine(pyenvRoot, "shims", "python.bat"),
                Path.Combine(pyenvRoot, "shims", "python.exe"),
            };

            foreach (string candidate in pyenvCandidates)
            {
                if (!CanRunPython(candidate))
                {
                    continue;
                }

                return candidate;
            }

            return null;
        }

        private string GetPyenvRoot()
        {
            string[] envKeys = { "PYENV_ROOT", "PYENV", "PYENV_HOME" };
            foreach (string envKey in envKeys)
            {
                string root = System.Environment.GetEnvironmentVariable(envKey);
                if (!string.IsNullOrEmpty(root) && Directory.Exists(root))
                {
                    return root;
                }
            }

            string userHome = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
            if (string.IsNullOrEmpty(userHome))
            {
                return null;
            }

            string defaultPyenvRoot = Path.Combine(userHome, ".pyenv", "pyenv-win");
            if (Directory.Exists(defaultPyenvRoot))
            {
                return defaultPyenvRoot;
            }

            return null;
        }

        private string TryGetPyenvWhichPython(string pyenvBatPath)
        {
            if (string.IsNullOrEmpty(pyenvBatPath) || !File.Exists(pyenvBatPath))
            {
                return null;
            }

            try
            {
                ProcessStartInfo startInfo = CreateProcessStartInfo(pyenvBatPath, "which python");

                using (Process process = Process.Start(startInfo))
                {
                    if (process == null)
                    {
                        return null;
                    }

                    string stdout = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    if (process.ExitCode != 0)
                    {
                        return null;
                    }

                    string[] lines = stdout.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
                    if (lines.Length == 0)
                    {
                        return null;
                    }

                    string pythonPath = lines[0].Trim().Trim('"');
                    if (File.Exists(pythonPath))
                    {
                        return pythonPath;
                    }
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        private bool CanRunPython(string commandOrPath)
        {
            if (string.IsNullOrEmpty(commandOrPath))
            {
                return false;
            }

            try
            {
                ProcessStartInfo startInfo = CreateProcessStartInfo(commandOrPath, "--version");

                using (Process process = Process.Start(startInfo))
                {
                    if (process == null)
                    {
                        return false;
                    }

                    process.WaitForExit();
                    return process.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private ProcessStartInfo CreateProcessStartInfo(string fileNameOrCommand, string arguments)
        {
            bool isBatchFile = fileNameOrCommand.EndsWith(".bat", System.StringComparison.OrdinalIgnoreCase)
                || fileNameOrCommand.EndsWith(".cmd", System.StringComparison.OrdinalIgnoreCase);

            if (!isBatchFile)
            {
                return new ProcessStartInfo
                {
                    FileName = fileNameOrCommand,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
            }

            return new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"\"{fileNameOrCommand}\" {arguments}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
        }
        
        private string GetExtensionFromFormat(ExportFormat format)
        {
            return format switch
            {
                ExportFormat.PNG => ".png",
                ExportFormat.JPG => ".jpg",
                ExportFormat.WebP => ".webp",
                _ => ".png"
            };
        }
        
        private ExportFormat GetFormatFromString(string formatStr)
        {
            return formatStr.ToUpper() switch
            {
                "PNG" => ExportFormat.PNG,
                "JPG" or "JPEG" => ExportFormat.JPG,
                "WEBP" => ExportFormat.WebP,
                _ => ExportFormat.PNG
            };
        }
        
        private bool IsErrorLine(string line)
        {
            string lineLower = line.ToLower();
            foreach (string keyword in ERROR_KEYWORDS)
            {
                if (lineLower.Contains(keyword))
                    return true;
            }
            return false;
        }
        
        private T DrawEnumPopupWithChinese<T>(T value, string[] displayNames) where T : System.Enum
        {
            int currentIndex = System.Array.IndexOf(System.Enum.GetValues(typeof(T)), value);
            int newIndex = EditorGUILayout.Popup(currentIndex, displayNames);
            return (T)System.Enum.GetValues(typeof(T)).GetValue(newIndex);
        }
        
        /// <summary>
        /// 计算保持宽高比的显示矩形
        /// </summary>
        private Rect CalculateAspectRatioRect(Rect containerRect, int imageWidth, int imageHeight)
        {
            float imageAspect = (float)imageWidth / imageHeight;
            float containerAspect = containerRect.width / containerRect.height;
            
            float displayWidth, displayHeight;
            
            if (imageAspect > containerAspect)
            {
                // 图片更宽，以宽度为准
                displayWidth = containerRect.width;
                displayHeight = containerRect.width / imageAspect;
            }
            else
            {
                // 图片更高，以高度为准
                displayHeight = containerRect.height;
                displayWidth = containerRect.height * imageAspect;
            }
            
            // 居中显示
            float x = containerRect.x + (containerRect.width - displayWidth) * 0.5f;
            float y = containerRect.y + (containerRect.height - displayHeight) * 0.5f;
            
            return new Rect(x, y, displayWidth, displayHeight);
        }
        
        private void OnDestroy()
        {
            // 清理资源
            if (previewTexture != null)
            {
                DestroyImmediate(previewTexture);
            }
            
            // 清理进程和回调
            if (isProcessing)
            {
                EditorApplication.update -= UpdateProcessProgress;
                EditorUtility.ClearProgressBar();
            }
            
            if (currentProcess != null && !currentProcess.HasExited)
            {
                try
                {
                    currentProcess.Kill();
                }
                catch { }
                currentProcess.Dispose();
            }
        }
    }
}

