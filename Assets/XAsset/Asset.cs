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

        public virtual float progress { get { return 1; } }

        public System.Action<Asset> completed { get; set; }

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
            if (asset != null)
            {
                if (asset.GetType() != typeof(GameObject))
                {
                    Resources.UnloadAsset(asset);
                }
                asset = null;
            }
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
        AssetBundleRequest abRequest;

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
            abRequest = null;
            loadState = 0;
        }

        int loadState;

        public override bool isDone
        {
            get
            {
                if (request.error != null)
                {
                    return true;
                }

                if (loadState == 2)
                {
                    return true;
                }

                if (loadState == 1)
                {
                    if (abRequest.isDone)
                    {
                        asset = abRequest.asset;
                        loadState = 2;
                        return true;
                    }
                }
                else
                {
                    if (request.isDone)
                    {
                        abRequest = request.LoadAssetAsync(System.IO.Path.GetFileName(assetPath), assetType);
                        if (abRequest == null)
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

        public override float progress
        {
            get
            {
                if (request.error != null)
                {
                    return 1;
                }

                if (loadState == 2)
                {
                    return 1;
                }

                if (loadState == 1)
                {
                    return (abRequest.progress + request.progress) * 0.5f;
                }

                return abRequest.progress * 0.5f;
            }
        }
    }
}
