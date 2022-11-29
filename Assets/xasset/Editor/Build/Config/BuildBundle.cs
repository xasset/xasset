using System;
using System.Collections.Generic;

namespace xasset.editor
{
    [Serializable]
    public class BuildBundle
    {
        public int id;
        public int[] deps = Array.Empty<int>();
        public string group;
        public string hash;
        public string file;
        public ulong size;
        public List<string> assets = new List<string>();
    }
}