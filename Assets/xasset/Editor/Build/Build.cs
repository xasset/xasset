using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace xasset.editor
{
    /// <summary>
    ///     打包模式
    /// </summary>
    public enum BundleMode
    {
        /// <summary>
        ///     根据 Group 名字打包到一起
        /// </summary>
        PackTogether,
        PackByFile,
        PackByFileWithoutExtension, // 不带文件扩张名
        PackByFolder, // deep mode
        PackByFolderTopOnly,
        PackByRaw,
        PackByCustom,
    }

    /// <summary>
    ///     打包节点
    /// </summary>
    [Serializable]
    public class BuildEntry
    {
        /// <summary>
        ///     资产路径，可以是文件或者文件夹
        /// </summary>
        [ObjectReference] public string asset;

        /// <summary>
        ///     打包模式
        /// </summary>
        public BundleMode bundleMode = BundleMode.PackTogether;

        /// <summary>
        ///     寻址模式
        /// </summary>
        public AddressMode addressMode = AddressMode.LoadByPath;

        /// <summary>
        ///     资产路径是目录的时候有效，用法参考 AssetDatabase.FindAssets
        /// </summary>
        public string filter;

        /// <summary>
        ///     根据打包模式生成的 bundle 名字
        /// </summary>
        [HideInInspector] public string bundle;

        /// <summary>
        ///     节点所在的组
        /// </summary>
        [NonSerialized] public BuildGroup owner;

        /// <summary>
        ///     父节点
        /// </summary>
        [NonSerialized] public string parent;
    }

    /// <summary>
    ///     打包分组，相同生命周期的打包节点可以用一个打包分组管理起来。分组名字，可以用作运行时获取更新的参数。
    /// </summary>
    [Serializable]
    public class BuildGroup
    {
        public string name = "New Group";
        public int id;
        public bool enabled = true;
        public DeliveryMode deliveryMode = DeliveryMode.OnDemand;
        public BuildEntry[] assets = Array.Empty<BuildEntry>();
        [TextArea] public string desc;
        public string build { get; set; }
        /// <summary>
        ///     节点所在的组
        /// </summary>
        [NonSerialized] public Build owner;
    }

    [Serializable]
    public class BuildParameters
    {
        public int buildNumber;
        public bool optimizeDependencies = true;

        public BuildAssetBundleOptions options = BuildAssetBundleOptions.ChunkBasedCompression |
                                                 BuildAssetBundleOptions.DisableLoadAssetByFileName |
                                                 BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension;

        public string name { get; set; }
    }

    public class BuildBundle
    {
        public readonly List<string> assets = new List<string>();
        public int[] deps = Array.Empty<int>();
        public string file;
        public string group;
        public string hash;
        public int id;
        public ulong size;
    }

    [CreateAssetMenu(menuName = "xasset/" + nameof(Build), fileName = nameof(Build))]
    public class Build : ScriptableObject
    {
        public int id;
        public bool enabled = true;
        public BuildParameters parameters;
        public BuildGroup[] groups = Array.Empty<BuildGroup>();
    }
}