using UnityEngine;

namespace XAsset
{
    public static class Utility
    {
        public const string AssetBundlesOutputPath = "AssetBundles";
        public static bool ActiveBundleMode { get; set; }

        public static string GetPlatformName()
        {
            return GetPlatformForAssetBundles(Application.platform);
        }

        public static string GetPlatformForAssetBundles(RuntimePlatform platform)
        {
            if (platform == RuntimePlatform.Android)
            {
                return "Android";
            }
            if (platform == RuntimePlatform.IPhonePlayer)
            {
                return "iOS";
            }
            if (platform == RuntimePlatform.tvOS)
            {
                return "tvOS";
            }
            if (platform == RuntimePlatform.WebGLPlayer)
            {
                return "WebGL";
            }
            if (platform == RuntimePlatform.WindowsPlayer || platform == RuntimePlatform.WindowsEditor)
            {
                return "Windows";
            }
            if (platform == RuntimePlatform.OSXPlayer)
            {
                return "OSX";
            }
            return null;
        }
    }
}