using UnityEditor;
using UnityEngine;

namespace XAsset.Editor
{
    public static class EditorUtility
    {
        static int activeBundleMode = -1;

        const string kActiveBundleMode = "ActiveBundleMode";

        [InitializeOnLoadMethod]
        static void Init()
        { 
			Debug.Log("Init->activeBundleMode: " + ActiveBundleMode);
        }

        public static bool ActiveBundleMode
        {
            get
            {
                if (activeBundleMode == -1)
                    activeBundleMode = EditorPrefs.GetBool(kActiveBundleMode, true) ? 1 : 0;
                Utility.ActiveBundleMode = activeBundleMode != 0;
                return Utility.ActiveBundleMode;
            }
            set
            {
                int newValue = value ? 1 : 0;
                if (newValue != activeBundleMode)
                {
                    activeBundleMode = newValue;
                    EditorPrefs.SetBool(kActiveBundleMode, value);
                }
            }
        } 

        public const string AssetBundlesOutputPath = Utility.AssetBundlesOutputPath;

        public static string GetPlatformName()
        {
            return GetPlatformForAssetBundles(EditorUserBuildSettings.activeBuildTarget);
        }

        static string GetPlatformForAssetBundles(BuildTarget target)
        {
            if (target == BuildTarget.Android)
            {
                return "Android";
            }
            if (target == BuildTarget.tvOS)
            {
                return "tvOS";
            }
            if (target == BuildTarget.iOS)
            {
                return "iOS";
            }
            if (target == BuildTarget.WebGL)
            {
                return "WebGL";
            }
            if (target == BuildTarget.StandaloneWindows || target == BuildTarget.StandaloneWindows64)
            {
                return "Windows";
            }
			if (target == BuildTarget.StandaloneOSXUniversal)
            {
                return "OSX";
            }
            // Add more build targets for your own.
            // If you add more targets, don't forget to add the same platforms to GetPlatformForAssetBundles(RuntimePlatform) function.
            return null;
        }
    }
}