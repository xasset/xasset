using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace XAsset.Editor
{
    public abstract class BuildRule
    {
        protected static List<string> packedAssets = new List<string>();
        protected static List<AssetBundleBuild> builds = new List<AssetBundleBuild>();
        private static List<BuildRule> rules = new List<BuildRule>();
        private static Dictionary<string, List<string>> allDependencies = new Dictionary<string, List<string>>();

        public static List<AssetBundleBuild> GetBuilds(string manifestPath)
        {
            packedAssets.Clear();
            builds.Clear();
            rules.Clear();
            allDependencies.Clear();

            AssetBundleBuild build = new AssetBundleBuild();
            build.assetBundleName = "manifest";
            build.assetNames = new string[] { manifestPath };
            builds.Add(build);

            const string rulesini = "Assets/Rules.txt";
            if (File.Exists(rulesini))
            {
                LoadRules(rulesini);
            }
            else
            {
                rules.Add(new BuildAssetsWithFilename("Assets/SampleAssets", "*.prefab", SearchOption.AllDirectories));
                SaveRules(rulesini);
            }

            foreach (var item in rules)
            {
                CollectDependencies(GetFilesWithoutDirectories(item.searchPath, item.searchPattern, item.searchOption));
            }

            BuildDependenciesAssets();

            foreach (var item in rules)
            {
                item.Build();
            }

#if ENABLE_ATLAS
			BuildAtlas(); 
#endif
            UnityEditor.EditorUtility.ClearProgressBar();

            return builds;
        }

        static void BuildAtlas()
        {
            foreach (var item in builds)
            {
                var assets = item.assetNames;
                foreach (var asset in assets)
                {
                    var importer = AssetImporter.GetAtPath(asset);
                    if (importer is TextureImporter)
                    {
                        var ti = importer as TextureImporter;
                        if (ti.textureType == TextureImporterType.Sprite)
                        {
                            var tex = AssetDatabase.LoadAssetAtPath<Texture>(asset);
                            if (tex.texelSize.x >= 1024 || tex.texelSize.y >= 1024)
                            {
                                continue;
                            }

                            var tag = item.assetBundleName.Replace("/", "_");
                            if (! tag.Equals(ti.spritePackingTag))
                            {
                                var settings = ti.GetPlatformTextureSettings(EditorUtility.GetPlatformName());
                                settings.format = ti.GetAutomaticFormat(EditorUtility.GetPlatformName());
                                settings.overridden = true;
                                ti.SetPlatformTextureSettings(settings);
                                ti.spritePackingTag = tag;
                                ti.SaveAndReimport();
                            }
                        }
                    }
                }
 
            }
        }

        static void SaveRules(string rulesini)
        {
            using (var s = new StreamWriter(rulesini))
            {
                foreach (var item in rules)
                {
                    s.WriteLine("[{0}]", item.GetType().Name);
                    s.WriteLine("searchPath=" + item.searchPath);
                    s.WriteLine("searchPattern=" + item.searchPattern);
                    s.WriteLine("searchOption=" + item.searchOption);
                    s.WriteLine("bundleName=" + item.bundleName);
                    s.WriteLine();
                }
                s.Flush();
                s.Close();
            }
        }

        static void LoadRules(string rulesini)
        {
            using (var s = new StreamReader(rulesini))
            {
                rules.Clear();

                string line = null;
                while ((line = s.ReadLine()) != null)
                {
                    if (line == string.Empty || line.StartsWith("#", StringComparison.CurrentCulture) || line.StartsWith("//", StringComparison.CurrentCulture))
                    {
                        continue;
                    }
                    if (line.Length > 2 && line[0] == '[' && line[line.Length - 1] == ']')
                    {
                        var name = line.Substring(1, line.Length - 2);
                        var searchPath = s.ReadLine().Split('=')[1];
                        var searchPattern = s.ReadLine().Split('=')[1];
                        var searchOption = s.ReadLine().Split('=')[1];
                        var bundleName = s.ReadLine().Split('=')[1];
                        var type = typeof(BuildRule).Assembly.GetType("XAsset.Editor." + name);
                        if (type != null)
                        {
                            var rule = Activator.CreateInstance(type) as BuildRule;
                            rule.searchPath = searchPath;
                            rule.searchPattern = searchPattern;
                            rule.searchOption = (SearchOption)Enum.Parse(typeof(SearchOption), searchOption);
                            rule.bundleName = bundleName;
                            rules.Add(rule);
                        }
                    }
                }
            }
        }

        static List<string> GetFilesWithoutDirectories(string prefabPath, string searchPattern, SearchOption searchOption)
        {
            var files = Directory.GetFiles(prefabPath, searchPattern, searchOption);
            List<string> items = new List<string>();
            foreach (var item in files)
            {
                var assetPath = item.Replace('\\', '/');
                if (!Directory.Exists(assetPath))
                {
                    items.Add(assetPath);
                }
            }
            return items;
        }

        protected static void BuildDependenciesAssets()
        {
            Dictionary<string, List<string>> bundles = new Dictionary<string, List<string>>();
            foreach (var item in allDependencies)
            {
                var assetPath = item.Key;
                if (!assetPath.EndsWith(".cs", StringComparison.CurrentCulture))
                {
                    if (packedAssets.Contains(assetPath))
                    {
                        continue;
                    }
                    if (assetPath.EndsWith(".shader", StringComparison.CurrentCulture))
                    {
                        List<string> list = null;
                        if (!bundles.TryGetValue("shaders", out list))
                        {
                            list = new List<string>();
                            bundles.Add("shaders", list);
                        }
                        if (!list.Contains(assetPath))
                        {
                            list.Add(assetPath);
                            packedAssets.Add(assetPath);
                        }
                    }
                    else
                    {
                        if (item.Value.Count > 1)
                        {
                            var name = "shared/" + BuildAssetBundleNameWithAssetPath(Path.GetDirectoryName(assetPath));
                            List<string> list = null;
                            if (!bundles.TryGetValue(name, out list))
                            {
                                list = new List<string>();
                                bundles.Add(name, list);
                            }
                            if (!list.Contains(assetPath))
                            {
                                list.Add(assetPath);
                                packedAssets.Add(assetPath);
                            }
                        }
                    }
                }
            }
            foreach (var item in bundles)
            {
                AssetBundleBuild build = new AssetBundleBuild();
				build.assetBundleName = item.Key + "_" + item.Value.Count;
                build.assetNames = item.Value.ToArray();
                builds.Add(build);
            }
        }

        protected static List<string> GetDependenciesWithoutShared(string item)
        {
            var assets = AssetDatabase.GetDependencies(item);
            List<string> assetNames = new List<string>();
            foreach (var assetPath in assets)
            {
                if (assetPath.Contains(".prefab") || assetPath.Equals(item) || packedAssets.Contains(assetPath) || assetPath.EndsWith(".cs", StringComparison.CurrentCulture) || assetPath.EndsWith(".shader", StringComparison.CurrentCulture))
                {
                    continue;
                }
                if (allDependencies[assetPath].Count == 1)
                {
                    assetNames.Add(assetPath);
                }
            }
            return assetNames;
        }

        protected static void CollectDependencies(List<string> files)
        {
            for (int i = 0; i < files.Count; i++)
            {
                var item = files[i];
                var dependencies = AssetDatabase.GetDependencies(item);
                if (UnityEditor.EditorUtility.DisplayCancelableProgressBar(string.Format("Collecting... [{0}/{1}]", i, files.Count), item, i * 1f / files.Count))
                {
                    break;
                }

                foreach (var assetPath in dependencies)
                {
                    if (!allDependencies.ContainsKey(assetPath))
                    {
                        allDependencies[assetPath] = new List<string>();
                    }

                    if (!allDependencies[assetPath].Contains(item))
                    {
                        allDependencies[assetPath].Add(item);
                    }
                }
            }
        }

        protected static List<string> GetFilesWithoutPacked(string searchPath, string searchPattern, SearchOption searchOption)
        {
            var files = GetFilesWithoutDirectories(searchPath, searchPattern, searchOption);
            var filesCount = files.Count;
            var removeAll = files.RemoveAll((string obj) =>
            {
                return packedAssets.Contains(obj);
            });
            Debug.Log(string.Format("RemoveAll {0} size: {1}", removeAll, filesCount));

            return files;
        }

        protected static string BuildAssetBundleNameWithAssetPath(string assetPath)
        {
            return Path.Combine(Path.GetDirectoryName(assetPath), Path.GetFileNameWithoutExtension(assetPath)).Replace('\\', '/').ToLower();
        }

        public string searchPath;
        public string searchPattern;
        public SearchOption searchOption = SearchOption.AllDirectories;
        public string bundleName;


        protected BuildRule()
        {

        }

        protected BuildRule(string path, string pattern, SearchOption option)
        {
            searchPath = path;
            searchPattern = pattern;
            searchOption = option;
        }

        public abstract void Build();

        public abstract string GetAssetBundleName(string assetPath);
    }

    public class BuildAssetsWithAssetBundleName : BuildRule
    {
        public BuildAssetsWithAssetBundleName()
        {

        }

        public override string GetAssetBundleName(string assetPath)
        {
            return bundleName;
        }

        public BuildAssetsWithAssetBundleName(string path, string pattern, SearchOption option, string assetBundleName) : base(path, pattern, option)
        {
            bundleName = assetBundleName;
        }

        public override void Build()
        {
            var files = GetFilesWithoutPacked(searchPath, searchPattern, searchOption);
            List<string> list = new List<string>();
            foreach (var item in files)
            {
                list.AddRange(GetDependenciesWithoutShared(item));
            }
            files.AddRange(list);
            AssetBundleBuild build = new AssetBundleBuild();
            build.assetBundleName = bundleName;
            build.assetNames = files.ToArray();
            builds.Add(build);
            packedAssets.AddRange(files);
        }
    }

    public class BuildAssetsWithDirectroyName : BuildRule
    {
        public BuildAssetsWithDirectroyName()
        {

        }

        public BuildAssetsWithDirectroyName(string path, string pattern, SearchOption option) : base(path, pattern, option)
        {
        }

        public override string GetAssetBundleName(string assetPath)
        {
            return BuildAssetBundleNameWithAssetPath(Path.GetDirectoryName(assetPath));
        }

        public override void Build()
        {
            var files = GetFilesWithoutPacked(searchPath, searchPattern, searchOption);

            Dictionary<string, List<string>> bundles = new Dictionary<string, List<string>>();
            for (int i = 0; i < files.Count; i++)
            {
                var item = files[i];
                if (UnityEditor.EditorUtility.DisplayCancelableProgressBar(string.Format("Collecting... [{0}/{1}]", i, files.Count), item, i * 1f / files.Count))
                {
                    break;
                }
                var path = Path.GetDirectoryName(item);
                if (!bundles.ContainsKey(path))
                {
                    bundles[path] = new List<string>();
                }
                bundles[path].Add(item);
                bundles[path].AddRange(GetDependenciesWithoutShared(item));
            }

            int count = 0;
            foreach (var item in bundles)
            {
                AssetBundleBuild build = new AssetBundleBuild();
                build.assetBundleName = BuildAssetBundleNameWithAssetPath(item.Key) + "_" + item.Value.Count;
                build.assetNames = item.Value.ToArray();
                packedAssets.AddRange(build.assetNames);
                builds.Add(build);
                if (UnityEditor.EditorUtility.DisplayCancelableProgressBar(string.Format("Packing... [{0}/{1}]", count, bundles.Count), build.assetBundleName, count * 1f / bundles.Count))
                {
                    break;
                }
                count++;
            }
        }
    }

    public class BuildAssetsWithFilename : BuildRule
    {
        public BuildAssetsWithFilename()
        {

        }

        public override string GetAssetBundleName(string assetPath)
        {
            return BuildAssetBundleNameWithAssetPath(assetPath);
        }

        public BuildAssetsWithFilename(string path, string pattern, SearchOption option) : base(path, pattern, option)
        {
        }

        public override void Build()
        {
            var files = GetFilesWithoutPacked(searchPath, searchPattern, searchOption);

            for (int i = 0; i < files.Count; i++)
            {
                var item = files[i];
                if (UnityEditor.EditorUtility.DisplayCancelableProgressBar(string.Format("Packing... [{0}/{1}]", i, files.Count), item, i * 1f / files.Count))
                {
                    break;
                }
                AssetBundleBuild build = new AssetBundleBuild();
                build.assetBundleName = BuildAssetBundleNameWithAssetPath(item);
                var assetNames = GetDependenciesWithoutShared(item);
                assetNames.Add(item);
                build.assetNames = assetNames.ToArray();
                packedAssets.AddRange(assetNames);
                builds.Add(build);
            }
        }
    }

	public class BuildAssetsWithScenes : BuildRule
    {
		#region implemented abstract members of BuildRule

		public override string GetAssetBundleName (string assetPath)
		{
            return BuildAssetBundleNameWithAssetPath(assetPath);
		}

		#endregion

        public BuildAssetsWithScenes()
        {

        }

        public BuildAssetsWithScenes(string path, string pattern, SearchOption option) : base(path, pattern, option)
        {

        }

        public override void Build()
        {
            var files = GetFilesWithoutPacked(searchPath, searchPattern, searchOption);

            for (int i = 0; i < files.Count; i++)
            {
                var item = files[i];
                if (UnityEditor.EditorUtility.DisplayCancelableProgressBar(string.Format("Packing... [{0}/{1}]", i, files.Count), item, i * 1f / files.Count))
                {
                    break;
                }
                AssetBundleBuild build = new AssetBundleBuild();
                build.assetBundleName = BuildAssetBundleNameWithAssetPath(item); 
                build.assetNames = new [] { item };
                packedAssets.AddRange(build.assetNames);
                builds.Add(build);
            }
        }
    }

}