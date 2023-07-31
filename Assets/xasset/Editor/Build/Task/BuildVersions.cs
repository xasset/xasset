using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace xasset.editor
{
    public class BuildVersions : IBuildStep
    {
        public void Start(BuildTask task)
        {
            var versions = Settings.GetDefaultVersions();
            var version = versions.Get(task.parameters.name);
            var manifest = Utility.LoadFromFile<Manifest>(Settings.GetCachePath(version.file));
            if (!UpdateManifest(task, manifest)) return;
            SaveVersions(task, version, manifest, versions);
        }

        private static void SaveVersions(BuildTask task, Version version, Manifest manifest, Versions versions)
        {
            if (task.parameters.buildNumber > 0)
                version.ver = task.parameters.buildNumber;
            else
                version.ver++;
            version.name = task.parameters.name;
            var json = JsonUtility.ToJson(manifest);
            var bytes = Encoding.UTF8.GetBytes(json);
            version.hash = Utility.ComputeHash(bytes);
            var file = version.GetFilename();
            var path = Settings.GetCachePath(file);
            File.WriteAllText(path, json);
            // save version
            var info = new FileInfo(path);
            version.file = file;
            version.size = (ulong)info.Length;
            versions.Set(version);
            versions.Save(Settings.GetCachePath(Versions.Filename));
        }

        private static bool UpdateManifest(BuildTask task, Manifest manifest)
        {
            var getBundles = new Dictionary<string, ManifestBundle>();
            foreach (var bundle in manifest.bundles) getBundles[bundle.name] = bundle;
            var dirs = new List<string>();
            var assets = new List<ManifestAsset>();
            for (var index = 0; index < task.bundles.Count; index++)
            {
                var bundle = task.bundles[index];
                foreach (var asset in bundle.assets)
                {
                    var entry = task.GetAsset(asset);
                    if (entry.addressMode == AddressMode.LoadByDependencies) continue;

                    var dir = Path.GetDirectoryName(asset)?.Replace("\\", "/");
                    var pos = dirs.IndexOf(dir);
                    if (pos == -1)
                    {
                        pos = dirs.Count;
                        dirs.Add(dir);
                    }

                    var manifestAsset = new ManifestAsset
                    {
                        id = assets.Count,
                        name = Path.GetFileName(asset),
                        path = asset,
                        bundle = index,
                        dir = pos,
                        addressMode = entry.addressMode
                    };
                    assets.Add(manifestAsset);
                }

                if (getBundles.TryGetValue(bundle.group, out var value) && value.hash == bundle.hash &&
                    value.size == bundle.size) continue;

                task.changes.Add(bundle.file);
            }

            if (task.changes.Count == 0 && manifest.time == task.buildLastTimeTime && task.bundles.Count == getBundles.Count)
            {
                task.error = "Nothing to build.";
                task.nothingToBuild = true;
                return false;
            }

            var map = assets.ToDictionary(a => a.path);
            foreach (var asset in assets)
            {
                var dependencies = Settings.GetDependencies(asset.path);
                var deps = new List<int>();
                foreach (var dependency in dependencies)
                    if (map.TryGetValue(dependency, out var dep))
                        deps.Add(dep.id);

                asset.deps = deps.ToArray();
            }

            manifest.Clear();
            manifest.bundles = task.bundles.ConvertAll(Converter).ToArray();
            manifest.assets = assets.ToArray();
            manifest.dirs = dirs.ToArray();
            manifest.name = task.parameters.name;
            manifest.time = task.buildLastTimeTime;

            BuildGroups(manifest, task);
            return true;
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

        private static void BuildGroups(Manifest manifest, BuildTask task)
        {
            manifest.groups = Array.Empty<ManifestGroup>();
            manifest.OnAfterDeserialize();
            var manifestGroups = new List<ManifestGroup>();
            var packed = new HashSet<int>();
            var groups = new List<BuildGroup>();
            groups.AddRange(task.groups);
            groups.Sort((a, b) => a.id.CompareTo(b.id));
            foreach (var group in groups)
            {
                if (!group.enabled) continue;
                var set = new HashSet<int>();
                var assets = new HashSet<string>();
                foreach (var entry in group.assets)
                    if (!Directory.Exists(entry.asset))
                        assets.Add(entry.asset);
                    else
                        foreach (var child in Settings.GetChildren(entry))
                            assets.Add(child);

                foreach (var asset in assets)
                {
                    if (!manifest.TryGetAsset(asset, out var result)) continue;
                    set.Add(result.bundle); 
                }

                set.ExceptWith(packed);
                packed.UnionWith(set);
                if (set.Count <= 0) continue;
                var bundles = new List<int>(set);
                bundles.Sort();
                manifestGroups.Add(new ManifestGroup
                {
                    name = group.name,
                    desc = group.desc,
                    manifest = manifest,
                    assets = bundles,
                    deliveryMode = group.deliveryMode
                }); 
            } 
            manifest.groups = manifestGroups.ToArray();
        } 
    }
}