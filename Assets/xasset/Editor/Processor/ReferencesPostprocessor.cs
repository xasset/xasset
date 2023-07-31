using UnityEditor;

namespace xasset.editor
{
    public class ReferencesPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            for (var i = 0; i < movedAssets.Length; i++)
            {
                Settings.MoveAsset(movedAssets[i], movedFromAssetPaths[i]);
            } 
            
            foreach (var deletedAsset in deletedAssets) Settings.DeleteAsset(deletedAsset);
        }
    }
}