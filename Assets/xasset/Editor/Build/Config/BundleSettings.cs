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
        public string extension = ".bundle";

        public List<string> shaderExtensions = new List<string>
            {".shader", ".shadervariants", ".compute"};

        public List<string> excludeFiles = new List<string>
        {
            ".cs",
            ".cginc",
            ".hlsl",
            ".spriteatlas",
            ".dll",
        };
    }
}