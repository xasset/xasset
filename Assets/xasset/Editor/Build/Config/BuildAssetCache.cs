using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace xasset.editor
{
    public class BuildAssetCache : ScriptableObject, ISerializationCallbackReceiver
    {
        public static readonly string Filename = $"Assets/xasset/Config/{nameof(BuildAssetCache)}.asset";
        public List<BuildAsset> data = new List<BuildAsset>();
        private readonly Dictionary<string, BuildAsset> _data = new Dictionary<string, BuildAsset>();

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            foreach (var asset in data) _data[asset.path] = asset;
        } 

        public BuildAsset GetAsset(string path)
        {
            if (!_data.TryGetValue(path, out var value))
            {
                var type = AssetDatabase.GetMainAssetTypeAtPath(path);
                value = new BuildAsset
                {
                    path = path,
                    type = type == null ? "MissType" : type.Name
                };
                _data[path] = value;
                data.Add(value);
                EditorUtility.SetDirty(this);
            }

            BuildDependenciesIfNeed(value);

            return value;
        }

        private static ulong GetAssetSize(string path)
        {
            var file = new FileInfo(path);
            return (ulong) (file.Exists ? file.Length : 0);
        }

        private void BuildDependenciesIfNeed(BuildAsset asset)
        {
            if (Settings.FindReferences(asset))
            {
                var lastWriteTime = Settings.GetLastWriteTime(asset.path);
                if (asset.lastWriteTime != lastWriteTime)
                {
                    asset.dependencies = Settings.GetDependenciesWithoutCache(asset.path);
                    asset.lastWriteTime = lastWriteTime;
                    EditorUtility.SetDirty(this);
                }
            }

            GetAssetSize(asset.path);
        }

        public string[] GetDependencies(string path)
        {
            return GetAsset(path).dependencies;
        }
    }
}