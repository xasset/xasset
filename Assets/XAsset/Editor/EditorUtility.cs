using UnityEditor;
using UnityEngine;

namespace XAsset.Editor
{
    public class EditorUtility : Utility
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
    }
}