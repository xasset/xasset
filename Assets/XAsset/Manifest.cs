using System.Collections.Generic;
using System.IO;

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

        public bool ContainsBundle(string bundle)
        {
            return bmap.ContainsKey(bundle);
        }

        public bool ContainsAsset (string assetPath)
        {
            return amap.ContainsKey(assetPath);
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

        void Init()
        {
            amap.Clear();
            bmap.Clear();

            allAssets = new string[0];
            allBundles = new string[0];
        } 

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
                var fields = line.Split(':');
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
    }

}