using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace xasset
{
    public static class Assets
    {
        public const string Bundles = "Bundles";
        public static readonly System.Version APIVersion = new System.Version(2022, 2, 1);
        public static string UpdateURL { get; set; }
        public static string DownloadURL { get; set; }
        public static Versions Versions { get; set; } = ScriptableObject.CreateInstance<Versions>();
        public static PlayerAssets PlayerAssets { get; set; } = ScriptableObject.CreateInstance<PlayerAssets>();
        public static bool SimulationMode { get; set; }
        public static bool FastVerifyMode { get; set; } = true;
        public static Platform Platform { get; set; } = Utility.GetPlatform();
        public static bool IsWebGLPlatform => Platform == Platform.WebGL;
        public static string Protocol => Utility.GetProtocol();
        public static string PlayerDataPath => $"{Application.streamingAssetsPath}/{Bundles}";
        public static string DownloadDataPath { get; set; } = $"{Application.persistentDataPath}/{Bundles}";

        /// <summary>
        ///     初始化。
        /// </summary>
        /// <param name="completed"></param>
        /// <returns></returns>
        public static InitializeRequest InitializeAsync(Action<Request> completed = null)
        {
            var request = new InitializeRequest();
            request.SendRequest();
            request.completed = completed;
            return request;
        }

        /// <summary>
        ///     获取更新信息。
        /// </summary>
        /// <returns></returns>
        public static GetUpdateInfoRequest GetUpdateInfoAsync()
        {
            var request = new GetUpdateInfoRequest();
            request.SendRequest();
            return request;
        }

        /// <summary>
        ///     获取版本文件。
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static VersionsRequest GetVersionsAsync(UpdateInfo info)
        {
            var request = new VersionsRequest {url = GetDownloadURL(info.file), hash = info.hash, size = info.size, retryTimes = 2};
            request.SendRequest();
            return request;
        }

        /// <summary>
        ///     获取更新大小。
        /// </summary>
        /// <param name="versions"></param>
        /// <param name="assetPaths"></param>
        /// <returns></returns>
        public static GetDownloadSizeRequest GetDownloadSizeAsync(Versions versions, params string[] assetPaths)
        {
            var request = new GetDownloadSizeRequest {assetPaths = assetPaths, versions = versions};
            request.SendRequest();
            return request;
        }

        /// <summary>
        ///     删除下载的资源
        /// </summary>
        /// <param name="assetPaths"></param>
        /// <returns></returns>
        public static RemoveRequest RemoveAsync(params string[] assetPaths)
        {
            var set = new HashSet<string>();
            foreach (var file in assetPaths)
                if (Versions.TryGetAssets(file, out var assets))
                    foreach (var asset in assets)
                    {
                        var bundles = asset.manifest.bundles;
                        if (asset.bundle < bundles.Length)
                            set.Add(GetDownloadDataPath(bundles[asset.bundle].nameWithAppendHash));
                    }
                else
                    Logger.W($"File not found {file}");

            var request = new RemoveRequest();
            request.files.AddRange(set);
            request.SendRequest();
            return request;
        }

        /// <summary>
        ///     判断资源是否包含在加载的版本文件中。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool Contains(string path)
        {
            return Versions.data.Exists(version => version.manifest.ContainsAsset(path));
        }

        /// <summary>
        ///     判断资源是否下载完成，会检查依赖。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsDownloaded(string path)
        {
            if (!Versions.TryGetAsset(path, out var asset)) return true;
            var bundle = asset.mainBundle;
            if (!IsDownloaded(bundle))
                return false;

            foreach (var dependency in asset.mainBundle.deps)
            {
                bundle = asset.manifest.bundles[dependency];
                if (!IsDownloaded(bundle)) return false;
            }

            return true;
        }

        /// <summary>
        ///     判断 bundle 是否下载，不检查依赖。
        /// </summary>
        /// <param name="bundle"></param>
        /// <returns></returns>
        public static bool IsDownloaded(ManifestBundle bundle)
        {
            if (IsPlayerAsset(bundle.hash)) return true;
            var path = GetDownloadDataPath(bundle.nameWithAppendHash);
            var file = new FileInfo(path);
            if (!file.Exists || file.Length != (long) bundle.size) return false;
            if (FastVerifyMode) return true;
            return Utility.ComputeHash(path) == bundle.hash;
        }

        /// <summary>
        ///     判断资源是否在安装包内部。
        /// </summary>
        /// <param name="key">资源内容的hash</param>
        /// <returns></returns>
        public static bool IsPlayerAsset(string key)
        {
            return PlayerAssets != null && PlayerAssets.Contains(key);
        }

        /// <summary>
        ///     判断版本是否下载
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public static bool IsDownloaded(Version version)
        {
            var path = GetDownloadDataPath(version.file);
            var file = new FileInfo(path);
            if (!file.Exists || file.Length != (long) version.size) return false;
            if (FastVerifyMode) return true;
            return Utility.ComputeHash(path) == version.hash;
        }

        /// <summary>
        ///     获取下载地址
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string GetDownloadURL(string filename)
        {
            return $"{DownloadURL}/{filename}";
        }

        /// <summary>
        ///     获取安装包数据路径。
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string GetPlayerDataPath(string filename)
        {
            return $"{PlayerDataPath}/{filename}";
        }

        /// <summary>
        ///     获取安装包数据URL。
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string GetPlayerDataURl(string filename)
        {
            return $"{Protocol}{GetPlayerDataPath(filename)}";
        }

        /// <summary>
        ///     获取下载数据路径。
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string GetDownloadDataPath(string filename)
        {
            var path = $"{DownloadDataPath}/{filename}";
            Utility.CreateDirectoryIfNecessary(path);
            return path;
        }

        /// <summary>
        ///     获取临时缓存路径。
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string GetTemporaryCachePath(string filename)
        {
            var path = $"{Application.temporaryCachePath}/{filename}";
            Utility.CreateDirectoryIfNecessary(path);
            return path;
        }
    }
}