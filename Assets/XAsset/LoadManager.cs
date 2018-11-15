using System;
using System.Collections.Generic;
using UnityEngine;

namespace XAsset
{
	public class AssetLoadInfo
	{
		public string path;
		public Type type;
	}

	public class LoadManager : MonoBehaviour
	{
		List<Loader> loaders = new List<Loader> ();

		Dictionary<string, List<Loader>> cache = new Dictionary<string, List<Loader>> ();

		int progress;
		bool loading = false;

		public bool Loading {
			get {
				return loading;
			}
		}

		public int Progress {
			get {
				return progress;
			}
		}

		public int Max {
			get {
				return loaders.Count;
			}
		}

		double frameTime = 1 / 30;

		public double FrameTime {
			get {
				return frameTime;
			}
			set {
				frameTime = value;
			}
		}

		const string kCurrentCacheName = "current";

		Action onCompleted;
		Action<float> onUpdateProgress;


		public void Load (AssetLoadInfo[] assets, Loader[] customLoaders, Action<float>updateProgress, Action completed)
		{
			loaders.Clear (); 

			List<string> bundles = new List<string> ();
			for (int i = 0, I = assets.Length; i < I; i++) {
				var bundleName = Assets.GetBundleName (assets [i].path);
				var allDependencies = Bundles.GetAllDependencies (bundleName);
				for (int j = 0, J = allDependencies.Length; j < J; j++) {
					var item = allDependencies [j];
					if (!bundles.Contains (item)) { 
						bundles.Add (item); 
					}
				}
			}  

			loaders.AddRange (Array.ConvertAll<string, BundleLoader> (bundles.ToArray (), input => {
				return new BundleLoader () {
					bundleName = input
				};
			}));

			loaders.AddRange (Array.ConvertAll<AssetLoadInfo, AssetLoader> (assets, delegate(AssetLoadInfo input) {
				return new AssetLoader () {
					assetPath = input.path,
					assetType = input.type
				};
			}));

			loaders.AddRange (customLoaders);

			bundles.Clear ();
			bundles = null; 
			progress = 0;

			onCompleted = completed;
			onUpdateProgress = updateProgress;

			Cache (kCurrentCacheName);
		}

		public void Cache (string name)
		{
			cache.Add (name, new List<Loader> (loaders));
		}

		public void Uncache (string name)
		{
			List<Loader> list;
			if (cache.TryGetValue (name, out list)) {
				cache.Remove (name);
				for (int i = 0, I = list.Count; i < I; i++) {
					var item = list [i];
					item.Unload ();
				} 
				list.Clear ();
			}
		}

		public void Clear ()
		{
			Uncache (kCurrentCacheName);
		}

		public bool IsDone ()
		{
			return progress >= loaders.Count;
		}

		void Complete ()
		{
			if (onCompleted != null) {
				onCompleted ();
				onCompleted = null;
			}
			loading = false;
		}

		void UpdateProgress ()
		{
			if (progress >= 0 && progress < loaders.Count) {
				loaders [progress].Load ();
				progress++; 
			}
			if (onUpdateProgress != null) {
				onUpdateProgress (Progress * 1f / Max);
			}
		}

		void Update ()
		{
			if (Loading) {
				var time = System.DateTime.Now.TimeOfDay.TotalMilliseconds;
				var elasped = 0d;
				while (elasped < frameTime) {
					if (IsDone ()) {
						Complete ();
						break;
					}	
					UpdateProgress ();
					elasped = System.DateTime.Now.TimeOfDay.TotalMilliseconds - time;
				}
			}
		}
	}
}
