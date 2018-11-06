using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


namespace XAsset.Editor
{
    public static class AssetsMenuItem
	{
		[MenuItem ("Assets/Copy Asset Path")]
		static void CopyAssetPath ()
		{
			if (EditorApplication.isCompiling) {
				return;
			}
			string path = AssetDatabase.GetAssetPath (Selection.activeInstanceID);   
			GUIUtility.systemCopyBuffer = path;
			Debug.Log (string.Format ("systemCopyBuffer: {0}", path));
		}  

		const string kRuntimeMode = "Assets/XAsset/Bundle Mode"; 

		[MenuItem (kRuntimeMode)]
		public static void ToggleRuntimeMode ()
		{
            EditorUtility.ActiveBundleMode = !EditorUtility.ActiveBundleMode;  
		}

		[MenuItem (kRuntimeMode, true)]
		public static bool ToggleRuntimeModeValidate ()
		{
            Menu.SetChecked (kRuntimeMode, EditorUtility.ActiveBundleMode);
			return true;
		} 

		const string assetsManifesttxt = "Assets/Manifest.txt";

		[MenuItem ("Assets/XAsset/Build Manifest")]  
		public static void BuildAssetManifest ()
		{  
			if (EditorApplication.isCompiling) {
				return;
			}     
			List<AssetBundleBuild> builds = BuildRule.GetBuilds (assetsManifesttxt);
            BuildScript.BuildManifest (assetsManifesttxt, builds);
		}  

		[MenuItem ("Assets/XAsset/Build AssetBundles")]  
		public static void BuildAssetBundles ()
		{  
			if (EditorApplication.isCompiling) {
				return;
			}       
			List<AssetBundleBuild> builds = BuildRule.GetBuilds (assetsManifesttxt);
            BuildScript.BuildManifest (assetsManifesttxt, builds);
			BuildScript.BuildAssetBundles (builds);
		}  

		[MenuItem ("Assets/XAsset/Copy AssetBundles to StreamingAssets")]  
		public static void CopyAssetBundlesToStreamingAssets ()
		{  
			if (EditorApplication.isCompiling) {
				return;
			}        
			BuildScript.CopyAssetBundlesTo (Path.Combine (Application.streamingAssetsPath, EditorUtility.AssetBundlesOutputPath));

            AssetDatabase.Refresh();
		}  

		[MenuItem ("Assets/XAsset/Build Player")]  
		public static void BuildPlayer ()
		{
			if (EditorApplication.isCompiling) {
				return;
			}  
			BuildScript.BuildStandalonePlayer ();
		}
	}
}