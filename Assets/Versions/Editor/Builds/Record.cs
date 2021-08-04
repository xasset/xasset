using System;
using System.Collections.Generic;

namespace VEngine.Editor.Builds
{
    [Serializable]
    public class Record
    {
        public string build;
        public int version;
        public long size;
        public long time;
        public List<string> files = new List<string>();
    }
}