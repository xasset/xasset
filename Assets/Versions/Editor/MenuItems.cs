using System.IO;
using UnityEditor;
using UnityEngine;
using VEngine.Editor.Builds;

namespace VEngine.Editor
{
    public static class MenuItems
    {
        [MenuItem("Assets/Versions/Pack by file")]
        public static void PackByFile()
        {
            foreach (var o in Selection.GetFiltered<Object>(SelectionMode.DeepAssets))
            {
                var assetPath = AssetDatabase.GetAssetPath(o);
                if (string.IsNullOrEmpty(assetPath))
                {
                    continue;
                }

                if (Directory.Exists(assetPath))
                {
                    continue;
                }

                var assetImport = AssetImporter.GetAtPath(assetPath);
                var dir = Path.GetDirectoryName(assetPath)?.Replace('\\', '/').Replace('/', '_');
                var name = Path.GetFileNameWithoutExtension(assetPath);
                var type = Path.GetExtension(assetPath);
                assetImport.assetBundleName = string.Format("{0}_{1}{2}", dir, name, type).ToLower().Replace('.', '_');
            }
        }

        [MenuItem("Assets/Versions/Pack by dir")]
        public static void PackByDir()
        {
            foreach (var o in Selection.GetFiltered<Object>(SelectionMode.DeepAssets))
            {
                var assetPath = AssetDatabase.GetAssetPath(o);
                if (string.IsNullOrEmpty(assetPath))
                {
                    continue;
                }

                if (Directory.Exists(assetPath))
                {
                    continue;
                }

                var assetImport = AssetImporter.GetAtPath(assetPath);
                var dir = Path.GetDirectoryName(assetPath)?.Replace('\\', '/').Replace('/', '_').Replace('.', '_');
                assetImport.assetBundleName = dir;
            }
        }

        [MenuItem("Assets/Compute CRC")]
        public static void ComputeCRC()
        {
            var target = Selection.activeObject;
            var path = AssetDatabase.GetAssetPath(target);
            var crc32 = Utility.ComputeCRC32(File.OpenRead(path));
            Debug.LogFormat("ComputeCRC for {0} with {1}", path, crc32);
        }

        [MenuItem("Versions/Build Bundles")]
        public static void BuildBundles()
        {
            BuildScript.BuildBundles();
        }

        [MenuItem("Versions/Build Player")]
        public static void BuildPlayer()
        {
            BuildScript.BuildPlayer();
        }

        [MenuItem("Versions/Copy To StreamingAssets")]
        public static void CopyToStreamingAssets()
        {
            BuildScript.CopyToStreamingAssets();
        }

        [MenuItem("Versions/Clear Build")]
        public static void ClearBuild()
        {
            if (EditorUtility.DisplayDialog("提示", "清理构建数据将无法正常增量打包，确认清理？", "确定"))
            {
                var buildPath = Settings.PlatformBuildPath;
                Directory.Delete(buildPath, true);
            }
        }

        [MenuItem("Versions/Clear History")]
        public static void ClearHistory()
        {
            BuildScript.ClearHistory();
        }

        [MenuItem("Versions/Clear Download")]
        public static void ClearDownload()
        {
            Directory.Delete(Application.persistentDataPath, true);
        }

        [MenuItem("Versions/Clear Temporary")]
        public static void ClearTemporary()
        {
            Directory.Delete(Application.temporaryCachePath, true);
        }

        [MenuItem("Versions/View Settings")]
        public static void ViewSettings()
        {
            Settings.PingWithSelected(Settings.GetDefaultSettings());
        }

        [MenuItem("Versions/View Build")]
        public static void ViewBuild()
        {
            EditorUtility.OpenWithDefaultApp(Settings.PlatformBuildPath);
        }

        [MenuItem("Versions/View Download")]
        public static void ViewDownload()
        {
            EditorUtility.OpenWithDefaultApp(Application.persistentDataPath);
        }

        [MenuItem("Versions/View Temporary")]
        public static void ViewTemporary()
        {
            EditorUtility.OpenWithDefaultApp(Application.temporaryCachePath);
        }

#if !UNITY_2018_1_OR_NEWER
        [MenuItem("Assets/Copy Path")]
        public static void CopyAssetPath()
        {
            EditorGUIUtility.systemCopyBuffer = AssetDatabase.GetAssetPath(Selection.activeObject);
        }
#endif
    }
}