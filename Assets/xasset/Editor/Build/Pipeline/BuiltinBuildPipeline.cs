using UnityEditor;

namespace xasset.editor
{
    public class BuiltinBuildPipeline : IBuildPipeline
    {
        public IAssetBundleManifest BuildAssetBundles(string outputPath, AssetBundleBuild[] builds,
            BuildAssetBundleOptions options, BuildTarget target)
        {
            var manifest = BuildPipeline.BuildAssetBundles(outputPath, builds, options, target);
            return manifest != null ? new BuiltinAssetBundleManifest(manifest) : null;
        }
    }
}