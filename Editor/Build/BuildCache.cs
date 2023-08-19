using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace xasset.editor
{
    public class BuildCache : ScriptableObject, ISerializationCallbackReceiver
    {
        public List<BuildEntry> data = new List<BuildEntry>();

        private readonly Dictionary<string, BuildEntry> assets = new Dictionary<string, BuildEntry>();

        public BuildEntry GetAsset(string asset)
        {
            if (assets.TryGetValue(asset, out var result)) return result;
            result = new BuildEntry { asset = asset };
            assets.Add(asset, result);
            data.Add(result);
            EditorUtility.SetDirty(this);
            return result;
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            assets.Clear();
            foreach (var entry in data)
                assets[entry.asset] = entry;
        }
    }
}