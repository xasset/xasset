using System.Collections.Generic;

namespace xasset.editor
{
    public interface IAssetBundleManifest
    {
        IEnumerable<string> GetAllAssetBundles();
        string[] GetDependencies(string assetBundle);
        string GetAssetBundleHash(string assetBundle);
    }
}