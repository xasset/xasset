using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace XAsset
{
    public class Utility
    {
        public const string AssetBundlesOutputPath = "AssetBundles";

        public static void TraceTime(string name, System.Action action)
        {
            var time = System.DateTime.Now.TimeOfDay.TotalSeconds;
            if (action != null)
            {
                action();
            }
            var elasped = System.DateTime.Now.TimeOfDay.TotalSeconds - time;
            Debug.Log(string.Format(name + " elasped {0}.", elasped));
        }

        public static string GetPlatformName()
        {
#if UNITY_EDITOR
            return GetPlatformForAssetBundles(EditorUserBuildSettings.activeBuildTarget);
#else
			return GetPlatformForAssetBundles(Application.platform);
#endif
        }

        private static string GetPlatformForAssetBundles(RuntimePlatform platform)
        {
            switch (platform)
            {
                case RuntimePlatform.Android:
                    return "Android";
                case RuntimePlatform.IPhonePlayer:
                    return "iOS";
#if UNITY_TVOS
				case RuntimePlatform.tvOS:
				return "tvOS";
#endif
                case RuntimePlatform.WebGLPlayer:
                    return "WebGL";
                //                case RuntimePlatform.OSXWebPlayer:
                //                case RuntimePlatform.WindowsWebPlayer:
                //                    return "WebPlayer";
                case RuntimePlatform.WindowsPlayer:
                    return "Windows";
                case RuntimePlatform.OSXPlayer:
                    return "OSX";
                // Add more build targets for your own.
                // If you add more targets, don't forget to add the same platforms to GetPlatformForAssetBundles(RuntimePlatform) function.
                default:
                    return null;
            }
        }

#if UNITY_EDITOR
        private static string GetPlatformForAssetBundles(BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.Android:
                    return "Android";
#if UNITY_TVOS
		case BuildTarget.tvOS:
		return "tvOS";
#endif
                case BuildTarget.iOS:
                    return "iOS";
                case BuildTarget.WebGL:
                    return "WebGL";
                //                case BuildTarget.WebPlayer:
                //                    return "WebPlayer";
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "Windows";
                case BuildTarget.StandaloneOSXIntel:
                case BuildTarget.StandaloneOSXIntel64:
                case BuildTarget.StandaloneOSXUniversal:
                    return "OSX";
                // Add more build targets for your own.
                // If you add more targets, don't forget to add the same platforms to GetPlatformForAssetBundles(RuntimePlatform) function.
                default:
                    return null;
            }
        }

        public static void RemoveScriptingDefineSymbol(string name)
        {
            var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone).Split(';');
            if (System.Array.Exists(symbols, obj =>
            {
                return obj.Equals(name);
            }))
            {
                ArrayUtility.Remove(ref symbols, name);
            }
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, string.Join(";", symbols));
        }

        public static void AddScriptingDefineSymbol(string name)
        {
            var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone).Split(';');
            if (!System.Array.Exists(symbols, obj =>
            {
                return obj.Equals(name);
            }))
            {
                ArrayUtility.Add(ref symbols, name);
            }
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, string.Join(";", symbols));
        }
#endif
    }
}