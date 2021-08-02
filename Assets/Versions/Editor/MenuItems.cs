using System.IO;
using UnityEditor;
using UnityEngine;
using Versions.Editor.Builds;

namespace Versions.Editor
{
    public static class MenuItems
    {
        [MenuItem("Assets/Compute CRC")]
        public static void ComputeCRC()
        {
            var target = Selection.activeObject;
            var path = AssetDatabase.GetAssetPath(target);
            var crc32 = Utility.ComputeCRC32(File.OpenRead(path));
            Debug.LogFormat("ComputeCRC for {0} with {1}", path, crc32);
        }

        [MenuItem("Assets/Versions/Build/Bundles")]
        public static void BuildBundles()
        {
            BuildScript.BuildBundles();
        }

        [MenuItem("Assets/Versions/Build/Player")]
        public static void BuildPlayer()
        {
            BuildScript.BuildPlayer();
        }

        [MenuItem("Assets/Versions/Copy To StreamingAssets")]
        public static void CopyToStreamingAssets()
        {
            BuildScript.CopyToStreamingAssets();
        }

        [MenuItem("Assets/Versions/Clear/Build")]
        public static void ClearBuild()
        {
            if (EditorUtility.DisplayDialog("提示", "清理构建数据将无法正常增量打包，确认清理？", "确定"))
            {
                var buildPath = Settings.PlatformBuildPath;
                Directory.Delete(buildPath, true);
            }
        }

        [MenuItem("Assets/Versions/Clear/History")]
        public static void ClearHistory()
        {
            BuildScript.ClearHistory();
        }

        [MenuItem("Assets/Versions/Clear/Download")]
        public static void ClearDownload()
        {
            Directory.Delete(Application.persistentDataPath, true);
        }

        [MenuItem("Assets/Versions/Clear/Temporary")]
        public static void ClearTemporary()
        {
            Directory.Delete(Application.temporaryCachePath, true);
        }

        [MenuItem("Assets/Versions/View/Settings")]
        public static void ViewSettings()
        {
            Settings.PingWithSelected(Settings.GetDefaultSettings());
        }

        [MenuItem("Assets/Versions/View/Build")]
        public static void ViewBuild()
        {
            EditorUtility.OpenWithDefaultApp(Settings.PlatformBuildPath);
        }

        [MenuItem("Assets/Versions/View/Download")]
        public static void ViewDownload()
        {
            EditorUtility.OpenWithDefaultApp(Application.persistentDataPath);
        }

        [MenuItem("Assets/Versions/View/Temporary")]
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