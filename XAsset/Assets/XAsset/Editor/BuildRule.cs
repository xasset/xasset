using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace XAsset
{
	public abstract class BuildRule
	{
		protected static List<string> packedAssets = new List<string> ();
		protected static List<AssetBundleBuild> builds = new List<AssetBundleBuild> (); 
		static List<BuildRule> rules = new List<BuildRule> ();

		static BuildRule()
		{
			rules.Add (new BuildAssetsWithFilename("Assets/SampleAssets", "*.prefab", SearchOption.AllDirectories));
		}

		static List<string> GetFilesWithoutDirectories (string prefabPath, string searchPattern, SearchOption searchOption)
		{
			var files = System.IO.Directory.GetFiles (prefabPath, searchPattern, searchOption);
			List<string> items = new List<string> ();
			foreach (var item in files) {
				var assetPath = item.Replace ('\\', '/');
				if (!System.IO.Directory.Exists (assetPath)) {
					items.Add (assetPath);
				}
			}
			return items;
		} 
		protected static List<string> GetFilesWithoutPacked (string searchPath, string searchPattern, SearchOption searchOption)
		{
			var files = GetFilesWithoutDirectories (searchPath, searchPattern, searchOption);
			var filesCount = files.Count;
			var removeAll = files.RemoveAll ((string obj) =>  {
				return packedAssets.Contains (obj);
			});
			Debug.Log (string.Format ("RemoveAll {0} size: {1}", removeAll, filesCount));
			return files;
		}  
		protected static string BuildAssetBundleNameWithAssetPath (string assetPath)
		{ 
			return System.IO.Path.Combine (System.IO.Path.GetDirectoryName (assetPath), System.IO.Path.GetFileNameWithoutExtension (assetPath)).Replace ('\\', '/').ToLower ();
		}

		public string searchPath;
		public string searchPattern;
		public SearchOption searchOption = SearchOption.AllDirectories; 

		public BuildRule (string path, string pattern, SearchOption option)
		{
			searchPath = path;
			searchPattern = pattern;
			searchOption = option;  
		}

		public abstract void Build (); 

		public static List<AssetBundleBuild> GetBuilds(string manifestPath)
		{
			packedAssets.Clear ();
			builds.Clear ();  

			AssetBundleBuild build = new AssetBundleBuild ();
			build.assetBundleName = "manifest";
			build.assetNames = new string[] { manifestPath };
			builds.Add (build);

			foreach (var item in rules) {
				item.Build ();
			}

			EditorUtility.ClearProgressBar ();

			return builds;
		}
	}

	public class BuildAssetsWithAssetBundleName : BuildRule
	{
		string bundle;

		public BuildAssetsWithAssetBundleName (string path, string pattern, SearchOption option, string assetBundleName) : base (path, pattern, option)
		{
			bundle = assetBundleName;
		}

		public override void Build ()
		{
			var files = GetFilesWithoutPacked (searchPath, searchPattern, searchOption);
			AssetBundleBuild build = new AssetBundleBuild ();
			build.assetBundleName = bundle;
			build.assetNames = files.ToArray ();
			builds.Add (build); 
			packedAssets.AddRange (files);
		}
	}

	public class BuildAssetsWithDirectroyName : BuildRule
	{
		public BuildAssetsWithDirectroyName (string path, string pattern, SearchOption option) : base (path, pattern, option)
		{
		}

		public override void Build ()
		{
			var files = GetFilesWithoutPacked (searchPath, searchPattern, searchOption); 
			Dictionary<string, List<string>> imagePaths = new Dictionary<string, List<string>> ();
			for (int i = 0; i < files.Count; i++) {
				var item = files [i];
				if (EditorUtility.DisplayCancelableProgressBar (string.Format ("Collecting... [{0}/{1}]", i, files.Count), item, i * 1f / files.Count)) {
					break;
				}
				var path = System.IO.Path.GetDirectoryName (item);
				if (!imagePaths.ContainsKey (path)) {
					imagePaths [path] = new List<string> ();
				}
				imagePaths [path].Add (item);
			}

			int count = 0;
			foreach (var item in imagePaths) {
				AssetBundleBuild build = new AssetBundleBuild ();
				build.assetBundleName = BuildAssetBundleNameWithAssetPath (item.Key);
				build.assetNames = item.Value.ToArray ();
				builds.Add (build);
				if (EditorUtility.DisplayCancelableProgressBar (string.Format ("Packing... [{0}/{1}]", count, imagePaths.Count), build.assetBundleName, count * 1f / imagePaths.Count)) {
					break;
				}
				count++;
			}
		}
	}

	public class BuildAssetsWithFilename : BuildRule
	{
		public BuildAssetsWithFilename (string path, string pattern, SearchOption option) : base (path, pattern, option)
		{
		}

		public override void Build ()
		{
			var files = GetFilesWithoutPacked (searchPath, searchPattern, searchOption); 

			Dictionary<string, List<string>> counts = new Dictionary<string, List<string>> (); 
			for (int i = 0; i < files.Count; i++) {
				var item = files [i];
				var dependencies = AssetDatabase.GetDependencies (item);
				if (EditorUtility.DisplayCancelableProgressBar (string.Format ("Collecting... [{0}/{1}]", i, files.Count), item, i * 1f / files.Count)) {
					break;
				} 
				foreach (var assetPath in dependencies) {  
					if (!counts.ContainsKey (assetPath)) {
						counts [assetPath] = new List<string> ();
					}
					counts [assetPath].Add (item);
				}
			}

			for (int i = 0; i < files.Count; i++) {
				var item = files [i];
				if (EditorUtility.DisplayCancelableProgressBar (string.Format ("Packing... [{0}/{1}]", i, files.Count), item, i * 1f / files.Count)) {
					break;
				}
				AssetBundleBuild build = new AssetBundleBuild (); 
				build.assetBundleName = BuildAssetBundleNameWithAssetPath (item);
				var assets = AssetDatabase.GetDependencies (item);
				List<string> assetNames = new List<string> ();
				foreach (var assetPath in assets) {
					if (assetPath.EndsWith (".cs") || assetPath.EndsWith (".shader")) {
						continue;
					}
					if (packedAssets.Contains (assetPath)) {
						continue;
					} 
					if (counts [assetPath].Count == 1) {
						assetNames.Add (assetPath);
					}
				}
				build.assetNames = assetNames.ToArray ();
				packedAssets.AddRange (assetNames);
				builds.Add (build);
			}

			foreach (var item in counts) {
				var assetPath = item.Key;
				if (!assetPath.EndsWith (".cs")) {
					if (packedAssets.Contains (assetPath)) {
						continue;
					} 

					if (assetPath.EndsWith (".shader")) {
						AssetBundleBuild build = new AssetBundleBuild ();
						build.assetBundleName = "shaders";
						build.assetNames = new string[] {
							assetPath
						};
						builds.Add (build);
						packedAssets.Add (assetPath);
					} else {
						if (item.Value.Count > 1) {
							AssetBundleBuild build = new AssetBundleBuild ();
							build.assetBundleName = "public/" + BuildAssetBundleNameWithAssetPath (assetPath);
							build.assetNames = new string[] { assetPath };
							builds.Add (build);
							packedAssets.Add (assetPath); 
						}
					}
				}
			}
		}
	}
  
}