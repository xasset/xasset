//
// Bundles.cs
//
// Author:
//       fjy <jiyuan.feng@live.com>
//
// Copyright (c) 2019 fjy
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Plugins.XAsset
{
	public delegate string OverrideDataPathDelegate (string bundleName);

	public static class Bundles
	{
		private static readonly int MAX_LOAD_SIZE_PERFREME = 3;
		// ReSharper disable once InconsistentNaming
		private static readonly List<Bundle> _bundles = new List<Bundle> ();

		// ReSharper disable once InconsistentNaming
		private static readonly List<Bundle> _unusedBundles = new List<Bundle> ();

		private static readonly List<Bundle> _ready2Load = new List<Bundle> ();

		private static readonly List<Bundle> _loading = new List<Bundle> ();

		public static string[] activeVariants { get; set; }

		private static string dataPath { get; set; }

		private static AssetBundleManifest manifest { get; set; }

		public static event OverrideDataPathDelegate OverrideBaseDownloadingUrl;

		public static string[] GetAllDependencies (string bundle)
		{
			return manifest == null ? null : manifest.GetAllDependencies (bundle);
		}

		public static void Initialize (string path, string platform, Action onSuccess, Action<string> onError)
		{
			dataPath = path;
			var request = Load (platform, true, true);
			request.completed += delegate {
				if (request.error != null)
				if (onError != null) {
					onError (request.error);
					return;
				}
				manifest = request.assetBundle.LoadAsset<AssetBundleManifest> ("AssetBundleManifest");
				request.assetBundle.Unload (false);
				request.assetBundle = null;
				request.Release ();
				request = null;
				if (onSuccess != null)
					onSuccess ();
			};
		}

		public static Bundle Load (string assetBundleName)
		{
			return Load (assetBundleName, false, false);
		}

		public static Bundle LoadAsync (string assetBundleName)
		{
			return Load (assetBundleName, false, true);
		}

		// ReSharper disable once MemberCanBePrivate.Global
		public static void Unload (Bundle bundle)
		{
			bundle.Release ();
			for (var i = 0; i < _unusedBundles.Count; i++) {
				var item = _unusedBundles [i];
				if (!item.name.Equals (bundle.name))
					continue;
				item.Unload ();
				_unusedBundles.RemoveAt (i);
				return;
			}
		}

		public static void Unload (string assetBundleName)
		{
			for (int i = 0, max = _bundles.Count; i < max; i++) {
				var item = _bundles [i];
				if (!item.name.Equals (assetBundleName))
					continue;
				Unload (item);
				break;
			}
		}

		private static void UnloadDependencies (Bundle bundle)
		{
			for (var i = 0; i < bundle.dependencies.Count; i++) {
				var item = bundle.dependencies [i];
				item.Release ();
			}

			bundle.dependencies.Clear ();
		}

		private static void LoadDependencies (Bundle bundle, string assetBundleName, bool asyncRequest)
		{
			var dependencies = manifest.GetAllDependencies (assetBundleName);
			if (dependencies.Length <= 0)
				return;
			for (var i = 0; i < dependencies.Length; i++) {
				var item = dependencies [i];
				bundle.dependencies.Add (Load (item, false, asyncRequest));
			}
		}

		[Conditional ("LOG_ENABLE")]
		private static void Log (string s)
		{
			Debug.Log ("[Bundles]" + s);
		}

		private static Bundle Load (string assetBundleName, bool isLoadingAssetBundleManifest, bool asyncMode)
		{
			if (string.IsNullOrEmpty (assetBundleName)) {
				Debug.LogError ("assetBundleName == null");
				return null;
			}

			if (!isLoadingAssetBundleManifest) {
				if (manifest == null) {
					Debug.LogError ("Please initialize AssetBundleManifest by calling Bundles.Initialize()");
					return null;
				}

				assetBundleName = RemapVariantName (assetBundleName);
			}

			var url = GetDataPath (assetBundleName) + assetBundleName;
			for (int i = 0, max = _bundles.Count; i < max; i++) {
				var item = _bundles [i];
				if (!item.name.Equals (url))
					continue;
				item.Retain ();
				return item;
			}

			Bundle bundle;
			if (url.StartsWith ("http://") ||
			    url.StartsWith ("https://") ||
			    url.StartsWith ("file://") ||
			    url.StartsWith ("ftp://"))
				bundle = new WebBundle {
					hash = manifest != null ? manifest.GetAssetBundleHash (assetBundleName):new Hash128(),
					cache = !isLoadingAssetBundleManifest
				};
			else
				bundle = asyncMode ? new BundleAsync () : new Bundle ();

			bundle.name = url;
			_bundles.Add (bundle);
			if (MAX_LOAD_SIZE_PERFREME > 0 && (bundle is BundleAsync || bundle is WebBundle)) {
				_ready2Load.Add (bundle);
			} else {
				bundle.Load ();
			}
			if (!isLoadingAssetBundleManifest)
				LoadDependencies (bundle, assetBundleName, asyncMode);
			bundle.Retain ();
			Log ("Load->" + url);
			return bundle;
		}

		private static string GetDataPath (string bundleName)
		{
			if (OverrideBaseDownloadingUrl == null)
				return dataPath;
			foreach (var @delegate in OverrideBaseDownloadingUrl.GetInvocationList()) {
				var method = (OverrideDataPathDelegate)@delegate;
				var res = method (bundleName);
				if (res != null)
					return res;
			}

			return dataPath;
		}

		internal static void Update ()
		{
			if (MAX_LOAD_SIZE_PERFREME > 0) {
				if (_ready2Load.Count > 0 && _loading.Count < MAX_LOAD_SIZE_PERFREME) {
					for (int i = 0; i < Math.Min(MAX_LOAD_SIZE_PERFREME - _loading.Count, _ready2Load.Count); i++) {
						var item = _ready2Load [i];
						if (item.loadState == LoadState.Init) {
							item.Load();
							_loading.Add(item);
							_ready2Load.RemoveAt (i);
							i--;
						}
					}
				}

				for (int i = 0; i < _loading.Count; i++) {
					var item = _loading [i];
					if (item.loadState == LoadState.Loaded || item.loadState == LoadState.Unload) {
						_loading.RemoveAt (i);
						i--;
					}
				}
			}

			for (var i = 0; i < _bundles.Count; i++) {
				var item = _bundles [i];
				if (item.Update () || !item.IsUnused ())
					continue;
				_unusedBundles.Add (item);
				UnloadDependencies (item);
				_bundles.RemoveAt (i);
				i--;
			}

			for (var i = 0; i < _unusedBundles.Count; i++) {
				var item = _unusedBundles [i];
				item.Unload ();
				Log ("Unload->" + item.name);
			}

			_unusedBundles.Clear ();
		}

		private static string RemapVariantName (string assetBundleName)
		{
			var bundlesWithVariant = manifest.GetAllAssetBundlesWithVariant ();

			// Get base bundle name
			var baseName = assetBundleName.Split ('.') [0];

			var bestFit = int.MaxValue;
			var bestFitIndex = -1;
			// Loop all the assetBundles with variant to find the best fit variant assetBundle.
			for (var i = 0; i < bundlesWithVariant.Length; i++) {
				var curSplit = bundlesWithVariant [i].Split ('.');
				var curBaseName = curSplit [0];
				var curVariant = curSplit [1];

				if (curBaseName != baseName)
					continue;

				var found = Array.IndexOf (activeVariants, curVariant);

				// If there is no active variant found. We still want to use the first
				if (found == -1)
					found = int.MaxValue - 1;

				if (found >= bestFit)
					continue;
				bestFit = found;
				bestFitIndex = i;
			}

			if (bestFit == int.MaxValue - 1)
				Debug.LogWarning (
					"Ambiguous asset bundle variant chosen because there was no matching active variant: " +
					bundlesWithVariant [bestFitIndex]);

			return bestFitIndex != -1 ? bundlesWithVariant [bestFitIndex] : assetBundleName;
		}
	}
}