using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VEngine
{
    public static class Versions
    {
        public const string APIVersion = "7.1";
        public static Manifest Manifest;
        private static readonly Dictionary<string, string> BundleWithPathOrUrLs = new Dictionary<string, string>();
        public static bool SimulationMode;
        public static readonly List<string> builtinAssets = new List<string>();
        public static bool OfflineMode { get; set; }
        public static string ManifestsVersion => Manifest == null ? string.Empty : Manifest.version.ToString();
        public static string PlayerDataPath { get; set; }
        public static string DownloadURL { get; set; }
        public static string DownloadDataPath { get; set; }
        internal static string LocalProtocol { get; set; }
        public static string PlatformName { get; set; }

        public static void Override(Manifest value)
        {
            Manifest = value;
        }

        public static string GetDownloadDataPath(string file)
        {
            return $"{DownloadDataPath}/{file}";
        }

        public static string GetPlayerDataURL(string file)
        {
            return $"{LocalProtocol}{PlayerDataPath}/{file}";
        }

        public static string GetPlayerDataPath(string file)
        {
            return $"{PlayerDataPath}/{file}";
        }

        public static string GetDownloadURL(string file)
        {
            return $"{DownloadURL}{PlatformName}/{file}";
        }

        public static string GetTemporaryPath(string file)
        {
            var ret = $"{Application.temporaryCachePath}/{file}";
            var dir = Path.GetDirectoryName(ret);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            return ret;
        }

        public static ClearHistory ClearAsync()
        {
            var clearAsync = new ClearHistory();
            clearAsync.Start();
            return clearAsync;
        }

        public static void InitializeOnLoad()
        {
            if (Application.platform != RuntimePlatform.OSXEditor &&
                Application.platform != RuntimePlatform.OSXPlayer &&
                Application.platform != RuntimePlatform.IPhonePlayer)
            {
                if (Application.platform == RuntimePlatform.WindowsEditor ||
                    Application.platform == RuntimePlatform.WindowsPlayer)
                {
                    LocalProtocol = "file:///";
                }
                else
                {
                    LocalProtocol = string.Empty;
                }
            }
            else
            {
                LocalProtocol = "file://";
            }

            if (string.IsNullOrEmpty(PlatformName))
            {
                PlatformName = Utility.GetPlatformName();
            }

            if (string.IsNullOrEmpty(PlayerDataPath))
            {
                PlayerDataPath = $"{Application.streamingAssetsPath}/{Utility.buildPath}";
            }

            if (string.IsNullOrEmpty(DownloadDataPath))
            {
                DownloadDataPath = $"{Application.persistentDataPath}/{Utility.buildPath}";
            }

            if (!Directory.Exists(DownloadDataPath))
            {
                Directory.CreateDirectory(DownloadDataPath);
            }
        }


        public static InitializeVersions InitializeAsync(string downloadUrl)
        {
            DownloadURL = downloadUrl;
            InitializeOnLoad();
            var operation = new InitializeVersions();
            operation.Start();
            return operation;
        }


        public static UpdateVersions UpdateAsync(string file)
        {
            var operation = new UpdateVersions
            {
                file = file
            };
            operation.Start();
            return operation;
        }

        public static GetDownloadSize GetDownloadSizeAsync()
        {
            var getDownloadSize = new GetDownloadSize();
            getDownloadSize.bundles.AddRange(Manifest.bundles);
            getDownloadSize.Start();
            return getDownloadSize;
        }

        public static GetDownloadSize GetDownloadSizeAsync(UpdateVersions updateVersion)
        {
            var getDownloadSize = new GetDownloadSize();
            if (updateVersion.asset != null)
            {
                getDownloadSize.bundles.AddRange(updateVersion.asset.asset.bundles);
            }

            getDownloadSize.Start();
            return getDownloadSize;
        }

        public static DownloadVersions DownloadAsync(IEnumerable<DownloadInfo> groups)
        {
            var download = new DownloadVersions();
            download.items.AddRange(groups);
            download.Start();
            return download;
        }

        public static bool IsDownloaded(ManifestBundle bundle)
        {
            if (OfflineMode || builtinAssets.Contains(bundle.nameWithAppendHash))
            {
                return true;
            }

            var path = GetDownloadDataPath(bundle.nameWithAppendHash);
            var file = new FileInfo(path);
            return file.Exists && file.Length == bundle.size && Utility.ComputeCRC32(path) == bundle.crc;
        }

        internal static void SetBundlePathOrURl(string assetBundleName, string url)
        {
            BundleWithPathOrUrLs[assetBundleName] = url;
        }

        internal static string GetBundlePathOrURL(ManifestBundle bundle)
        {
            var assetBundleName = bundle.nameWithAppendHash;
            if (BundleWithPathOrUrLs.TryGetValue(assetBundleName, out var path))
            {
                return path;
            }

            if (OfflineMode || builtinAssets.Contains(assetBundleName))
            {
                path = GetPlayerDataPath(assetBundleName);
                BundleWithPathOrUrLs[assetBundleName] = path;
                return path;
            }

            if (IsDownloaded(bundle))
            {
                path = GetDownloadDataPath(assetBundleName);
                BundleWithPathOrUrLs[assetBundleName] = path;
                return path;
            }

            path = GetDownloadURL(assetBundleName);
            BundleWithPathOrUrLs[assetBundleName] = path;
            return path;
        }

        public static bool GetDependencies(string assetPath, out ManifestBundle bundle, out ManifestBundle[] bundles)
        {
            if (Manifest.Contains(assetPath))
            {
                bundle = Manifest.GetBundle(assetPath);
                bundles = Manifest.GetDependencies(bundle);
                return true;
            }

            bundle = null;
            bundles = null;
            return false;
        }

        public static bool Contains(string assetPath)
        {
            return Manifest.Contains(assetPath);
        }

        public static ManifestBundle GetBundle(string bundle)
        {
            return Manifest.GetBundle(bundle);
        }
    }
}