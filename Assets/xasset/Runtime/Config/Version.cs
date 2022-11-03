using System;

namespace xasset
{
    [Serializable]
    public class Version
    {
        public string build;
        public string file;
        public ulong size;
        public string hash;
        public int ver;
        public Manifest manifest { get; set; }
    }
}