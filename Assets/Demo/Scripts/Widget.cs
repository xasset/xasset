using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace libx
{
    public class Widget : IEnumerator
    {
        public enum State
        {
            Loading,
            Loaded,
            Destroyed,
        }

        private AssetRequest _request;
        private System.Action<Widget> _onloaded;
        public GameObject gameObject { get; protected set; }
        public State state { get; private set; }

        public object Current
        {
            get
            {
                return null;
            }
        }

        public Action<Widget> Onloaded
        {
            get
            {
                return _onloaded;
            }

            set
            {
                _onloaded = value;
            }
        }

        private Dictionary<string, AssetRequest> assets = new Dictionary<string, AssetRequest>();

        public Widget(string assetPath)
        {
            _request = Assets.LoadAssetAsync(assetPath, typeof(GameObject));
            state = State.Loading;
            WidgetManager.Add(this);
        }

        public T GetComponent<T>(string path = null) where T : Component
        {
            if (string.IsNullOrEmpty(path))
            {
                return gameObject.GetComponent<T>();
            }
            return gameObject.transform.Find(path).GetComponent<T>();
        }

        public AssetRequest LoadAsset<T>(string assetPath) where T : UnityEngine.Object
        {
            AssetRequest request;
            if (assets.TryGetValue(assetPath, out request))
            {
                request = Assets.LoadAsset(assetPath, typeof(T));
                assets.Add(assetPath, request);
            }
            return request;
        }

        public AssetRequest LoadAssetAsync<T>(string assetPath) where T : UnityEngine.Object
        {
            AssetRequest request;
            if (assets.TryGetValue(assetPath, out request))
            {
                request = Assets.LoadAssetAsync(assetPath, typeof(T));
                assets.Add(assetPath, request);
            }
            return request;
        }

        public bool UnloadAsset(AssetRequest request)
        {
            if (request == null)
            {
                return false;
            }
            request.Release();
            return assets.Remove(request.url);
        }

        public void Destroy()
        {
            state = State.Destroyed;
        }

        public void Update()
        {
            switch (state)
            {
                case State.Loading:
                    if (_request.isDone)
                    {
                        if (_request.error != null)
                        {
                            state = State.Destroyed;
                        }
                        else
                        {
                            gameObject = GameObject.Instantiate(_request.asset as GameObject);
                            state = State.Loaded;
                            if (Onloaded != null)
                            {
                                Onloaded(this);
                            }
                        }
                    }
                    break;
                case State.Loaded:

                    break;
                case State.Destroyed:
                    if (gameObject != null)
                    {
                        GameObject.Destroy(gameObject);
                        gameObject = null;
                    }
                    foreach (var item in assets)
                    {
                        item.Value.Release();
                    }
                    _request.Release();
                    _request = null;
                    assets.Clear();
                    WidgetManager.Remove(this);
                    break;

                default:
                    break;
            }
        }

        public bool MoveNext()
        {
            return state == State.Loading;
        }

        public void Reset()
        {
        }
    }
}