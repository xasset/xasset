using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace xasset.editor
{
    public static class MenuItems
    {
        private const string kSimulationMode = "xasset/Simulation Mode";

        [MenuItem("xasset/About xasset", false, 1)]
        public static void OpenAbout()
        {
            Application.OpenURL("https://xasset.cc");
        }

        [MenuItem("xasset/Get Unity Online Services", false, 1)]
        public static void GetUnityOnlineServices()
        {
            Application.OpenURL("https://uos.unity.cn");
        }


        [MenuItem("xasset/Open/Settings", false, 1)]
        public static void PingSettings()
        {
            Selection.activeObject = Settings.GetDefaultSettings();
            EditorGUIUtility.PingObject(Selection.activeObject);
            EditorUtility.FocusProjectWindow();
        }

        [MenuItem(kSimulationMode, false, 1)]
        public static void SwitchSimulationMode()
        {
            var settings = Settings.GetDefaultSettings();
            settings.simulationMode = !settings.simulationMode;
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        }

        [MenuItem(kSimulationMode, true, 100)]
        public static bool RefreshSimulationMode()
        {
            var settings = Settings.GetDefaultSettings();
            Menu.SetChecked(kSimulationMode, settings.simulationMode);
            return true;
        }

        [MenuItem("xasset/Open/Startup Scene", false, 100)]
        public static void OpenStartupScene()
        {
            EditorSceneManager.OpenScene("Assets/xasset/Example/Startup.unity");
        }

        [MenuItem("xasset/Open/Download Data Path", false, 100)]
        public static void OpenDownloadBundles()
        {
            EditorUtility.OpenWithDefaultApp(Assets.DownloadDataPath);
        }

        [MenuItem("xasset/Build Bundles", false, 100)]
        public static void BuildBundles()
        {
            Builder.BuildBundles(Selection.GetFiltered<Build>(SelectionMode.DeepAssets));
        }

        [MenuItem("xasset/Build Bundles with Last Build", false, 100)]
        public static void BuildBundlesWithLastBuild()
        {
            Builder.BuildBundlesWithLastBuild(Selection.GetFiltered<Build>(SelectionMode.DeepAssets));
        }

        [MenuItem("xasset/Build Player", false, 100)]
        public static void BuildPlayer()
        {
            editor.BuildPlayer.Build();
        }

        [MenuItem("xasset/Build Player Assets", false, 100)]
        public static void BuildPlayerAssetsWithSelection()
        {
            var path = EditorUtility.OpenFilePanelWithFilters("Select", Settings.PlatformDataPath,
                new[] {"versions", "json"});
            if (string.IsNullOrEmpty(path)) return;
            var versions = Utility.LoadFromFile<Versions>(path);
            BuildPlayerAssets.CustomBuilder = null;
            BuildPlayerAssets.StartNew(versions);
        }

        [MenuItem("xasset/Build Update Info", false, 100)]
        public static void BuildUpdateInfo()
        {
            var path = EditorUtility.OpenFilePanelWithFilters("Select", Settings.PlatformDataPath,
                new[] {"versions", "json"});
            if (string.IsNullOrEmpty(path)) return;

            var versions = Utility.LoadFromFile<Versions>(path);
            var file = new FileInfo(path);
            var hash = Utility.ComputeHash(path);
            Builder.BuildUpdateInfo(versions, hash, file.Length);
        }

        [MenuItem("xasset/Clear Download", false, 200)]
        public static void ClearDownload()
        {
            var directory = Application.persistentDataPath;
            if (Directory.Exists(directory))
                Directory.Delete(directory, true);
        }

        [MenuItem("xasset/Clear Bundles", false, 200)]
        public static void ClearBundles()
        {
            var directory = Settings.PlatformDataPath;
            if (Directory.Exists(directory))
                Directory.Delete(directory, true);
        }

        [MenuItem("xasset/Clear History", false, 200)]
        public static void ClearHistory()
        {
            editor.ClearHistory.Start();
        }

        [MenuItem("xasset/Check for Updates", false, 300)]
        public static void CheckForUpdates()
        {
            Application.OpenURL("https://xasset.cc/docs/next/change-log");
        }

        [MenuItem("xasset/Print Changes", false, 300)]
        public static void PrintChanges()
        {
            var path = EditorUtility.OpenFilePanelWithFilters("Select", Settings.PlatformDataPath,
                new[] {"versions", "json"});
            if (string.IsNullOrEmpty(path)) return;
            var versions = Utility.LoadFromFile<Versions>(path);
            var filename = versions.GetFilename();
            var records = Utility.LoadFromFile<BuildRecords>(Settings.GetCachePath(BuildRecords.Filename));
            if (records.TryGetValue(filename, out var value)) Builder.GetChanges(value.changes, filename);
        }

        [MenuItem("Assets/To Json")]
        public static void ToJson()
        {
            var activeObject = Selection.activeObject;
            var json = JsonUtility.ToJson(activeObject);
            var path = AssetDatabase.GetAssetPath(activeObject);
            var ext = Path.GetExtension(path);
            File.WriteAllText(path.Replace(ext, ".json"), json);
        }
    }
}