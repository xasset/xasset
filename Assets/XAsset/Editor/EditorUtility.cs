using UnityEditor;
using UnityEngine;

namespace XAsset.Editor
{
    public class EditorUtility : Utility
    { 
        [InitializeOnLoadMethod]
        static void Init()
        { 
			Debug.Log("Init->activeBundleMode: " + ActiveBundleMode);
        } 

    }
}