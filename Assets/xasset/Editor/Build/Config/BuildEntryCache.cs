using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace xasset.editor
{
    [Serializable]
    public class BuildEntry
    {
        public string path;
        public long lastWriteTime;
        public long lastWriteTimeForGroup;
        public List<BuildAsset> assets = new List<BuildAsset>();
        public BuildGroup group;

        private Dictionary<string, BuildAsset> _assets = new Dictionary<string, BuildAsset>();

        public void OnAfterDeserialize()
        {
            foreach (var asset in assets)
            {
                _assets[asset.path] = asset;
            }
        }

        public void AddAsset(string assetPath)
        {
            if (Settings.customFilter != null && !Settings.customFilter(assetPath))
            {
                return;
            }

            if (_assets.TryGetValue(assetPath, out _))
            {
                Logger.W(
                    $"Failed to add {assetPath} to assets with group {group.name} with entry {path}, because which is already exist.");
                return;
            }

            var asset = Settings.GetAsset(assetPath);
            asset.entry = path;
            asset.group = group;
            asset.addressMode = group.addressMode;
            assets.Add(asset);
            _assets.Add(assetPath, asset);
        }

        public void Clear()
        {
            assets.Clear();
            _assets.Clear();
        }
    }

    public class BuildEntryCache : ScriptableObject, ISerializationCallbackReceiver
    {
        public static readonly string Filename = $"Assets/xasset/Config/{nameof(BuildEntryCache)}.asset";
        public List<BuildEntry> data = new List<BuildEntry>();
        private readonly Dictionary<string, BuildEntry> _data = new Dictionary<string, BuildEntry>();


        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            foreach (var asset in data)
            {
                _data[asset.path] = asset;
                asset.OnAfterDeserialize();
            }
        }

        public BuildEntry GetEntry(string path, BuildGroup group)
        {
            if (!_data.TryGetValue(path, out var value))
            {
                value = new BuildEntry {path = path};
                _data[path] = value;
                data.Add(value);
            }

            if (UpdateEntry(value, group))
            {
                EditorUtility.SetDirty(this);
            }

            return value;
        }

        private static bool UpdateEntry(BuildEntry value, BuildGroup group)
        {
            var lastWriteTimeForGroup = Settings.GetLastWriteTime(AssetDatabase.GetAssetPath(group));
            var lastWriteTime = Settings.GetLastWriteTime(value.path);
            // if (value.group == group && value.lastWriteTime == lastWriteTimeForGroup && value.lastWriteTimeForGroup == lastWriteTimeForGroup)
            //     return false;
            value.lastWriteTimeForGroup = lastWriteTimeForGroup;
            value.lastWriteTime = lastWriteTime;
            value.group = group;
            value.Clear();

            if (Directory.Exists(value.path))
            {
                var guilds = AssetDatabase.FindAssets(group.filter, new[]
                {
                    value.path
                });
                var set = new HashSet<string>();
                var exclude = Settings.GetDefaultSettings().bundle.excludeFiles;
                foreach (var guild in guilds)
                {
                    var child = AssetDatabase.GUIDToAssetPath(guild);
                    if (string.IsNullOrEmpty(child) || exclude.Exists(child.EndsWith)
                                                    || Directory.Exists(child)
                                                    || set.Contains(child)) continue;
                    set.Add(child);
                    value.AddAsset(child);
                }
            }
            else
            {
                value.AddAsset(value.path);
            }

            return true;
        }
    }
}