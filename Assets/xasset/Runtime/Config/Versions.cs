using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace xasset
{
    [Serializable]
    public class Version : Downloadable
    {
        public int ver;
        public Manifest manifest { get; set; }

        public string GetFilename()
        {
            return $"{name.ToLower()}_{hash}.json";
        }

        public Manifest Load(string path)
        {
            if (manifest != null)
                return manifest;

            var bundle = AssetBundle.LoadFromFile(path);
            if (bundle == null)
            {
                Debug.LogError($"File not found {path}");
                return ScriptableObject.CreateInstance<Manifest>();
            }

            var assetPath = $"Assets/{name}.asset";
            manifest = bundle.LoadAsset<Manifest>(assetPath);
            manifest.name = name;
            bundle.Unload(false);
            return manifest;
        }
    }

    public class Versions : ScriptableObject, ISerializationCallbackReceiver
    {
        public const string Filename = "versions.json";
        public const string BundleFilename = "bundle-versions.json";
        public long timestamp;
        public List<Version> data = new List<Version>();
        private Dictionary<string, Version> _data = new Dictionary<string, Version>();

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            _data = new Dictionary<string, Version>();
            foreach (var item in data)
            {
                item.file = item.GetFilename();
                _data[item.name] = item;
            }
        }

        public bool IsNew(Versions v)
        {
            return timestamp > v.timestamp;
        }

        public string GetFilename()
        {
            return $"versions_v{ToString()}.json";
        }

        public override string ToString()
        {
            data.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
            return string.Join(".", data.ConvertAll(v => v.ver));
        }

        public void Set(Version value)
        {
            if (!_data.ContainsKey(value.name)) data.Add(value);

            _data[value.name] = value;
            timestamp = DateTime.Now.ToFileTime();
        }

        public Version Get(string build)
        {
            return _data.TryGetValue(build, out var value) ? value : new Version();
        }

        public bool TryGetVersion(string build, out Version version)
        {
            return _data.TryGetValue(build, out version);
        }

        public void Save(string path)
        {
            File.WriteAllText(path, JsonUtility.ToJson(this));
        }

        public bool TryGetAssets(string path, out ManifestAsset[] assets)
        {
            foreach (var item in data)
            {
                var manifest = item.manifest;
                if (!manifest.IsDirectory(path))
                {
                    if (!manifest.TryGetAsset(path, out var value)) continue;
                    assets = new[]
                    {
                        value
                    };
                    return true;
                }

                assets = Array.ConvertAll(manifest.GetAssets(path, true), input =>
                {
                    var asset = manifest.assets[input];
                    return asset;
                });
                return true;
            }

            assets = null;
            return false;
        }

        public bool TryGetAsset(string path, out ManifestAsset asset)
        {
            foreach (var item in data)
            {
                if (!item.manifest.TryGetAsset(path, out var value)) continue;
                asset = value;
                return true;
            }

            asset = null;
            return false;
        }

        public bool TryGetGroups(string group, out ManifestGroup[] result)
        {
            var groups = new List<ManifestGroup>();
            foreach (var version in data)
                if (version.manifest.TryGetGroups(group, out var value))
                    groups.Add(value);
            result = groups.ToArray();
            return groups.Count > 0;
        }

        public ManifestBundle GetBundle(string bundle)
        {
            foreach (var version in data)
            {
                var result = version.manifest.GetBundle(bundle);
                if (result != null)
                    return result;
            }

            return null;
        }


        public GetDownloadSizeRequest GetDownloadSizeAsync(params string[] assets)
        {
            var request = new GetDownloadSizeRequest { assets = assets, versions = this };
            request.SendRequest();
            return request;
        }
    }
}