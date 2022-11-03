using System;
using System.Collections.Generic;

namespace xasset.editor
{
    [Serializable]
    public class BundleSettings
    {
        public bool checkReference = true;
        public bool applyBundleNameWithHash;
        public bool splitBundleNameWithBuild = true;
        public bool packByFileForAllScenes = true;
        public bool packTogetherForAllShaders = true;
        public string bundleExtension = ".bundle";

        public List<string> excludeFiles = new List<string>
            {".meta", ".dll", ".spriteatlas"};

        public List<string> shaderExtensions = new List<string>
            {".shader", ".shadervariants", ".compute"};
    }
}