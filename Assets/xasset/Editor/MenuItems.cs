using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace xasset.editor
{
    public static class PlayModeMenuItems
    {
        private const string kFastPlayWithoutBuild = "xasset/Play Mode/Fast Play Without Build";
        private const string kPlayByUpdateWithSimulation = "xasset/Play Mode/Play By Update With Simulation";
        private const string kPlayWithoutUpdate = "xasset/Play Mode/Play Without Update";
        private const string kPlayByUpdateWithRealtime = "xasset/Play Mode/Play By Update With Realtime";

        private static void SetPlayMode(PlayMode mode)
        {
            var settings = Settings.GetDefaultSettings();
            settings.playMode = mode;
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        }

        [MenuItem(kFastPlayWithoutBuild, false, 80)]
        public static void SwitchToPlayByFastWithoutBuild()
        {
            SetPlayMode(PlayMode.FastPlayWithoutBuild);
        }

        [MenuItem(kPlayByUpdateWithSimulation, false, 80)]
        public static void SwitchPlayByBuildAndUpdateWithoutFileServer()
        { 
            SetPlayMode(PlayMode.PlayByUpdateWithSimulation);
        } 

        [MenuItem(kPlayByUpdateWithRealtime, false, 80)]
        public static void SwitchPlayByBuildAndUpdateByFileServer()
        { 
            SetPlayMode(PlayMode.PlayByUpdateWithRealtime);
        }
        
        [MenuItem(kPlayWithoutUpdate, false, 80)]
        public static void SwitchPlayByBuildWithoutUpdate()
        { 
            SetPlayMode(PlayMode.PlayWithoutUpdate);
        }

        [MenuItem(kFastPlayWithoutBuild, true, 80)]
        public static bool RefreshPlayByFastWithoutBuild()
        {
            var settings = Settings.GetDefaultSettings();
            Menu.SetChecked(kFastPlayWithoutBuild, settings.playMode == PlayMode.FastPlayWithoutBuild);
            return true;
        }

        [MenuItem(kPlayByUpdateWithSimulation, true, 80)]
        public static bool RefreshPlayByBuildAndUpdateWithoutFileServer()
        {
            var settings = Settings.GetDefaultSettings();
            Menu.SetChecked(kPlayByUpdateWithSimulation, settings.playMode == PlayMode.PlayByUpdateWithSimulation);
            return true;
        } 

        [MenuItem(kPlayByUpdateWithRealtime, true, 80)]
        public static bool RefreshPlayByUpdateWithRealtime()
        {
            var settings = Settings.GetDefaultSettings();
            Menu.SetChecked(kPlayByUpdateWithRealtime, settings.playMode == PlayMode.PlayByUpdateWithRealtime);
            return true;
        }
        
        [MenuItem(kPlayWithoutUpdate, true, 80)]
        public static bool RefreshPlayByBuildWithoutUpdate()
        {
            var settings = Settings.GetDefaultSettings();
            Menu.SetChecked(kPlayWithoutUpdate, settings.playMode == PlayMode.PlayWithoutUpdate);
            return true;
        }
    }

    public static class MenuItems
    {
        [MenuItem("xasset/About xasset", false, 1)]
        public static void OpenAbout()
        {
            Application.OpenURL("https://xasset.cc");
        }

        [MenuItem("xasset/Edit Settings", false, 1)]
        public static void PingSettings()
        {
            Selection.activeObject = Settings.GetDefaultSettings();
            EditorGUIUtility.PingObject(Selection.activeObject);
            EditorUtility.FocusProjectWindow();
        }


        [MenuItem("xasset/Build Bundles", false, 100)]
        public static void BuildBundles()
        {
            Builder.BuildBundles(Selection.GetFiltered<Build>(SelectionMode.DeepAssets));
        }

        [MenuItem("xasset/Build Bundles with Cache", false, 100)]
        public static void BuildBundlesWithCache()
        {
            Builder.BuildBundlesWithCache(Selection.GetFiltered<Build>(SelectionMode.DeepAssets));
        }

        [MenuItem("xasset/Build Player", false, 100)]
        public static void BuildPlayer()
        {
            Builder.BuildPlayer();
        }

        [MenuItem("xasset/Build Player Assets", false, 100)]
        public static void BuildPlayerAssetsWithSelection()
        {
            var path = EditorUtility.OpenFilePanelWithFilters("Select", Settings.PlatformDataPath,
                new[] { "versions", "json" });
            if (string.IsNullOrEmpty(path)) return;
            var versions = Utility.LoadFromFile<Versions>(path);
            Builder.BuildPlayerAssets(versions);
        }

        [MenuItem("xasset/Build Update Info", false, 100)]
        public static void BuildUpdateInfo()
        {
            var path = EditorUtility.OpenFilePanelWithFilters("Select", Settings.PlatformDataPath,
                new[] { "versions", "json" });
            if (string.IsNullOrEmpty(path)) return;

            var versions = Utility.LoadFromFile<Versions>(path);
            var file = new FileInfo(path);
            var hash = Utility.ComputeHash(path);
            Builder.BuildUpdateInfo(versions, hash, file.Length);
        }

        [MenuItem("xasset/Print Changes with Selection", false, 150)]
        public static void PrintChangesFromSelection()
        {
            var path = EditorUtility.OpenFilePanelWithFilters("Select", Settings.PlatformDataPath,
                new[] { "versions", "json" });
            if (string.IsNullOrEmpty(path)) return;
            var versions = Utility.LoadFromFile<Versions>(path);
            var filename = versions.GetFilename();
            var records = Utility.LoadFromFile<BuildChanges>(Settings.GetCachePath(BuildChanges.Filename));
            if (records.TryGetValue(filename, out var value)) Builder.GetChanges(value.files, filename);
        }
        
        [MenuItem("xasset/Clear Download", false, 300)]
        public static void ClearDownload()
        {
            var directory = Application.persistentDataPath;
            if (Directory.Exists(directory))
                Directory.Delete(directory, true);
            PlayerPrefs.SetString(Assets.kBundlesVersions, string.Empty);
            PlayerPrefs.Save();
        }

        [MenuItem("xasset/Clear Bundles", false, 300)]
        public static void ClearBundles()
        {
            var directories = new[] { Settings.PlatformDataPath, Settings.PlatformCachePath };
            foreach (var directory in directories)
                if (Directory.Exists(directory))
                    Directory.Delete(directory, true);
        }

        [MenuItem("xasset/Clear History", false, 300)]
        public static void ClearHistory()
        {
            Builder.ClearHistory();
        }

        [MenuItem("xasset/Open/Open Download Data Path", false, 300)]
        public static void OpenDownloadBundles()
        {
            EditorUtility.OpenWithDefaultApp(Assets.DownloadDataPath);
        }

        [MenuItem("xasset/Open/Open Temp Data Path", false, 300)]
        public static void OpenTempDataPath()
        {
            EditorUtility.OpenWithDefaultApp(Application.temporaryCachePath);
        }

        [MenuItem("xasset/Open/Open Bundles", false, 300)]
        public static void OpenBundles()
        {
            EditorUtility.OpenWithDefaultApp(Settings.PlatformDataPath);
        }

        [MenuItem("xasset/Open/Open Bundles Cache", false, 300)]
        public static void OpenBundlesCache()
        {
            EditorUtility.OpenWithDefaultApp(Settings.PlatformCachePath);
        }
        
        
        
        [MenuItem("xasset/Generate Group Assets Menu Items", false, 350)]
        public static void GenAssetsMenuItems()
        {
            var builds = Settings.FindAssets<Build>();
            var sb = new StringBuilder();
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using UnityEditor;");
            sb.AppendLine("namespace xasset.editor");
            sb.AppendLine("{");
            sb.AppendLine("\tpublic static class GroupAssetsMenuItems");
            sb.AppendLine("\t{");
            foreach (var build in builds)
            {
                for (var index = 0; index < build.groups.Length; index++)
                {
                    var group = build.groups[index];
                    sb.AppendLine($"\t\t[MenuItem(\"Assets/Group To/{build.name}/{group.name}\")]");
                    sb.AppendLine($"\t\tprivate static void GroupTo{build.name.Trim()}{group.name.Trim()}()");
                    sb.AppendLine("\t\t{");
                    sb.AppendLine($"\t\t\t{nameof(Settings)}.{nameof(Settings.MakeSelectionAssetsGroupTo)}(\"{build.name}\", \"{group.name}\");");
                    sb.AppendLine($"\t\t\tDebug.Log(\"Group to {group.name} with build {build.name}.\");");
                    sb.AppendLine("\t\t}");
                    if (index < build.groups.Length - 1)
                        sb.AppendLine();
                }
            } 
            sb.AppendLine("\t}");
            sb.AppendLine("}");
            var menuPath = "Assets/xasset/Editor/GroupAssetsMenuItems.cs";
            File.WriteAllText(menuPath, sb.ToString());
            AssetDatabase.ImportAsset(menuPath);
        }

        [MenuItem("xasset/Get Unity Online Services", false, 400)]
        public static void GetUnityOnlineServices()
        {
            Application.OpenURL("https://uos.unity.cn");
        }

        [MenuItem("xasset/Check for Updates", false, 400)]
        public static void CheckForUpdates()
        {
            Application.OpenURL("https://xasset.cc/docs/release-notes");
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