namespace xasset.editor
{
    public interface IAssetBundleManifest
    {
        string[] GetAllAssetBundles();
        string[] GetAllDependencies(string assetBundle);
        string GetAssetBundleHash(string assetBundle);
    }
}