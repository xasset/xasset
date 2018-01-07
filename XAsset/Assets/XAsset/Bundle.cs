using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

namespace XAsset
{    
	public class Bundle : Logger
	{ 
		public int references{ get; private set; }  
		public virtual string error { get; protected set; }
		public virtual float progress { get { return 1; } } 
		public virtual bool isDone { get { return true; } } 
		public virtual AssetBundle assetBundle { get { return _assetBundle; } }  
		public string name { get; protected set; }
		protected List<Bundle> dependencies = new List<Bundle>(); 
		AssetBundle _assetBundle = null;   

		internal Bundle (string bundleName, bool loadDependencies) { 
			I ("Load " +  bundleName);
			name = bundleName;  
			OnInit (loadDependencies);  
		}

		public T LoadAsset<T> (string assetName) where T:UnityEngine.Object
		{
			if (error != null) {
				return null;
			}
			return assetBundle.LoadAsset (assetName, typeof(T)) as T;
		}

		public UnityEngine.Object LoadAsset (string assetName, System.Type assetType)
		{
			if (error != null) {
				return null;
			}
			return assetBundle.LoadAsset (assetName, assetType);
		}

		public AssetBundleRequest LoadAssetAsync (string assetName, System.Type assetType)
		{
			if (error != null) {
				return null;
			}
			return assetBundle.LoadAssetAsync (assetName, assetType);
		} 

		public void Load()
		{
			references++;   
		}

		public void Unload()
		{  
			if (--references < 0) {
				E ("refCount < 0");
			}
			for (int i = 0, I = dependencies.Count; i < I; i++) {
				var item = dependencies [i];
				item.Unload ();
			} 
		}

		protected virtual void OnInit (bool loadDependencies)
		{ 
			_assetBundle = AssetBundle.LoadFromFile (Bundles.GetDataPath (name) + name);
			if (_assetBundle == null) {
				error = name + " LoadFromFile failed.";
			}  
			if (loadDependencies) { 
				var items = Bundles.manifest.GetAllDependencies (name); 
				if (items != null && items.Length > 0) {
					for (int i = 0, I = items.Length; i < I; i++) {
						var item = items [i];
						dependencies.Add(Bundles.Load (item));
					}
				} 
			}
		}

		protected virtual void OnDispose()
		{ 
			if (_assetBundle != null) { 
				_assetBundle.Unload (false);
				_assetBundle = null;
			}
		}

		public void Dispose ()
		{
			I ("Unload " + name); 
			OnDispose (); 
		}  
	}

	public class BundleAsync : Bundle, IEnumerator
	{ 
		#region IEnumerator implementation

		public bool MoveNext ()
		{
			return !isDone;
		}

		public void Reset ()
		{ 
		}

		public object Current {
			get {
				return assetBundle;
			}
		}

		#endregion

		public override AssetBundle assetBundle { 
			get {  
				if (error != null) {
					return null;
				}

				if (dependencies.Count == 0) { 
					return _request.assetBundle;
				} 

				for (int i = 0, I = dependencies.Count; i < I; i++) {
					var item = dependencies [i];
					if (item.assetBundle == null) {
						return null;
					}
				} 

				return _request.assetBundle;
			}
		} 

		public override float progress {
			get {  
				if (error != null) {
					return 1;
				}

				if (dependencies.Count == 0) {
					return _request.progress;  
				} 

				float value = _request.progress;
				for (int i = 0, I = dependencies.Count; i < I; i++) {
					var item = dependencies [i];
					value += item.progress;
				}  
				return value / (dependencies.Count + 1); 
			}
		}

		public override bool isDone {
			get { 
				if (error != null) {
					return true;
				}

				if (dependencies.Count == 0) {
					return _request.isDone;  
				}

				for (int i = 0, I = dependencies.Count; i < I; i++) {
					var item = dependencies [i];
					if (item.error != null) {
						error = "Falied to load Dependencies " + item;
						return true;
					} 
					if (!item.isDone) {
						return false;
					}
				}   
				return _request.isDone;
			}
		}

		AssetBundleCreateRequest _request; 

		protected override void OnInit (bool loadDependencies)
		{
			_request = AssetBundle.LoadFromFileAsync (Bundles.GetDataPath (name) + name);  
			if (_request == null) {
				error = name + " LoadFromFileAsync falied.";
			}
		}

		protected override void OnDispose ()
		{
			if (_request != null) {
				if (_request.assetBundle != null) {
					_request.assetBundle.Unload (false);
				}
				_request = null;
			}
		}

		internal BundleAsync (string assetBundleName, bool loadDependencies) : base (assetBundleName, loadDependencies)
		{ 
			 
		} 
	} 
}