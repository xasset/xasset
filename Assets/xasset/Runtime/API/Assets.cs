using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace xasset
{
    public static class Assets
    {
        public const string Bundles = "Bundles";
        public static readonly System.Version APIVersion = new System.Version(2023, 0, 0);
        public static string UpdateInfoURL { get; set; }
        public static string DownloadURL { get; set; }
        public static Versions Versions { get; set; } = ScriptableObject.CreateInstance<Versions>();
        public static PlayerAssets PlayerAssets { get; set; } = ScriptableObject.CreateInstance<PlayerAssets>();
        public static bool SimulationMode { get; set; }
        public static bool OfflineMode { get; set; } = true;
        public static Platform Platform { get; set; } = Utility.GetPlatform();
        public static bool IsWebGLPlatform => Platform == Platform.WebGL;
        public static string Protocol { get; } = Utility.GetProtocol();
        public static string PlayerDataPath { get; set; } = $"{Application.streamingAssetsPath}/{Bundles}";
        public static string DownloadDataPath { get; set; } = $"{Application.persistentDataPath}/{Bundles}";

        public static InitializeRequest InitializeAsync(Action<Request> completed = null)
        {
            return InitializeRequest.InitializeAsync(completed);
        }

        public static GetUpdateInfoRequest GetUpdateInfoAsync()
        {
            var request = new GetUpdateInfoRequest();
            request.SendRequest();
            return request;
        }

        public static VersionsRequest GetVersionsAsync(UpdateInfo info)
        {
            var request = new VersionsRequest {url = GetDownloadURL(info.file), hash = info.hash, size = info.size};
            request.SendRequest();
            return request;
        }

        public static GetDownloadSizeRequest GetDownloadSizeAsync(Versions versions)
        {
            var request = new GetDownloadSizeRequest {versions = versions};
            request.SendRequest();
            return request;
        }

        public static RemoveRequest RemoveAsync(params string[] assetPaths)
        {
            var set = new HashSet<string>();
            foreach (var file in assetPaths)
                if (Versions.TryGetAssets(file, out var assets))
                    foreach (var asset in assets)
                        set.Add(GetDownloadDataPath(asset.manifest.bundles[asset.bundle].file));
                else
                    Logger.W($"File not found {file}");

            var request = new RemoveRequest();
            request.files.AddRange(set);
            request.SendRequest();
            return request;
        }

        public static bool Contains(string path)
        {
            if (SimulationMode)
            {
                return File.Exists(path);
            }

            return Versions.data.Exists(version => version.manifest.ContainsAsset(path));
        }

        public static bool IsDownloaded(string path)
        {
            if (!TryGetAsset(ref path, out var asset)) return true;

            var bundles = asset.manifest.bundles;
            var bundle = bundles[asset.bundle];
            if (!IsDownloaded(bundle))
                return false;

            foreach (var dependency in bundle.deps)
                if (!IsDownloaded(bundles[dependency]))
                    return false;

            return true;
        }

        public static bool IsDownloaded(Downloadable item)
        {
            var path = GetDownloadDataPath(item.file);
            var file = new FileInfo(path);
            return file.Exists && file.Length == (long) item.size;
        }

        public static bool IsPlayerAsset(string key)
        {
            if (OfflineMode) return true;
            return PlayerAssets != null && PlayerAssets.Contains(key);
        }

        public static string GetDownloadURL(string filename)
        {
            return $"{DownloadURL}/{filename}";
        }

        public static string GetPlayerDataPath(string filename)
        {
            return $"{PlayerDataPath}/{filename}";
        }

        public static string GetPlayerDataURl(string filename)
        {
            return $"{Protocol}{GetPlayerDataPath(filename)}";
        }

        public static string GetDownloadDataPath(string filename)
        {
            var path = $"{DownloadDataPath}/{filename}";
            Utility.CreateDirectoryIfNecessary(path);
            return path;
        }

        public static string GetTemporaryCachePath(string filename)
        {
            var path = $"{Application.temporaryCachePath}/{filename}";
            Utility.CreateDirectoryIfNecessary(path);
            return path;
        }

        private static readonly Dictionary<string, string> addressWithPaths = new Dictionary<string, string>();

        public static void SetAddress(string assetPath, string address)
        {
            if (!addressWithPaths.TryGetValue(address, out var value))
            {
                addressWithPaths[address] = assetPath;
            }
            else
            {
                if (assetPath != value)
                {
                    Logger.W($"Failed to set address for {assetPath}, because the address {address} already mapping to {value}");
                }
            }
        }

        public static void GetActualPath(ref string path)
        {
            if (addressWithPaths.TryGetValue(path, out var value))
            {
                path = value;
            }
        }

        public static bool TryGetAsset(ref string path, out ManifestAsset asset)
        {
            GetActualPath(ref path);

            if (!SimulationMode)
            {
                if (Versions.TryGetAsset(path, out asset))
                {
                    return true;
                }

                Logger.E($"File not found:{path}.");
                return false;
            }

            asset = null;
            if (File.Exists(path))
            {
                return true;
            }

            Logger.E($"File not found:{path}.");
            return false;
        }
    }
}