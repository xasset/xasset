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
        public string name { get; protected set; }
        protected List<Bundle> dependencies = new List<Bundle>();
        AssetBundle _assetBundle;

        internal Bundle(string bundleName)
        {
            name = bundleName; 
        }

        internal void Load(bool loadDependencies)
        {
            I("Load " + name); 
            OnLoad(loadDependencies);
            if (loadDependencies)
            {
                var items = Bundles.manifest.GetAllDependencies(name);
                if (items != null && items.Length > 0)
                {
					for (int i = 0, max = items.Length; i < max; i++)
                    {
                        var item = items[i];
                        dependencies.Add(this is BundleAsync ? Bundles.LoadAsync(item) : Bundles.Load(item));
                    }
                }
            }
        }

        internal void Unload()
        {
            I("Unload " + name);
            OnUnload();
			for (int i = 0, max = dependencies.Count; i < max; i++)
            {
                var item = dependencies[i];
                item.Release();
            }
            dependencies.Clear();
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

        protected virtual void OnLoad(bool loadDependencies)
        {
            _assetBundle = AssetBundle.LoadFromFile(Bundles.GetDataPath(name) + name);
            if (_assetBundle == null)
            {
                error = name + " LoadFromFile failed.";
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

                if (dependencies.Count == 0)
                {
                    return _request.assetBundle;
                }

                for (int i = 0, I = dependencies.Count; i < I; i++)
                {
                    var item = dependencies[i];
                    if (item.assetBundle == null)
                    {
                        return null;
                    }
                }

                return _request.assetBundle;
            }
        }

        public override float progress
        {
            get
            {
                if (error != null)
                {
                    return 1;
                }

                if (dependencies.Count == 0)
                {
                    return _request.progress;
                }

                float value = _request.progress;
                for (int i = 0, I = dependencies.Count; i < I; i++)
                {
                    var item = dependencies[i];
                    value += item.progress;
                }
                return value / (dependencies.Count + 1);
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

                if (dependencies.Count == 0)
                {
                    return _request.isDone;
                }

                for (int i = 0, I = dependencies.Count; i < I; i++)
                {
                    var item = dependencies[i];
                    if (item.error != null)
                    {
                        error = "Falied to load Dependencies " + item;
                        return true;
                    }
                    if (!item.isDone)
                    {
                        return false;
                    }
                }
                return _request.isDone;
            }
        }

        AssetBundleCreateRequest _request;

        protected override void OnLoad(bool loadDependencies)
        {
            _request = AssetBundle.LoadFromFileAsync(Bundles.GetDataPath(name) + name);
            if (_request == null)
            {
                error = name + " LoadFromFileAsync falied.";
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

        internal BundleAsync(string assetBundleName) : base(assetBundleName)
        {

        }
    }
}
