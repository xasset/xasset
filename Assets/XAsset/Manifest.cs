using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace XAsset
{
    public class Manifest
    {
        static readonly Dictionary<string, int> amap = new Dictionary<string, int>();
        static readonly Dictionary<string, List<int>> bmap = new Dictionary<string, List<int>>();

        public string[] allAssets { get; private set; }

        public string[] allBundles { get; private set; }

        public string GetBundleName(string assetPath)
        {
            return allBundles[amap[assetPath]];
        }

        public string[] GetBundleAssets(string bundleName)
        {
            return System.Array.ConvertAll<int, string>(bmap[bundleName].ToArray(), input =>
            {
                return allAssets[input];
            });
        }

        public string GetAssetName(string assetPath)
        {
            return Path.GetFileName(assetPath);
        }

        public Manifest()
        {
            Init();
        }

        void Init()
        {
            amap.Clear();
            bmap.Clear();

            allAssets = new string[0];
            allBundles = new string[0];
        }

        const string kBundles = "Bundles";
        const string kAssets = "Assets";
        const char kSpliter = ':';

        public void Load(TextReader reader)
        {
            Init();

            List<string> bundles = new List<string>();
            List<string> assets = new List<string>();

            string bundle = null;
            string line = null;
            while ((line = reader.ReadLine()) != null)
            {
                if (line == string.Empty)
                {
                    continue;
                }
                var fields = line.Split(kSpliter);
                if (fields.Length > 1)
                {
                    bundle = fields[0];
                    bundles.Add(bundle);
                    bmap.Add(bundle, new List<int>());
                }
                else
                {
                    string asset = line.TrimStart('\t');
                    assets.Add(asset);
                    bmap[bundle].Add(assets.Count - 1);
                    amap[asset] = bundles.Count - 1;
                }
            }

            allBundles = bundles.ToArray();
            allAssets = assets.ToArray();
        }

#if UNITY_EDITOR
        public void Save(string path, List<AssetBundleBuild> builds)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            using (var writer = new StreamWriter(path))
            {
                foreach (var item in builds)
                {
                    writer.WriteLine(item.assetBundleName + kSpliter);
                    foreach (var asset in item.assetNames)
                    {
                        writer.WriteLine(string.Format("\t{0}", asset));
                    }
                    writer.WriteLine();
                }
                writer.Flush();
                writer.Close();
            }
        }

        public void Build(string path, List<AssetBundleBuild> builds, bool forceRebuild = false)
        {
            if (File.Exists(path))
            {
                using (var reader = new StreamReader(path))
                {
                    Load(reader);
                    reader.Close();
                }
            }

            Dictionary<string, string> newpaths = new Dictionary<string, string>();
            List<string> bundles = new List<string>();
            List<string> assets = new List<string>();
            bool dirty = false;
            if (builds.Count > 0)
            {
                foreach (var item in builds)
                {
                    bundles.Add(item.assetBundleName);
                    foreach (var assetPath in item.assetNames)
                    {
                        newpaths[assetPath] = item.assetBundleName;
                        assets.Add(assetPath + kSpliter + (bundles.Count - 1));
                    }
                }
            }

            if (newpaths.Count == amap.Count)
            {
                foreach (var item in newpaths)
                {
                    if (!amap.ContainsKey(item.Key) || !GetBundleName(item.Key).Equals(newpaths[item.Key]))
                    {
                        dirty = true;
                        break;
                    }
                }
            }
            else
            {
                dirty = true;
            }

            if (forceRebuild || dirty || !File.Exists(path))
            {
                Save(path, builds);
            }

            Debug.Log("[Manifest] Build " + assets.Count + " assets with " + bundles.Count + " bundels.");
        }
#endif
    }

}