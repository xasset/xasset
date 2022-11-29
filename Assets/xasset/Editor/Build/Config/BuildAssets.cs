using UnityEngine;

namespace xasset.editor
{
    public class BuildAssets : ScriptableObject
    {
        public BuildAsset[] bundledAssets;
        public BuildAsset[] rawAssets;
    }
}