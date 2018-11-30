using System;
using System.Collections.Generic;
using UnityEngine;

namespace XAsset
{ 
	/// <summary> 
	/// 面板抽象类 
	/// 1.FIFO 队列缓存已经创建过的面板
	/// 2.逻辑层不用关注 底层是同步还是异步加载资源  
	/// 
	/// 用法
	/// var panel = UIPanel.Create<WherecomePanel>("Assets/Prefabs/UI/WherecomePanel.prefab") 
	/// </summary> 
	public class UIPanel
	{ 
		protected void Log(string msg)
		{
			Debug.Log (string.Format ("[{0}] {1}", GetType().Name, msg));
		}
		
		public static T Create<T> (string assetPath) where T : UIPanel, new()
		{  
			var panel = UIPanelManager.Instance.GetCachedPanel<T> (assetPath);
			if (panel != null) {
				return panel;
			}
			panel = new T ();
			panel.Load (assetPath);
			return panel; 
		}

		protected List<UIPanelAction> actions = new List<UIPanelAction> ();
		protected Dictionary<string, Asset> assets = new Dictionary<string, Asset> ();
		protected GameObject gameObject;
		protected Asset asset;
		protected string key;

		public string Key {
			get {
				return key;
			}
		}

		protected virtual void OnLoaded ()
		{

		}

		protected virtual void OnUnload ()
		{
			
		}

		public void Load (string assetPath)
		{
			key = assetPath;
			asset = Assets.Load (assetPath, typeof(GameObject)); 

			UIPanelManager.Instance.Add (this);
		}

		public void Unload (bool cache = true)
		{ 
			OnUnload (); 
			UIPanelManager.Instance.Remove (this); 
			actions.Clear ();
			foreach (var item in assets) {
				item.Value.Release ();
			} 
			assets.Clear (); 
			if (cache && UIPanelManager.Instance.Cache (this)) {
				return;
			} 
			if (asset != null) {
				asset.Release ();
				asset = null; 
			} 
			if (gameObject != null) {
				GameObject.Destroy (gameObject);
				gameObject = null; 
			} 
		}

		public void SetImage (string assetPath, string child, bool nativeSize)
		{
			actions.Add (new SetImage () {
				origin = this,
				assetPath = assetPath,
				setNativeSize = nativeSize,
				target = child,  
			});
		}

		public void SetText (string text, string child)
		{
			actions.Add (new SetText () {
				origin = this,
				target = child,
				content = text,
			});
		}

		internal void Update ()
		{  
			if (gameObject == null) {
				if (asset == null) {
					return;
				}   
				if (asset.isDone) {
					gameObject = GameObject.Instantiate (asset.asset) as GameObject;
					OnLoaded ();
				} 
				return;
			}

			if (actions.Count > 0) {
				for (int i = 0; i < actions.Count; i++) {
					if (!actions [i].Update ()) {
						actions.RemoveAt (i);
						i--;
					}	
				} 
			}
		}

		internal T GetComponent<T> (string path) where T : Component
		{
			if (string.IsNullOrEmpty (path)) {
				return gameObject.GetComponent<T> ();
			}

			var node = gameObject.transform.Find (path);
			if (node != null) {
				return node.GetComponent<T> ();
			}

			return null;
		}

		internal Asset GetAsset<T> (string path) where T : UnityEngine.Object
		{
			Asset asset;
			if (!assets.TryGetValue (path, out asset)) {
				asset = Assets.Load<T> (path);
				assets.Add (path, asset);
			}
			return asset;
		}
	}
}
