using System.Collections.Generic;
using System.IO;
using GameFramework.Runtime;
using UnityEngine;
using YooAsset;

namespace TEngine
{
    internal partial class ResourceManager
    {
        /// <summary>
        /// 远端资源地址查询服务类
        /// 支持 CDN 结构：
        /// - 版本文件从根目录读取：{CDNBaseURL}/Package.version
        /// - Bundle 和清单文件从版本目录读取：{CDNBaseURL}/{version}/{fileName}
        /// </summary>
        private class RemoteServices : IRemoteServices
        {
            private readonly string _defaultHostServer;
            private readonly string _fallbackHostServer;
            private string _currentPackageVersion;
            private readonly string _packageName;
            private readonly string _versionFileName;

            public RemoteServices(string defaultHostServer, string fallbackHostServer, string packageName = "DefaultPackage")
            {
                _defaultHostServer = defaultHostServer.TrimEnd('/');
                _fallbackHostServer = fallbackHostServer.TrimEnd('/');
                _packageName = packageName;
                // 版本文件名格式：PackageManifest_{PackageName}_{BuildVersion}.version
                // 但实际 CDN 上可能是 Package.version
                _versionFileName = YooAssetSettingsData.GetPackageVersionFileName(packageName);
            }

            /// <summary>
            /// 更新当前包版本号（在读取版本文件后调用）
            /// </summary>
            public void UpdatePackageVersion(string version)
            {
                if (!string.IsNullOrEmpty(version) && version != _currentPackageVersion)
                {
                    _currentPackageVersion = version;
                    YooLogger.Log($"[RemoteServices] Updated package version to: {version}");
                }
            }

            /// <summary>
            /// 尝试从 package 获取当前版本号
            /// </summary>
            private string TryGetPackageVersion()
            {
                if (!string.IsNullOrEmpty(_currentPackageVersion))
                {
                    return _currentPackageVersion;
                }

                // 尝试从 YooAssets 获取当前版本号
                try
                {
                    var package = YooAssets.TryGetPackage(_packageName);
                    if (package != null && !string.IsNullOrEmpty(package.PackageVersion))
                    {
                        _currentPackageVersion = package.PackageVersion;
                        return _currentPackageVersion;
                    }
                }
                catch
                {
                    // 忽略错误，返回 null
                }

                return null;
            }

            string IRemoteServices.GetRemoteMainURL(string fileName)
            {
                return GetRemoteURL(_defaultHostServer, fileName);
            }

            string IRemoteServices.GetRemoteFallbackURL(string fileName)
            {
                return GetRemoteURL(_fallbackHostServer, fileName);
            }

            /// <summary>
            /// 根据文件类型构建 URL
            /// </summary>
            private string GetRemoteURL(string baseUrl, string fileName)
            {
                // 判断是否为版本文件
                // 版本文件格式：PackageManifest_{PackageName}_{BuildVersion}.version
                // 或者简化的 Package.version
                if (IsVersionFile(fileName))
                {
                    // 版本文件从根目录读取
                    // 支持两种格式：
                    // 1. PackageManifest_{PackageName}_{BuildVersion}.version -> Package.version
                    // 2. Package.version -> Package.version
                    string versionFileName = "Package.version";
                    return $"{baseUrl}/{versionFileName}";
                }
                else
                {
                    // Bundle 和清单文件从版本目录读取
                    string version = TryGetPackageVersion();
                    if (string.IsNullOrEmpty(version))
                    {
                        // 如果还没有版本号，先尝试从根目录读取（兼容旧逻辑）
                        // 这种情况可能发生在初始化阶段，版本文件还未读取
                        YooLogger.Warning($"[RemoteServices] Package version not available, using root path for file: {fileName}");
                        return $"{baseUrl}/{fileName}";
                    }
                    else
                    {
                        // 从版本目录读取：{baseUrl}/{version}/{fileName}
                        return $"{baseUrl}/{version}/{fileName}";
                    }
                }
            }

            /// <summary>
            /// 判断是否为版本文件
            /// </summary>
            private bool IsVersionFile(string fileName)
            {
                // 检查是否为版本文件
                // 1. 文件名以 .version 结尾
                // 2. 或者文件名匹配 PackageManifest_{PackageName}_*.version 格式
                if (fileName.EndsWith(".version"))
                {
                    // 检查是否匹配标准格式
                    if (fileName == _versionFileName)
                    {
                        return true;
                    }
                    // 也支持简化的 Package.version
                    if (fileName == "Package.version")
                    {
                        return true;
                    }
                    // 检查是否包含 PackageManifest 和 PackageName
                    if (fileName.Contains("PackageManifest") && fileName.Contains(_packageName))
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        
        /// <summary>
        /// 资源文件流加载解密类
        /// </summary>
        private class FileStreamDecryption : IDecryptionServices
        {
            /// <summary>
            /// 同步方式获取解密的资源包对象
            /// 注意：加载流对象在资源包对象释放的时候会自动释放
            /// </summary>
            AssetBundle IDecryptionServices.LoadAssetBundle(DecryptFileInfo fileInfo, out Stream managedStream)
            {
                BundleStream bundleStream = new BundleStream(fileInfo.FileLoadPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                managedStream = bundleStream;
                return AssetBundle.LoadFromStream(bundleStream, fileInfo.ConentCRC, GetManagedReadBufferSize());
            }

            /// <summary>
            /// 异步方式获取解密的资源包对象
            /// 注意：加载流对象在资源包对象释放的时候会自动释放
            /// </summary>
            AssetBundleCreateRequest IDecryptionServices.LoadAssetBundleAsync(DecryptFileInfo fileInfo, out Stream managedStream)
            {
                BundleStream bundleStream = new BundleStream(fileInfo.FileLoadPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                managedStream = bundleStream;
                return AssetBundle.LoadFromStreamAsync(bundleStream, fileInfo.ConentCRC, GetManagedReadBufferSize());
            }

            private static uint GetManagedReadBufferSize()
            {
                return 1024;
            }
        }

        /// <summary>
        /// 资源文件偏移加载解密类
        /// </summary>
        private class FileOffsetDecryption : IDecryptionServices
        {
            /// <summary>
            /// 同步方式获取解密的资源包对象
            /// 注意：加载流对象在资源包对象释放的时候会自动释放
            /// </summary>
            AssetBundle IDecryptionServices.LoadAssetBundle(DecryptFileInfo fileInfo, out Stream managedStream)
            {
                managedStream = null;
                return AssetBundle.LoadFromFile(fileInfo.FileLoadPath, fileInfo.ConentCRC, GetFileOffset());
            }

            /// <summary>
            /// 异步方式获取解密的资源包对象
            /// 注意：加载流对象在资源包对象释放的时候会自动释放
            /// </summary>
            AssetBundleCreateRequest IDecryptionServices.LoadAssetBundleAsync(DecryptFileInfo fileInfo, out Stream managedStream)
            {
                managedStream = null;
                return AssetBundle.LoadFromFileAsync(fileInfo.FileLoadPath, fileInfo.ConentCRC, GetFileOffset());
            }

            private static ulong GetFileOffset()
            {
                return 32;
            }
        }
    }

    /// <summary>
    /// 资源文件解密流
    /// </summary>
    public class BundleStream : FileStream
    {
        public const byte KEY = 64;

        public BundleStream(string path, FileMode mode, FileAccess access, FileShare share) : base(path, mode, access, share)
        {
        }

        public BundleStream(string path, FileMode mode) : base(path, mode)
        {
        }

        public override int Read(byte[] array, int offset, int count)
        {
            var index = base.Read(array, offset, count);
            for (int i = 0; i < array.Length; i++)
            {
                array[i] ^= KEY;
            }

            return index;
        }
    }

    /// <summary>
    /// 资源文件查询服务类
    /// </summary>
    public class GameQueryServices : IBuildinQueryServices
    {
        /// <summary>
        /// 查询内置文件的时候，是否比对文件哈希值
        /// </summary>
        public static bool CompareFileCRC = false;

        public bool Query(string packageName, string fileName, string fileCRC)
        {
            // 注意：fileName包含文件格式
            return StreamingAssetsHelper.FileExists(packageName, fileName, fileCRC);
        }
    }
    
    public class StreamingAssetsDefine
    {
        /// <summary>
        /// 根目录名称（保持和YooAssets资源系统一致）
        /// </summary>
        public const string RootFolderName = "package";
    }

#if UNITY_EDITOR
    public sealed class StreamingAssetsHelper
    {
        public static void Init()
        {
        }

        public static bool FileExists(string packageName, string fileName, string fileCRC)
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, StreamingAssetsDefine.RootFolderName, packageName, fileName);
            if (File.Exists(filePath))
            {
                if (GameQueryServices.CompareFileCRC)
                {
                    string crc32 = YooAsset.HashUtility.FileCRC32(filePath);
                    return crc32 == fileCRC;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
    }
#else
public sealed class StreamingAssetsHelper
{
    private class PackageQuery
    {
        public readonly Dictionary<string, BuildinFileManifest.Element> Elements = new Dictionary<string, BuildinFileManifest.Element>(1000);
    }

    private static bool _isInit = false;
    private static readonly Dictionary<string, PackageQuery> _packages = new Dictionary<string, PackageQuery>(10);

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        if (_isInit == false)
        {
            _isInit = true;

            var manifest = Resources.Load<BuildinFileManifest>("BuildinFileManifest");
            if (manifest != null)
            {
                foreach (var element in manifest.BuildinFiles)
                {
                    if (_packages.TryGetValue(element.PackageName, out PackageQuery package) == false)
                    {
                        package = new PackageQuery();
                        _packages.Add(element.PackageName, package);
                    }
                    package.Elements.Add(element.FileName, element);
                }
            }
        }
    }

    /// <summary>
    /// 内置文件查询方法
    /// </summary>
    public static bool FileExists(string packageName, string fileName, string fileCRC32)
    {
        if (_isInit == false)
            Init();

        if (_packages.TryGetValue(packageName, out PackageQuery package) == false)
            return false;

        if (package.Elements.TryGetValue(fileName, out var element) == false)
            return false;

        if (GameQueryServices.CompareFileCRC)
        {
            return element.FileCRC32 == fileCRC32;
        }
        else
        {
            return true;
        }
    }
}
#endif


#if UNITY_EDITOR
    internal class PreprocessBuild : UnityEditor.Build.IPreprocessBuildWithReport
    {
        public int callbackOrder
        {
            get { return 0; }
        }

        /// <summary>
        /// 在构建应用程序前处理
        /// 原理：在构建APP之前，搜索StreamingAssets目录下的所有资源文件，然后将这些文件信息写入内置清单，内置清单存储在Resources文件夹下。
        /// </summary>
        public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
        {
            string saveFilePath = "Assets/AATemp/Resources/BuildinFileManifest.asset";
            if (File.Exists(saveFilePath))
            {
                File.Delete(saveFilePath);
                UnityEditor.AssetDatabase.SaveAssets();
                UnityEditor.AssetDatabase.Refresh();
            }

            string folderPath = $"{Application.dataPath}/StreamingAssets/{StreamingAssetsDefine.RootFolderName}";
            DirectoryInfo root = new DirectoryInfo(folderPath);
            if (root.Exists == false)
            {
                Debug.LogWarning($"没有发现YooAsset内置目录 : {folderPath}");
                return;
            }

            var manifest = ScriptableObject.CreateInstance<BuildinFileManifest>();
            FileInfo[] files = root.GetFiles("*", SearchOption.AllDirectories);
            foreach (var fileInfo in files)
            {
                if (fileInfo.Extension == ".meta")
                    continue;
                if (fileInfo.Name.StartsWith("PackageManifest_"))
                    continue;

                BuildinFileManifest.Element element = new BuildinFileManifest.Element();
                element.PackageName = fileInfo.Directory.Name;
                element.FileCRC32 = YooAsset.HashUtility.FileCRC32(fileInfo.FullName);
                element.FileName = fileInfo.Name;
                manifest.BuildinFiles.Add(element);
            }

            if (Directory.Exists("Assets/AATemp/Resources") == false)
                Directory.CreateDirectory("Assets/AATemp/Resources");
            UnityEditor.AssetDatabase.CreateAsset(manifest, saveFilePath);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            Debug.Log($"一共{manifest.BuildinFiles.Count}个内置文件，内置资源清单保存成功 : {saveFilePath}");
        }
    }
#endif
}