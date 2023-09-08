using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace xasset
{
    public static class Assets
    {
        public const string kBundlesVersions = "kBundlesVersions";
        public const string Bundles = "Bundles";
        public static readonly System.Version APIVersion = new System.Version(2023, 1, 1);

        private static readonly Dictionary<string, string> addressWithPaths = new Dictionary<string, string>();
        public static string UpdateInfoURL { get; set; }
        public static string DownloadURL { get; set; }
        public static Versions Versions { get; set; } = ScriptableObject.CreateInstance<Versions>();
        public static PlayerAssets PlayerAssets { get; set; } = ScriptableObject.CreateInstance<PlayerAssets>();
        public static bool RealtimeMode { get; set; } = true;
        public static bool Updatable { get; set; } = true;
        public static Platform Platform { get; set; } = Utility.GetPlatform();
        public static bool IsWebGLPlatform => Platform == Platform.WebGL;
        public static string Protocol { get; } = Utility.GetProtocol();
        public static string PlayerDataPath { get; set; } = $"{Application.streamingAssetsPath}/{Bundles}";
        public static string DownloadDataPath { get; set; } = $"{Application.persistentDataPath}/{Bundles}";
        public static Func<string, bool> ContainsFunc { get; set; } = ContainsRuntime;

        public static byte MaxDownloads { get; set; } = 5;
        public static byte MaxRetryTimes { get; set; } = 2; 


        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            Manifest.OnReadAsset = OnReadAsset;
            var assets = Object.FindObjectOfType<Scheduler>();
            if (assets != null) return;
            var gameObject = new GameObject("xasset", typeof(Downloader), typeof(Recycler), typeof(Scheduler));
            Object.DontDestroyOnLoad(gameObject);
        }

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

        public static RemoveRequest RemoveAsync(params string[] assetPaths)
        {
            var set = new HashSet<string>();
            foreach (var file in assetPaths)
                if (Versions.TryGetGroups(file, out var packs))
                    foreach (var pack in packs) 
                    foreach (var asset in pack.assets)
                        set.Add(GetDownloadDataPath(pack.manifest.bundles[asset].file));
                else if (Versions.TryGetAssets(file, out var assets))
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
            return ContainsFunc.Invoke(path);
        }

        private static bool ContainsRuntime(string path)
        {
            return Versions.data.Exists(version => version.manifest.ContainsAsset(path));
        }

        public static bool IsDownloaded(string path)
        {
            if (!TryGetAsset(ref path, out var asset)) 
            {
                Logger.E($"File not found {path}.");    
                return false;
            }

            var bundles = asset.manifest.bundles;
            var bundle = bundles[asset.bundle];
            if (!IsDownloaded(bundle))
                return false;

            foreach (var dependency in bundle.deps)
                if (!IsDownloaded(bundles[dependency]))
                    return false;

            return true;
        }

        public static bool IsDownloaded(Downloadable item, bool checkPlayerAssets = true)
        {
            if (checkPlayerAssets && IsPlayerAsset(item.hash)) return true;
            var path = GetDownloadDataPath(item.file);
            var file = new FileInfo(path);
            return file.Exists && file.Length == (long)item.size;
        }

        public static bool IsPlayerAsset(string key)
        {
            if (!Updatable) return true;
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

        private static void OnReadAsset(ManifestAsset asset)
        {
            if (asset.addressMode == AddressMode.LoadByName)
                SetAddress(asset.path, asset.name);
            else if (asset.addressMode == AddressMode.LoadByNameWithoutExtension)
                SetAddress(asset.path, Path.GetFileNameWithoutExtension(asset.name));
        }

        public static void SetAddress(string assetPath, string address)
        {
            if (!addressWithPaths.TryGetValue(address, out var value))
            {
                addressWithPaths[address] = assetPath;
            }
            else
            {
                if (assetPath != value)
                    Logger.W(
                        $"Failed to set address for {assetPath}, because the address {address} already mapping to {value}");
            }
        }

        private static void GetActualPath(ref string path)
        {
            if (addressWithPaths.TryGetValue(path, out var value)) path = value;
        }

        internal static bool TryGetAsset(ref string path, out ManifestAsset asset)
        {
            GetActualPath(ref path);
            if (RealtimeMode) 
                return Versions.TryGetAsset(path, out asset); 
            asset = null;
            return Contains(path);
        }

        public static ReloadRequest ReloadAsync(Versions versions)
        {
            var request = new ReloadRequest { versions = versions };
            request.SendRequest();
            return request;
        }

        public static void LoadPlayerAssets(PlayerAssets settings)
        {
            UpdateInfoURL = settings.updateInfoURL;
            DownloadURL = settings.downloadURL; 
            MaxRetryTimes = settings.maxRetryTimes;
            MaxDownloads = settings.maxDownloads;
            Scheduler.MaxRequests = settings.maxRequests;
            Scheduler.Autoslicing = settings.autoslicing;
            Scheduler.AutoslicingTimestep = settings.autoslicingTimestep;
            Recycler.AutoreleaseTimestep = settings.autoreleaseTimestep;
            Logger.LogLevel = settings.logLevel;
            PlayerAssets = settings;
        }
    }
}