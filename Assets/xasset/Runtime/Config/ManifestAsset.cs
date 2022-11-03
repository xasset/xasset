using System;

namespace xasset
{
    [Serializable]
    public class ManifestAsset
    {
        public string name;
        public int bundle;
        public int id;
        public int dir;
        public bool auto;
        public string path { get; set; }
        public Manifest manifest { get; set; }
        public ManifestBundle mainBundle => manifest.bundles[bundle];
    }
}