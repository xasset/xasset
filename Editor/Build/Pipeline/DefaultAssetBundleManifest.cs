using System.Collections.Generic;
using UnityEngine;

namespace xasset.editor
{
    public class DefaultAssetBundleManifest : IAssetBundleManifest
    {
        private readonly AssetBundleManifest _manifest;

        public DefaultAssetBundleManifest(AssetBundleManifest manifest)
        {
            _manifest = manifest;
        }

        public IEnumerable<string> GetAllAssetBundles()
        {
            return _manifest.GetAllAssetBundles();
        }

        public string[] GetDependencies(string assetBundle)
        {
            return _manifest.GetDirectDependencies(assetBundle);
        }

        public string GetAssetBundleHash(string assetBundle)
        {
            return _manifest.GetAssetBundleHash(assetBundle).ToString();
        }
    }
}