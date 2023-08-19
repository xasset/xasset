using UnityEngine.TestTools;
using xasset.editor;

namespace xasset.tests
{
    public class EditorTest
    {
        [UnityTest]
        public void SettingsTest()
        {
            Settings.CustomPacker = CustomPacker;
            Settings.CustomFilter = CustomFilter;
            Logger.I($"{Builder.ErrorFile}:{Builder.HasError()}");
        }

        private static bool CustomFilter(string path)
        {
            return true;
        }

        private static string CustomPacker(BuildEntry asset)
        {
            return Settings.PackAsset(asset);
        }
    }
}