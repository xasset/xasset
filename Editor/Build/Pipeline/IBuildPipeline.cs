using UnityEditor;

namespace xasset.editor
{
    public interface IBuildPipeline
    {
        IAssetBundleManifest BuildAssetBundles(string outputPath, AssetBundleBuild[] builds,
            BuildAssetBundleOptions options, BuildTarget target);
    }
}