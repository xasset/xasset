using System;
using UnityEngine;

namespace xasset.editor
{
    public class BuildCache : ScriptableObject
    {
        public BuildEntry[] data = Array.Empty<BuildEntry>();
    }
}