using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XAsset
{
    public class Bundle : Logger
    {
        public int references { get; private set; }
        public virtual string error { get; protected set; }
        public virtual float progress { get { return 1; } }
        public virtual bool isDone { get { return true; } }
        public virtual AssetBundle assetBundle { get { return _assetBundle; } }
		public readonly List<Bundle> dependencies = new List<Bundle>();
        public string path { get; protected set; }
		public string name { get; internal set; }
		protected Hash128 version;

        AssetBundle _assetBundle; 

		internal Bundle(string url, Hash128 hash)
        {
			path = url; 
			version = hash;
        }

        internal void Load()
        {
			I("Load " + path); 
            OnLoad(); 
        }

        internal void Unload()
        {
			I("Unload " + path);
            OnUnload(); 
        }

        public T LoadAsset<T>(string assetName) where T : Object
        {
            if (error != null)
            {
                return null;
            }
            return assetBundle.LoadAsset(assetName, typeof(T)) as T;
        }

        public Object LoadAsset(string assetName, System.Type assetType)
        {
            if (error != null)
            {
                return null;
            }
            return assetBundle.LoadAsset(assetName, assetType);
        }

        public AssetBundleRequest LoadAssetAsync(string assetName, System.Type assetType)
        {
            if (error != null)
            {
                return null;
            }

            if (assetBundle == null)
            {
                return null;
            }

            return assetBundle.LoadAssetAsync(assetName, assetType);
        }

        public void Retain()
        {
            references++;
        }

        public void Release()
        {
            if (--references < 0)
            {
                E("refCount < 0");
            }
        }

        protected virtual void OnLoad()
        {
			_assetBundle = AssetBundle.LoadFromFile(path);
            if (_assetBundle == null)
            {
				error = path + " LoadFromFile failed.";
            } 
        }

        protected virtual void OnUnload()
        {
            if (_assetBundle != null)
            {
				_assetBundle.Unload(true);
                _assetBundle = null;
            }
        }  
    }

    public class BundleAsync : Bundle, IEnumerator
    {
        #region IEnumerator implementation

        public bool MoveNext()
        {
            return !isDone;
        }

        public void Reset()
        {
        }

        public object Current
        {
            get
            {
                return assetBundle;
            }
        }

        #endregion

        public override AssetBundle assetBundle
        {
            get
            {
                if (error != null)
                {
                    return null;
                } 

                return _request.assetBundle;
            }
        } 

        public override bool isDone
        {
            get
            {
                if (error != null)
                {
                    return true;
                } 
                return _request.isDone;
            }
        }

        AssetBundleCreateRequest _request;

        protected override void OnLoad()
        {
			_request = AssetBundle.LoadFromFileAsync(path);
            if (_request == null)
            {
				error = path + " LoadFromFileAsync falied.";
            }
        }

        protected override void OnUnload()
        {
            if (_request != null)
            {
                if (_request.assetBundle != null)
                {
					_request.assetBundle.Unload(true); 
                }
                _request = null;
            }
        }

		internal BundleAsync(string url, Hash128 hash) : base(url, hash)
        {

        }
    } 


	public class BundleWWW : Bundle, IEnumerator
	{
		#region IEnumerator implementation

		public bool MoveNext()
		{
			return !isDone;
		}

		public void Reset()
		{
		}

		public object Current
		{
			get
			{
                if (!_request.isDone)
                {
                    return null;
                }
				return assetBundle;
			}
		}

		#endregion

		public override AssetBundle assetBundle
		{
			get
			{
				if (error != null)
				{
					return null;
				} 
				return _request.assetBundle;
			}
		} 

		public override bool isDone
		{
			get
			{
				if (error != null)
				{
					return true;
				}

                if (_request.error != null)
                {
                    return true;
                }

                if (_request.isDone && _request.assetBundle !=null)
                {
                    return true;
                }

                return false;
			}
		}

		WWW _request;

		protected override void OnLoad()
		{
			_request = WWW.LoadFromCacheOrDownload(path, version);
			if (_request == null)
			{
				error = path + " LoadFromFileAsync falied.";
			}
		}

		protected override void OnUnload()
		{
			if (_request != null)
			{
				if (_request.assetBundle != null)
				{
					_request.assetBundle.Unload(true);
				}
				_request = null;
			}
		}

		internal BundleWWW(string url, Hash128 hash) : base(url, hash)
		{

		}
	} 
}
