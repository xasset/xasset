using System;
using UnityEngine;

namespace xasset.editor
{
    public class BuildAssets : ScriptableObject
    {
        public BuildAsset[] bundledAssets = Array.Empty<BuildAsset>();
    }
}