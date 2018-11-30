using System;
using System.Collections.Generic;
using UnityEngine;

namespace XAsset
{
	public class UIPanelManager : MonoBehaviour
	{
		static UIPanelManager instance;

		public static UIPanelManager Instance {
			get {
				if (instance == null) { 
					instance = GameObject.FindObjectOfType<UIPanelManager> ();
					if (instance == null) { 
						instance = new GameObject ("UIPanelManager").AddComponent<UIPanelManager> (); 
					}
				}
				return instance;
			}
		}

		const int maxCacheSize = 3;
		List<UIPanel> panels = new List<UIPanel> ();
		List<UIPanel> cache = new List<UIPanel> (maxCacheSize * 2);

		public void Add (UIPanel panel)
		{
			panels.Add (panel);
		}

		public void Remove (UIPanel panel)
		{
			panels.Remove (panel);
		}

		public bool Cache (UIPanel panel)
		{
			if (cache.Count < maxCacheSize) {
				cache.Add (panel);
				var size = cache.Count;
				if (size > maxCacheSize) { 
					var unloadIndex = size - 1;
					cache [unloadIndex].Unload (false);
					cache.RemoveAt (unloadIndex);
				}
				return true;
			}  
			return false;
		}

		public void ClearCache ()
		{
			foreach (var item in cache) {
				item.Unload (false);
			}
			cache.Clear ();
		}

		public T GetCachedPanel<T> (string assetPath) where T:UIPanel
		{
			foreach (var item in cache) {
				if (item.Key.Equals (assetPath) && item.GetType () == typeof(T)) {
					return item as T;
				}
			}   
			return null;
		}

		void Awake ()
		{
			DontDestroyOnLoad (gameObject);
		}

		void Update ()
		{
			for (int i = 0, max = panels.Count; i < max; i++) {
				var item = panels [i];
				item.Update ();
			}
		}

		void OnDestroy ()
		{
			foreach (var item in panels) {
				item.Unload (false);
			}
			panels.Clear (); 
			ClearCache ();
		}
	}
}
