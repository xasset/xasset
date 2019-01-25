using System.Collections;
using UnityEngine;

namespace XAsset
{
    public class Asset : Logger, IEnumerator
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
                return null;
            }
        }

        #endregion

        public int references { get; private set; }

        public string assetPath { get; protected set; }

        public System.Type assetType { get; protected set; }

        public virtual bool isDone { get { return true; } }

        private System.Action<Asset> completed;

        public void AddCompletedListener(System.Action<Asset> listener)
        {
            completed += listener;
        }

        public void RemoveCompletedListener(System.Action<Asset> listener)
        {
            completed -= listener;
        }

        public Object asset { get; protected set; }

        internal Asset(string path, System.Type type)
        {
            assetPath = path;
            assetType = type;
        }

        internal void Load()
        {
            I("Load " + assetPath);
            OnLoad();
        }

        internal void Unload()
        {
            I("Unload " + assetPath); 
			asset = null;
            OnUnload();
            assetPath = null;
        }

        protected virtual void OnLoad()
        {
#if UNITY_EDITOR
            asset = UnityEditor.AssetDatabase.LoadAssetAtPath(assetPath, assetType);
#endif
        }

        internal bool Update()
        {
            if (isDone)
            {
                if (completed != null)
                {
                    completed.Invoke(this);
                    completed = null;
                }
                return false;
            }
            return true;
        }

        protected virtual void OnUnload()
        {

        }

        public void Retain()
        {
            Update();
            references++;
        }

        public void Release()
        {
            if (--references < 0)
            {
                Debug.LogError("refCount < 0");
            }
        }
    }

    public class BundleAsset : Asset
    {
        protected Bundle request;

        internal BundleAsset(string path, System.Type type) : base(path, type)
        {
        }

        protected override void OnLoad()
        {
            request = Bundles.Load(Assets.GetBundleName(assetPath));
            asset = request.LoadAsset(Assets.GetAssetName(assetPath), assetType);
        }

        protected override void OnUnload()
        {
            if (request != null)
            {
                request.Release();
            }
            request = null;
        }
    }

    public class BundleAssetAsync : BundleAsset
    {
        AssetBundleRequest assetBundleRequest;

        internal BundleAssetAsync(string path, System.Type type) : base(path, type)
        {

        }

        protected override void OnLoad()
        {
            request = Bundles.LoadAsync(Assets.GetBundleName(assetPath));
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            assetBundleRequest = null;
            loadState = 0;
        }

        int loadState;

        public override bool isDone
        {
            get
            {
                if (loadState == 2)
                {
                    return true;
                }

                if (request.error != null)
                {
                    return true;
                }

                for (int i = 0; i < request.dependencies.Count; i++) // ÒÀÀµÃ»ÓÐ´íÎó
                {
                    var dep = request.dependencies[i];
                    if (dep.error != null)
                    {
                        return true;
                    }
                }

                if (loadState == 1)
                {
                    if (assetBundleRequest.isDone)
                    {
                        asset = assetBundleRequest.asset;
                        loadState = 2;
                        return true;
                    }
                }
                else
                {
                    bool allReady = true;
                    if (!request.isDone)
                    {
                        allReady = false;
                    }

                    if (request.dependencies.Count > 0)
                    {
                        if (!request.dependencies.TrueForAll(bundle => bundle.isDone))
                        {
                            allReady = false;
                        }
                    }

                    if (allReady)
                    {
                        assetBundleRequest = request.LoadAssetAsync(System.IO.Path.GetFileName(assetPath), assetType);
                        if (assetBundleRequest == null)
                        {
                            loadState = 2;
                            return true;
                        }
                        loadState = 1;
                    }
                }
                return false;
            }
        }
    }
}
