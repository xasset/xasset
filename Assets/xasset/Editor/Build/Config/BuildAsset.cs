using System;

namespace xasset.editor
{
    [Serializable]
    public class BuildAsset
    {
        public string path;
        public string bundle;
        public string type;
        public string entry;
        public long lastWriteTime;
        public AddressMode addressMode = AddressMode.LoadByPath;
        public string[] dependencies = Array.Empty<string>();
        public BuildGroup group;
    }
}