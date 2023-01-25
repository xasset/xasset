using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace xasset.editor
{
    public class BuildVersions : IBuildJobStep
    {
        public void Start(BuildJob job)
        {
            var versions = Settings.GetDefaultVersions();
            var version = versions.Get(job.parameters.name);
            var manifest = Utility.LoadFromFile<Manifest>(Settings.GetDataPath(version.file));
            if (!UpdateManifest(job, manifest)) return;
            SaveVersions(job, version, manifest, versions);
        }

        private static void SaveVersions(BuildJob job, Version version, Manifest manifest, Versions versions)
        {
            if (job.parameters.buildNumber > 0)
                version.ver = job.parameters.buildNumber;
            else
                version.ver++;
            version.name = job.parameters.name;
            var json = JsonUtility.ToJson(manifest);
            var bytes = Encoding.UTF8.GetBytes(json);
            version.hash = Utility.ComputeHash(bytes);
            var file = version.GetFilename();
            var path = Settings.GetDataPath(file);
            File.WriteAllText(path, json);
            job.changes.Add(file);
            // save version
            var info = new FileInfo(path);
            version.file = file;
            version.size = (ulong) info.Length;
            versions.Set(version);
            for (var index = 0; index < versions.data.Count; index++)
            {
                var item = versions.data[index];
                if (File.Exists(Settings.GetDataPath(item.file))) continue;
                versions.data.RemoveAt(index);
                index--;
            }

            versions.Save(Settings.GetCachePath(Versions.Filename));
        }

        private static bool UpdateManifest(BuildJob job, Manifest manifest)
        {
            var getBundles = new Dictionary<string, ManifestBundle>();
            foreach (var bundle in manifest.bundles) getBundles[bundle.name] = bundle;
            var dirs = new List<string>();
            var assets = new List<ManifestAsset>();
            for (var index = 0; index < job.bundles.Count; index++)
            {
                var bundle = job.bundles[index];
                AddAsset(bundle, dirs, index, assets);
                if (getBundles.TryGetValue(bundle.group, out var value) && value.hash == bundle.hash &&
                    value.size == bundle.size) continue;

                job.changes.Add(bundle.file);
            }

            if (job.changes.Count == 0 && !job.parameters.forceRebuild && job.bundles.Count == getBundles.Count)
            {
                job.error = "Nothing to build.";
                return false;
            }

            var map = assets.ToDictionary(a => a.path);
            foreach (var asset in assets)
            {
                var dependencies = Settings.GetDependencies(asset.path);
                var deps = new List<int>();
                foreach (var dependency in dependencies)
                {
                    if (map.TryGetValue(dependency, out var dep))
                    {
                        deps.Add(dep.id);
                    }
                }

                asset.deps = deps.ToArray();
            }

            var settings = Settings.GetDefaultSettings();
            manifest.Clear();
            manifest.saveBundleName = settings.bundle.saveBundleName;
            manifest.extension = settings.bundle.extension;
            manifest.bundles = job.bundles.ConvertAll(Converter).ToArray();
            manifest.assets = assets.ToArray();
            manifest.dirs = dirs.ToArray();
            manifest.build = job.parameters.name;
            return true;
        }

        private static void AddAsset(BuildBundle bundle, IList<string> dirs, int index, ICollection<ManifestAsset> assets)
        {
            foreach (var asset in bundle.assets)
            {
                var buildAsset = Settings.GetAsset(asset);
                if (buildAsset.addressMode == AddressMode.LoadByDependencies)
                {
                    continue;
                }

                var dir = Path.GetDirectoryName(asset)?.Replace("\\", "/");
                var pos = dirs.IndexOf(dir);
                if (pos == -1)
                {
                    pos = dirs.Count;
                    dirs.Add(dir);
                }

                var manifestAsset = new ManifestAsset
                {
                    path = asset,
                    name = Path.GetFileName(asset),
                    bundle = index,
                    dir = pos,
                    addressMode = buildAsset.addressMode,
                    id = assets.Count
                };
                assets.Add(manifestAsset);
            }
        }

        private static ManifestBundle Converter(BuildBundle input)
        {
            return new ManifestBundle
            {
                name = input.group,
                size = input.size,
                hash = input.hash,
                deps = input.deps
            };
        }
    }
}