using UnityEngine;

namespace xasset.editor
{
    public class BuiltinAssetBundleManifest : IAssetBundleManifest
    {
        private readonly AssetBundleManifest _manifest;

        public BuiltinAssetBundleManifest(AssetBundleManifest manifest)
        {
            _manifest = manifest;
        }

        public string[] GetAllAssetBundles()
        {
            return _manifest.GetAllAssetBundles();
        }

        public string[] GetAllDependencies(string assetBundle)
        {
            return _manifest.GetAllDependencies(assetBundle);
        }

        public string GetAssetBundleHash(string assetBundle)
        {
            return _manifest.GetAssetBundleHash(assetBundle).ToString();
        }
    }
}