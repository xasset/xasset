using System;
using System.Collections.Generic;
using UnityEngine;

namespace XAsset
{
	public interface Loader
	{ 
		void Load ();

		void Unload ();
	}

	public class AssetLoader : Loader
	{
		#region Loader implementation

		public void Load ()
		{
			asset = Assets.Load (assetPath, assetType);
			if (onLoad != null) {
				onLoad (this);
			}
		}

		public void Unload ()
		{
			asset.Unload ();
			if (onUnload != null) {
				onUnload (this);
			}
		}

		#endregion

		public string assetPath;
		public System.Type assetType;
		public Asset asset;

		public Action<AssetLoader> onLoad;
		public Action<AssetLoader> onUnload;
	}

	public class BundleLoader : Loader
	{
		public string bundleName;
		public Bundle bundle;

		#region Loader implementation

		public void Load ()
		{
			bundle = Bundles.Load (bundleName); 
		}

		public void Unload ()
		{
			bundle.Unload ();
		}

		#endregion
	}

}
