using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace xasset
{
    public class Versions : ScriptableObject, ISerializationCallbackReceiver
    {
        public const string Filename = "versions.json";
        public long timestamp;
        public List<Version> data = new List<Version>();
        private Dictionary<string, Version> _data = new Dictionary<string, Version>();

        public void OnBeforeSerialize()
        {
            data.Clear();
            data.AddRange(_data.Values);
            data.Sort((x, y) => string.Compare(x.build, y.build, StringComparison.Ordinal));
        }

        public void OnAfterDeserialize()
        {
            _data = new Dictionary<string, Version>();
            foreach (var item in data) _data[item.build] = item;
        }

        public string GetFilename()
        {
            return $"versions_v{this}.json";
        }

        public override string ToString()
        {
            data.Sort((a, b) => string.Compare(a.build, b.build, StringComparison.Ordinal));
            return string.Join(".", data.ConvertAll(v => v.ver));
        }


        public void Set(Version value)
        {
            _data[value.build] = value;
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
    }
}