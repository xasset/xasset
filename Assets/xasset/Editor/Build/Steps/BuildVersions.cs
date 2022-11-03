using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace xasset.editor
{
    public class BuildVersions : IBuildJobStep
    {
        public void Start(BuildJob job)
        {
            var versions = Settings.GetDefaultVersions();
            var version = versions.Get(job.parameters.build);
            var manifest = Utility.LoadFromFile<Manifest>(Settings.GetDataPath(version.file));
            if (!BuildManifest(job, manifest)) return;
            if (job.parameters.buildNumber > 0)
                version.ver = job.parameters.buildNumber;
            else
                version.ver++;
            var json = JsonUtility.ToJson(manifest);
            var bytes = Encoding.UTF8.GetBytes(json);
            var hash = Utility.ComputeHash(bytes);
            var buildToLower = manifest.build.ToLower();
            var file = $"{buildToLower}_{hash}.json";
            File.WriteAllText(Settings.GetDataPath(file), json);
            job.changes.Add(file);
            // save version
            var info = new FileInfo(Settings.GetDataPath(file));
            version.build = job.parameters.build;
            version.file = file;
            version.size = (ulong) info.Length;
            version.hash = hash;
            versions.Set(version);
            versions.Save(Settings.GetCachePath(Versions.Filename));
        }

        private static bool BuildManifest(BuildJob job, Manifest manifest)
        {
            var getBundles = new Dictionary<string, ManifestBundle>();
            foreach (var bundle in manifest.bundles) getBundles[bundle.name] = bundle;
            var dirs = new List<string>();
            var assets = new List<ManifestAsset>();
            for (var index = 0; index < job.bundles.Count; index++)
            {
                var bundle = job.bundles[index];
                foreach (var asset in bundle.assets)
                {
                    var dir = Path.GetDirectoryName(asset)?.Replace("\\", "/");
                    var pos = dirs.IndexOf(dir);
                    if (pos == -1)
                    {
                        pos = dirs.Count;
                        dirs.Add(dir);
                    }

                    var manifestAsset = new ManifestAsset
                    {
                        name = Path.GetFileName(asset),
                        bundle = index,
                        dir = pos,
                        id = assets.Count
                    };
                    assets.Add(manifestAsset);
                }

                if (getBundles.TryGetValue(bundle.name, out var value) && value.hash == bundle.hash &&
                    value.size == bundle.size) continue;

                job.changes.Add(bundle.nameWithAppendHash);
            }

            if (job.changes.Count == 0 && !job.parameters.forceRebuild && job.bundles.Count == getBundles.Count)
            {
                job.error = "Nothing to build.";
                return false;
            }

            manifest.bundles = job.bundles.ConvertAll(input => new ManifestBundle
            {
                name = input.name,
                size = input.size,
                hash = input.hash,
                deps = input.deps
            }).ToArray();
            manifest.assets = assets.ToArray();
            manifest.dirs = dirs.ToArray();
            manifest.build = job.parameters.build;
            return true;
        }
    }
}