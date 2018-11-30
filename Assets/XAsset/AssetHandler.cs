using System;
using System.Collections.Generic;
using UnityEngine;

namespace XAsset
{
	public class AssetOperation
	{
		public string assetPath;
		public Type assetType;
		public Action completed;

		Asset asset;

		internal void Load ()
		{
			asset = Assets.Load (assetPath, assetType);
		}

		internal void Unload ()
		{
			asset.Release ();
			asset = null;
		}

		internal void Complete ()
		{
			if (completed != null) {
				completed ();
				completed = null;
			}
		}
	}

	public class AssetHandler : MonoBehaviour
	{
		Dictionary<string, AssetOperation> assets = new Dictionary<string, AssetOperation> ();
		List<AssetOperation> opers = new List<AssetOperation> ();

		public static AssetHandler Get (GameObject go)
		{
			var helper = go.GetComponent<AssetHandler> ();
			if (helper == null) {
				helper = go.AddComponent<AssetHandler> ();
			}
			return helper;
		}

	 
		public void AddOperation (string assetPath, Type assetType, Action completed)
		{  
			AssetOperation oper; 
			if (assets.TryGetValue (assetPath, out oper)) {
				oper.completed = completed;
				oper.Complete (); 
				return;
			} 
			oper = new AssetOperation ();
			oper.assetPath = assetPath;
			oper.assetType = assetType;
			oper.completed = completed;
			assets.Add (assetPath, oper); 
		}

		const double frameTime = 1 / 30;

		void Update ()
		{
			if (opers.Count > 0) {
				var time = System.DateTime.Now.TimeOfDay.TotalMilliseconds;
				var elasped = 0d; 
				while (elasped < frameTime && opers.Count > 0) {  
					var oper = opers [0];
					oper.Load ();   
					oper.Complete ();
					opers.RemoveAt (0);
					elasped = System.DateTime.Now.TimeOfDay.TotalMilliseconds - time;
				}
			}
		}

		void OnDestroy ()
		{
			foreach (var item in assets) {
				item.Value.Unload ();
			}
			assets.Clear ();
		}
	}
}
