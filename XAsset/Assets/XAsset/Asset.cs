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

        public Object asset { get; protected set; }

        internal Asset(string path, System.Type type)
        {
            assetPath = path;
            assetType = type;
            I("Load " + assetPath);
#pragma warning disable RECS0021 // Warns about calls to virtual member functions occuring in the constructor
            OnInit();
#pragma warning restore RECS0021 // Warns about calls to virtual member functions occuring in the constructor
        }

        protected virtual void OnInit()
        {
#if UNITY_EDITOR
            asset = UnityEditor.AssetDatabase.LoadAssetAtPath(assetPath, assetType);
#endif
        }

        protected virtual void OnDispose()
        {

        }

        internal void Dispose()
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
            OnDispose();
            assetPath = null;
        }

        public void Load()
        {
            references++;
        }

        public void Unload()
        {
            if (--references < 0)
            {
                Debug.LogError("refCount < 0");
            }
        }
    }

    public class BundleAsset : Asset
    {
        protected Bundle request = null;

        internal BundleAsset(string path, System.Type type) : base(path, type)
        {
        }

        protected override void OnInit()
        {
            request = Bundles.Load(Assets.GetBundleName(assetPath));
            asset = request.LoadAsset(Assets.GetAssetName(assetPath), assetType);
        }

        protected override void OnDispose()
        {
            if (request != null)
            {
                request.Unload();
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

        protected override void OnDispose()
        {
            base.OnDispose();
            abRequest = null;
            loadState = 0;
        }

        protected override void OnInit()
        {
            request = Bundles.LoadAsync(Assets.GetBundleName(assetPath));
        }

        int loadState = 0;

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
                else
                {
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
                            else
                            {
                                loadState = 1;
                            }
                        }
                    }
                    return false;
                }
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
                else
                {
                    if (loadState == 1)
                    {
                        return (abRequest.progress + request.progress) * 0.5f;
                    }
                    else
                    {
                        return abRequest.progress * 0.5f;
                    }
                }
            }
        }
    }
}
