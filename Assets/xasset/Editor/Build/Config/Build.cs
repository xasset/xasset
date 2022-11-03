using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace xasset.editor
{
    [Serializable]
    public class BuildAsset
    {
        public string path;
        public string bundle;
        public string type;
        public string entry;
        public bool auto;
        public Group group { get; set; }
    }

    [Serializable]
    public class BuildBundle
    {
        public int id;
        public int[] deps = Array.Empty<int>();
        public string name;
        public string hash;
        public string nameWithAppendHash;
        public ulong size;
        public List<string> assets = new List<string>();
    }

    [Serializable]
    public class BuildParameters
    {
        public int buildNumber;
        public bool autoGrouping = true;
        public bool forceRebuild;
        public BuildAssetBundleOptions options = BuildAssetBundleOptions.ChunkBasedCompression;
        public Group[] groups;
        public string build { get; set; }
    }

    [CreateAssetMenu(menuName = "xasset/" + nameof(Build), fileName = nameof(Build))]
    public class Build : ScriptableObject
    {
        public BuildParameters parameters;
    }
}