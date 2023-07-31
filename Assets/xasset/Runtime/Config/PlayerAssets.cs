using System.Collections.Generic;
using UnityEngine;

namespace xasset
{
    /// <summary>
    ///     安装包资源分包模式
    /// </summary>
    public enum PlayerAssetsSplitMode
    {
        /// <summary>
        ///     安装包含交付模式为 InstallTime 的所有资源。
        /// </summary>
        IncludeInstallTimeAssetsOnly,

        /// <summary>
        ///     安装包包含所有资源。
        /// </summary>
        IncludeAllAssets,

        /// <summary>
        ///     安装包不包含资源。
        /// </summary>
        ExcludeAllAssets
    } 

    public class PlayerAssets : ScriptableObject, ISerializationCallbackReceiver
    {
        public static readonly string Filename = $"{nameof(PlayerAssets).ToLower()}.json";
        public static readonly string AssetsName = $"{nameof(PlayerAssets).ToLower()}.assets";
        public LogLevel logLevel = LogLevel.Debug;
        public PlayerAssetsSplitMode splitMode = PlayerAssetsSplitMode.ExcludeAllAssets;
        public byte maxDownloads = 5;
        public byte maxRetryTimes = 3;
        public bool updatable;
        public string version;
        public string updateInfoURL;
        public string downloadURL;
        public byte maxRequests;
        public bool autoslicing;
        public float autoslicingTimestep;
        public float autoreleaseTimestep;
        public List<string> data = new List<string>(); 


        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        { 
        }

        public bool Contains(string key)
        {
            return data.Contains(key);
        }
    }
}