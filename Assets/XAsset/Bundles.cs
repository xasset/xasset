using System.Collections.Generic;
using UnityEngine;

namespace XAsset
{
	public delegate string OverrideDataPathDelegate (string bundleName);

	public static class Bundles
	{
		public static string[] activeVariants { get; private set; }

		public static string dataPath { get; private set; }

		public static event OverrideDataPathDelegate overrideBaseDownloadingURL;

		public static AssetBundleManifest manifest { get; private set; }

		public static string GetDataPath (string bundleName)
		{
			if (overrideBaseDownloadingURL != null) {
				foreach (OverrideDataPathDelegate method in overrideBaseDownloadingURL.GetInvocationList()) {
					string res = method (bundleName);
					if (res != null)
						return res;
				}
			}
			return dataPath;
		}

		public static bool Initialize (string path)
		{
			activeVariants = new string[0];
			dataPath = path;
			var request = LoadInternal (Utility.GetPlatformName (), true, false);
			if (request == null || request.error != null) {
				return false;
			}
			manifest = request.LoadAsset<AssetBundleManifest> ("AssetBundleManifest");
			if (manifest == null) {
				return false;
			}
			return true;
		}

		public static Bundle Load (string assetBundleName)
		{
			return LoadInternal (assetBundleName, false, false);
		}

		public static Bundle LoadAsync (string assetBundleName)
		{
			return LoadInternal (assetBundleName, false, true);
		}

		public static void Unload (Bundle bundle)
		{
			bundle.Release ();
		}

		static void UnloadDependencies (Bundle bundle)
		{
			foreach (var item in bundle.dependencies) {
				item.Release ();
			}
			bundle.dependencies.Clear ();
		} 

		static void LoadDependencies (Bundle bundle, string assetBundleName, bool asyncRequest)
		{
			var dependencies = manifest.GetAllDependencies (assetBundleName);
			if (dependencies.Length > 0) {
				foreach (var item in dependencies) {
					bundle.dependencies.Add (LoadInternal (item, false, asyncRequest));
				}
			}
		}

		static Bundle LoadInternal (string assetBundleName, bool isLoadingAssetBundleManifest, bool asyncRequest)
		{
			if (!isLoadingAssetBundleManifest) {
				if (manifest == null) {
					Logger.L (LogType.Error, "Bundles", "Please initialize AssetBundleManifest by calling Bundles.Initialize()");
					return null;
				}
				assetBundleName = RemapVariantName (assetBundleName);
			}

			var url = Bundles.GetDataPath (assetBundleName) + assetBundleName; 
			Bundle bundle = bundles.Find (obj => {
				return obj.name == assetBundleName;
			});  

			var hash = manifest.GetAssetBundleHash (assetBundleName);   
			if (bundle == null) {
				bundle.name = assetBundleName;

				if (url.StartsWith ("http://") ||
				    url.StartsWith ("https://") ||
				    url.StartsWith ("file://") ||
				    url.StartsWith ("ftp://")) { 
					bundle = new BundleWWW (url, hash);
				} else {
					if (asyncRequest) {
						bundle = new BundleAsync (url, hash);
					} else {
						bundle = new Bundle (url, hash);
					}
				} 
				bundles.Add (bundle);
				bundle.Load ();
				if (!isLoadingAssetBundleManifest) { 
					LoadDependencies (bundle, assetBundleName, asyncRequest);
				} 
			} 
			bundle.Retain ();
			return bundle;
		}

		static string RemapVariantName (string assetBundleName)
		{
			string[] bundlesWithVariant = manifest.GetAllAssetBundlesWithVariant ();

			// Get base bundle name
			string baseName = assetBundleName.Split ('.') [0];

			int bestFit = int.MaxValue;
			int bestFitIndex = -1;
			// Loop all the assetBundles with variant to find the best fit variant assetBundle.
			for (int i = 0; i < bundlesWithVariant.Length; i++) {
				string[] curSplit = bundlesWithVariant [i].Split ('.');
				string curBaseName = curSplit [0];
				string curVariant = curSplit [1];

				if (curBaseName != baseName)
					continue;

				int found = System.Array.IndexOf (activeVariants, curVariant);

				// If there is no active variant found. We still want to use the first
				if (found == -1)
					found = int.MaxValue - 1;

				if (found < bestFit) {
					bestFit = found;
					bestFitIndex = i;
				}
			}

			if (bestFit == int.MaxValue - 1) {
				Debug.LogWarning ("Ambigious asset bundle variant chosen because there was no matching active variant: " + bundlesWithVariant [bestFitIndex]);
			}

			if (bestFitIndex != -1) {
				return bundlesWithVariant [bestFitIndex];
			}
			return assetBundleName;
		}

		readonly internal static List<Bundle> bundles = new List<Bundle> (); 

		internal static void Update ()
		{
			for (int i = 0; i < bundles.Count; i++) {
				var bundle = bundles [i];
				if (bundle.isDone && bundle.references <= 0) {
					bundle.Unload (); 

					UnloadDependencies (bundle); 

					bundle = null;
					bundles.RemoveAt (i);
					i--;
				}
			}
		}
	}
}
