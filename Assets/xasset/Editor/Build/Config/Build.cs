using System;
using UnityEditor;
using UnityEngine;

namespace xasset.editor
{
    [Serializable]
    public class BuildParameters
    {
        public int buildNumber;
        public bool optimizeDependentAssets = true;
        public bool forceRebuild;
        public BuildAssetBundleOptions options = BuildAssetBundleOptions.ChunkBasedCompression;
        public BuildGroup[] groups;
        public string name { get; set; }
    }

    [CreateAssetMenu(menuName = "xasset/" + nameof(Build), fileName = nameof(Build))]
    public class Build : ScriptableObject
    {
        public BuildParameters parameters;
    }
}