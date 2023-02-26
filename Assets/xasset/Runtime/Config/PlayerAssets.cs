using System.Collections.Generic;
using UnityEngine;

namespace xasset
{
    public class PlayerAssets : ScriptableObject
    {
        public static readonly string Filename = $"{nameof(PlayerAssets).ToLower()}.json";
        public string version;
        public string updateInfoURL;
        public string downloadURL;
        public bool offlineMode;
        public List<string> data = new List<string>();
        public byte maxRetryTimes;
        public byte maxDownloads;

        public bool Contains(string key)
        {
            return data.Contains(key);
        }
    }
}