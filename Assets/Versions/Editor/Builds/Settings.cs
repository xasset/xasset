using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace VEngine.Editor.Builds
{
    public enum ScriptPlayMode
    {
        Simulation,
        Preload,
        Incremental
    }

    [CreateAssetMenu(menuName = "Versions/Settings", fileName = "Settings", order = 0)]
    public sealed class Settings : ScriptableObject
    {
        public List<string> excludeFiles =
            new List<string>
            {
                ".spriteatlas",
                ".giparams",
                "LightingData.asset"
            };

        public bool offlineMode;
        public ScriptPlayMode scriptPlayMode = ScriptPlayMode.Simulation;

        public static string PlatformBuildPath
        {
            get
            {
                var dir = Utility.buildPath + $"/{GetPlatformName()}";
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                return dir;
            }
        }

        public static string BuildPlayerDataPath =>
            string.Format("{0}/{1}", Application.streamingAssetsPath, Utility.buildPath);

        public static PlayerSettings GetPlayerSettings()
        {
            return LoadAsset<PlayerSettings>("Assets/Resources/PlayerSettings.asset");
        }

        public static Settings GetDefaultSettings()
        {
            var guilds = AssetDatabase.FindAssets("t:Settings");
            foreach (var guild in guilds)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guild);
                if (string.IsNullOrEmpty(assetPath)) continue;

                var settings = LoadAsset<Settings>(assetPath);
                if (settings == null) continue;

                return settings;
            }

            return LoadAsset<Settings>("Assets/Settings.asset");
        }

        public static Manifest GetManifest()
        {
            var manifest = CreateInstance<Manifest>();
            manifest.name = nameof(Manifest);
            var path = GetBuildPath(manifest.name);
            if (!File.Exists(path)) return manifest;

            manifest.Load(path);
            return manifest;
        }

        public static List<ManifestBundle> GetBundlesInBuild(bool includeManifest)
        {
            var bundles = new List<ManifestBundle>();
            var manifest = GetManifest();
            bundles.AddRange(manifest.bundles);
            if (includeManifest)
            {
                var manifestName = $"{manifest.name}";
                var manifestVersionName = Manifest.GetVersionFile(manifestName);
                bundles.Add(new ManifestBundle
                {
                    name = manifestVersionName,
                    nameWithAppendHash = manifestVersionName
                });
                bundles.Add(new ManifestBundle
                {
                    name = manifestName,
                    nameWithAppendHash = manifestName
                });
            }

            return bundles;
        }

        public static string GetBuildPath(string file)
        {
            return $"{PlatformBuildPath}/{file}";
        }

        public static string GetPlatformName()
        {
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.Android:
                    return "Android";
                case BuildTarget.StandaloneOSX:
                    return "OSX";
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "Windows";
                case BuildTarget.iOS:
                    return "iOS";
                case BuildTarget.WebGL:
                    return "WebGL";
                default:
                    return Utility.unsupportedPlatform;
            }
        }

        public static void PingWithSelected(Object target)
        {
            Selection.activeObject = target;
            EditorGUIUtility.PingObject(target);
        }

        public static T LoadAsset<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;

            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

            asset = CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        public static void SaveAsset(Object asset)
        {
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static IEnumerable<string> GetDependencies(string path)
        {
            var set = new HashSet<string>(AssetDatabase.GetDependencies(path, true));
            set.Remove(path);
            return set;
        }
    }
}