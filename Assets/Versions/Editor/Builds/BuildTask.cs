using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace VEngine.Editor.Builds
{
    public class BuildTask
    {
        private readonly BuildAssetBundleOptions buildAssetBundleOptions;
        private readonly List<Asset> bundledAssets = new List<Asset>();
        public readonly string name;
        private readonly Dictionary<string, Asset> pathWithAssets = new Dictionary<string, Asset>();

        public BuildTask()
        {
            name = nameof(Manifest);
            buildAssetBundleOptions = BuildAssetBundleOptions.ChunkBasedCompression |
                                      BuildAssetBundleOptions.AppendHashToAssetBundleName;
        }

        public Record record { get; private set; }

        private static string GetRecordsPath(string buildName)
        {
            return Settings.GetBuildPath($"build_records_for_{buildName}.json");
        }


        private static void WriteRecord(Record record)
        {
            var records = GetRecords(record.build);
            records.data.Insert(0, record);
            File.WriteAllText(GetRecordsPath(record.build), JsonUtility.ToJson(records));
        }

        private static Records GetRecords(string build)
        {
            var records = ScriptableObject.CreateInstance<Records>();
            var path = GetRecordsPath(build);
            if (File.Exists(path)) JsonUtility.FromJsonOverwrite(File.ReadAllText(path), records);

            return records;
        }

        private static void DisplayProgressBar(string title, string content, int index, int max)
        {
            EditorUtility.DisplayProgressBar($"{title}({index}/{max}) ", content,
                index * 1f / max);
        }

        public void BuildBundles()
        {
            var assetBundleNames = AssetDatabase.GetAllAssetBundleNames();
            for (var i = 0; i < assetBundleNames.Length; i++)
            {
                var assetBundleName = assetBundleNames[i];
                DisplayProgressBar("采集资源", assetBundleName, i, assetBundleNames.Length);
                var assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);
                bundledAssets.AddRange(Array.ConvertAll(assetNames, input => new Asset
                {
                    path = input,
                    bundle = assetBundleName
                }));
            }

            CheckAssets();
            EditorUtility.ClearProgressBar();
            FinishBuild();
        }

        private void CheckAssets()
        {
            for (var i = 0; i < bundledAssets.Count; i++)
            {
                var asset = bundledAssets[i];
                if (!pathWithAssets.TryGetValue(asset.path, out var ba))
                {
                    pathWithAssets[asset.path] = asset;
                }
                else
                {
                    bundledAssets.RemoveAt(i);
                    i--;
                    Debug.LogWarningFormat("{0} can't pack with {1}, because already pack to {2}", asset.path,
                        asset.bundle, ba.bundle);
                }
            }
        }

        private void FinishBuild()
        {
            var bundles = new List<ManifestBundle>();
            var dictionary = new Dictionary<string, List<string>>();
            foreach (var asset in bundledAssets)
            {
                if (!dictionary.TryGetValue(asset.bundle, out var assets))
                {
                    assets = new List<string>();
                    dictionary.Add(asset.bundle, assets);
                    bundles.Add(new ManifestBundle
                    {
                        name = asset.bundle,
                        assets = assets
                    });
                }

                assets.Add(asset.path);
            }

            var outputPath = Settings.PlatformBuildPath;
            if (bundles.Count <= 0) return;
            var manifest = BuildPipeline.BuildAssetBundles(outputPath, bundles.ConvertAll(bundle =>
                    new AssetBundleBuild
                    {
                        assetNames = bundle.assets.ToArray(),
                        assetBundleName = bundle.name
                    }).ToArray(),
                buildAssetBundleOptions | BuildAssetBundleOptions.AppendHashToAssetBundleName,
                EditorUserBuildSettings.activeBuildTarget);

            if (manifest == null)
            {
                Debug.LogErrorFormat("Failed to build {0}.", name);
                return;
            }

            AfterBuildBundles(bundles, manifest);
        }


        private string GetOriginBundle(string assetBundle)
        {
            var pos = assetBundle.LastIndexOf("_", StringComparison.Ordinal) + 1;
            var hash = assetBundle.Substring(pos);
            var originBundle = $"{assetBundle.Replace("_" + hash, "")}";
            return originBundle;
        }

        private void AfterBuildBundles(List<ManifestBundle> bundles,
            AssetBundleManifest manifest)
        {
            var nameWithBundles = new Dictionary<string, ManifestBundle>();
            for (var i = 0; i < bundles.Count; i++)
            {
                var bundle = bundles[i];
                bundle.id = i;
                nameWithBundles[bundle.name] = bundle;
            }

            if (manifest != null)
            {
                var assetBundles = manifest.GetAllAssetBundles();
                foreach (var assetBundle in assetBundles)
                {
                    var originBundle = GetOriginBundle(assetBundle);
                    var dependencies =
                        Array.ConvertAll(manifest.GetAllDependencies(assetBundle), GetOriginBundle);
                    if (nameWithBundles.TryGetValue(originBundle, out var manifestBundle))
                    {
                        manifestBundle.nameWithAppendHash = assetBundle;
                        manifestBundle.dependencies =
                            Array.ConvertAll(dependencies, input => nameWithBundles[input].id);
                        var file = Settings.GetBuildPath(assetBundle);
                        if (File.Exists(file))
                            using (var stream = File.OpenRead(file))
                            {
                                manifestBundle.size = stream.Length;
                                manifestBundle.crc = Utility.ComputeCRC32(stream);
                            }
                        else
                            Debug.LogErrorFormat("File not found: {0}", file);
                    }
                    else
                    {
                        Debug.LogErrorFormat("Bundle not exist: {0}", originBundle);
                    }
                }
            }

            CreateManifest(bundles);
        }

        private void CreateManifest(List<ManifestBundle> bundles)
        {
            var manifest = Settings.GetManifest();
            manifest.version++;
            manifest.appVersion = UnityEditor.PlayerSettings.bundleVersion;
            var getBundles = manifest.GetBundles();
            var newFiles = new List<string>();
            var newSize = 0L;
            foreach (var bundle in bundles)
                if (!getBundles.TryGetValue(bundle.name, out var value) ||
                    value.nameWithAppendHash != bundle.nameWithAppendHash)
                {
                    newFiles.Add(bundle.nameWithAppendHash);
                    newSize += bundle.size;
                }

            manifest.bundles = bundles;
            var newFilesSize = Utility.FormatBytes(newSize);
            newFiles.AddRange(WriteManifest(manifest));
            // write upload files
            var filename = Settings.GetBuildPath($"upload_files_for_{manifest.name}.txt");
            File.WriteAllText(filename, string.Join("\n", newFiles.ToArray()));
            record = new Record
            {
                build = name,
                version = manifest.version,
                files = newFiles,
                size = newSize,
                time = DateTime.Now.ToFileTime()
            };
            WriteRecord(record);
            Debug.LogFormat("Build bundles with {0}({1}) files with version {2} for {3}.", newFiles.Count, newFilesSize,
                manifest.version, manifest.name);
        }


        private static IEnumerable<string> WriteManifest(Manifest manifest)
        {
            var newFiles = new List<string>();
            var filename = $"{manifest.name}";
            var version = manifest.version;
            WriteJson(manifest, filename, newFiles);
            var path = Settings.GetBuildPath(filename);
            var crc = Utility.ComputeCRC32(path);
            var info = new FileInfo(path);
            WriteJson(manifest, $"{filename}_v{version}_{crc}", newFiles);
            // for version file
            var manifestVersion = ScriptableObject.CreateInstance<ManifestVersion>();
            manifestVersion.crc = crc;
            manifestVersion.size = info.Length;
            manifestVersion.version = version;
            manifestVersion.appVersion = manifest.appVersion;
            WriteJson(manifestVersion, Manifest.GetVersionFile(filename), newFiles);
            WriteJson(manifestVersion, $"{filename}_v{version}_{crc}.version", newFiles);
            return newFiles;
        }

        private static void WriteJson(ScriptableObject so, string file, List<string> newFiles)
        {
            newFiles.Add(file);
            var json = JsonUtility.ToJson(so);
            File.WriteAllText(Settings.GetBuildPath(file), json);
        }
    }
}