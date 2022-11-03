using System;

namespace xasset
{
    [Serializable]
    public class ManifestBundle
    {
        public int[] deps = Array.Empty<int>();
        public string hash;
        public string name;
        public ulong size;
        public string nameWithAppendHash { get; set; }
    }
}