using UnityEngine;

namespace xasset
{
    public class UpdateInfo : ScriptableObject
    {
        public static readonly string Filename = $"{nameof(UpdateInfo).ToLower()}.json";
        public long timestamp;
        public string file;
        public string hash;
        public ulong size;
        public string downloadURL;
        public string playerURL;
        public string version;
    }
}