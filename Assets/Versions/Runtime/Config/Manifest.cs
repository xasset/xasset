using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VEngine
{
    [Serializable]
    public class ManifestBundle
    {
        public static ManifestBundle Empty = new ManifestBundle();
        public int id;
        public string name;
        public List<string> assets;
        public long size;
        public uint crc;
        public string nameWithAppendHash;
        public int[] dependencies;
    }

    public class Manifest : ScriptableObject
    {
        public int version;
        public string appVersion;
        public List<ManifestBundle> bundles = new List<ManifestBundle>();
        private readonly Dictionary<string, ManifestBundle> nameWithBundles = new Dictionary<string, ManifestBundle>();

        public Dictionary<string, ManifestBundle> GetBundles()
        {
            var dictionary = new Dictionary<string, ManifestBundle>();
            foreach (var bundle in bundles) dictionary[bundle.name] = bundle;

            return dictionary;
        }

        public bool Contains(string assetPath)
        {
            return nameWithBundles.ContainsKey(assetPath);
        }

        public ManifestBundle GetBundle(string assetPath)
        {
            ManifestBundle manifestBundle;
            return nameWithBundles.TryGetValue(assetPath, out manifestBundle) ? manifestBundle : null;
        }

        public ManifestBundle[] GetDependencies(ManifestBundle bundle)
        {
            if (bundle == null) return Array.Empty<ManifestBundle>();

            return Array.ConvertAll(bundle.dependencies, input => bundles[input]);
        }

        public static string GetVersionFile(string file)
        {
            return $"{file}.version";
        }

        public void Load(string path)
        {
            var json = File.ReadAllText(path);
            JsonUtility.FromJsonOverwrite(json, this);
            nameWithBundles.Clear();
            foreach (var bundle in bundles)
            {
                nameWithBundles[bundle.nameWithAppendHash] = bundle;
                foreach (var asset in bundle.assets)
                {
                    nameWithBundles[asset] = bundle;
                }
            }
        }

        public void AddAsset(string assetPath)
        {
            nameWithBundles[assetPath] = ManifestBundle.Empty;
        }
    }
}