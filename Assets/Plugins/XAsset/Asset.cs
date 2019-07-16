//
// Asset.cs
//
// Author:
//       fjy <jiyuan.feng@live.com>
//
// Copyright (c) 2019 fjy
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Plugins.XAsset
{
    public enum LoadState
    {
        Init,
        LoadAssetBundle,
        LoadAsset,
        Loaded,
        Unload,
    }

    public class Asset : Reference, IEnumerator
    {
        private List<Object> _requires;
        public Type assetType;
        public string name;
        public LoadState loadState { get; protected set; }

        public Asset()
        {
            asset = null;
            loadState = LoadState.Init;
        }

        public virtual bool isDone
        {
            get { return true; }
        }

        public virtual float progress
        {
            get { return 1; }
        }

        public virtual string error { get; protected set; }

        // ReSharper disable once MemberCanBeProtected.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string text { get; protected set; }

        // ReSharper disable once MemberCanBeProtected.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public byte[] bytes { get; protected set; }

        public Object asset { get; internal set; }

        private bool checkRequires
        {
            get { return _requires != null; }
        }

        public void Require(Object obj)
        {
            if (_requires == null)
                _requires = new List<Object>();

            _requires.Add(obj);
            Retain();
        }

        // ReSharper disable once IdentifierTypo
        public void Dequire(Object obj)
        {
            if (_requires == null)
                return;

            if (_requires.Remove(obj))
                Release();
        }

        private void UpdateRequires()
        {
            for (var i = 0; i < _requires.Count; i++)
            {
                var item = _requires[i];
                if (item != null)
                    continue;
                Release();
                _requires.RemoveAt(i);
                i--;
            }

            if (_requires.Count == 0)
                _requires = null;
        }

        internal virtual void Load()
        {
            _Load();
        }

        [Conditional("UNITY_EDITOR")]
        private void _Load()
        {
            if (Utility.loadDelegate != null)
                asset = Utility.loadDelegate(name, assetType);
        }

        [Conditional("UNITY_EDITOR")]
        private void _Unload()
        {
            if (asset == null)
                return;
            if (!(asset is GameObject))
                Resources.UnloadAsset(asset);

            asset = null;
        }

        internal virtual void Unload()
        {
            _Unload();
        }

        internal bool Update()
        {
            if (checkRequires)
                UpdateRequires();
            if (!isDone)
                return true;
            if (completed == null)
                return false;
            try
            {
                completed.Invoke(this);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            completed = null;
            return false;
        }

        // ReSharper disable once InconsistentNaming
        public event Action<Asset> completed;

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
            get { return null; }
        }

        #endregion
    }

    public class BundleAsset : Asset
    {
        protected readonly string assetBundleName;
        protected Bundle bundle;

        public BundleAsset(string bundle)
        {
            assetBundleName = bundle;
        }

        internal override void Load()
        {
            bundle = Bundles.Load(assetBundleName);
            var assetName = Path.GetFileName(name);
            asset = bundle.assetBundle.LoadAsset(assetName, assetType);
        }

        internal override void Unload()
        {
            if (bundle != null)
            {
                bundle.Release();
                bundle = null;
            }

            asset = null;
        }
    }

    public class BundleAssetAsync : BundleAsset
    {
        private AssetBundleRequest _request;

        public BundleAssetAsync(string bundle)
            : base(bundle)
        {
        }

        public override bool isDone
        {
            get
            {
                if (error != null || bundle.error != null)
                    return true;

                for (int i = 0, max = bundle.dependencies.Count; i < max; i++)
                {
                    var item = bundle.dependencies[i];
                    if (item.error != null)
                        return true;
                }

                switch (loadState)
                {
                    case LoadState.Init:
                        return false;
                    case LoadState.Loaded:
                        return true;
                    case LoadState.LoadAssetBundle:
                        {
                            if (!bundle.isDone)
                                return false;

                            for (int i = 0, max = bundle.dependencies.Count; i < max; i++)
                            {
                                var item = bundle.dependencies[i];
                                if (!item.isDone)
                                    return false;
                            }

                            if (bundle.assetBundle == null)
                            {
                                error = "assetBundle == null";
                                return true;
                            }

                            var assetName = Path.GetFileName(name);
                            _request = bundle.assetBundle.LoadAssetAsync(assetName, assetType);
                            loadState = LoadState.LoadAsset;
                            break;
                        }
                    case LoadState.Unload:
                        break;
                    case LoadState.LoadAsset:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (loadState != LoadState.LoadAsset)
                    return false;
                if (!_request.isDone)
                    return false;
                asset = _request.asset;
                loadState = LoadState.Loaded;
                return true;
            }
        }

        public override float progress
        {
            get
            {
                var bundleProgress = bundle.progress;
                if (bundle.dependencies.Count <= 0)
                    return bundleProgress * 0.3f + (_request != null ? _request.progress * 0.7f : 0);
                for (int i = 0, max = bundle.dependencies.Count; i < max; i++)
                {
                    var item = bundle.dependencies[i];
                    bundleProgress += item.progress;
                }

                return bundleProgress / (bundle.dependencies.Count + 1) * 0.3f +
                       (_request != null ? _request.progress * 0.7f : 0);
            }
        }

        internal override void Load()
        {
            bundle = Bundles.LoadAsync(assetBundleName);
            loadState = LoadState.LoadAssetBundle;
        }

        internal override void Unload()
        {
            _request = null;
            loadState = LoadState.Unload;
            base.Unload();
        }
    }

    public class SceneAsset : Asset
    {
        protected readonly LoadSceneMode loadSceneMode;
        protected readonly string sceneName;
        public string assetBundleName;
        protected Bundle bundle;

        public SceneAsset(string path, bool addictive)
        {
            name = path;
            sceneName = Path.GetFileNameWithoutExtension(name);
            loadSceneMode = addictive ? LoadSceneMode.Additive : LoadSceneMode.Single;
        }

        public override float progress
        {
            get { return 1; }
        }

        internal override void Load()
        {
            if (!string.IsNullOrEmpty(assetBundleName))
            {
                bundle = Bundles.Load(assetBundleName);
                if (bundle != null)
                    SceneManager.LoadScene(sceneName, loadSceneMode);
            }
            else
            {
                SceneManager.LoadScene(sceneName, loadSceneMode);
            }
        }

        internal override void Unload()
        {
            if (bundle != null)
                bundle.Release();

            if (SceneManager.GetSceneByName(sceneName).isLoaded)
                SceneManager.UnloadSceneAsync(sceneName);

            bundle = null;
        }
    }

    public class SceneAssetAsync : SceneAsset
    {
        private AsyncOperation _request;

        public SceneAssetAsync(string path, bool addictive)
            : base(path, addictive)
        {
        }

        public override float progress
        {
            get
            {
                if (bundle == null)
                    return _request == null ? 0 : _request.progress;

                var bundleProgress = bundle.progress;
                if (bundle.dependencies.Count <= 0)
                    return bundleProgress * 0.3f + (_request != null ? _request.progress * 0.7f : 0);
                for (int i = 0, max = bundle.dependencies.Count; i < max; i++)
                {
                    var item = bundle.dependencies[i];
                    bundleProgress += item.progress;
                }

                return bundleProgress / (bundle.dependencies.Count + 1) * 0.3f +
                       (_request != null ? _request.progress * 0.7f : 0);
            }
        }

        public override bool isDone
        {
            get
            {
                switch (loadState)
                {
                    case LoadState.Loaded:
                        return true;
                    case LoadState.LoadAssetBundle:
                        {
                            if (bundle == null || bundle.error != null)
                                return true;

                            for (int i = 0, max = bundle.dependencies.Count; i < max; i++)
                            {
                                var item = bundle.dependencies[i];
                                if (item.error != null)
                                    return true;
                            }
                            
                            if (!bundle.isDone)
                                return false;

                            for (int i = 0, max = bundle.dependencies.Count; i < max; i++)
                            {
                                var item = bundle.dependencies[i];
                                if (!item.isDone)
                                    return false;
                            }

                            _request = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
                            loadState = LoadState.LoadAsset;
                            break;
                        }
                    case LoadState.Unload:
                        break;
                    case LoadState.LoadAsset:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (loadState != LoadState.LoadAsset)
                    return false;
                if (!_request.isDone)
                    return false;
                loadState = LoadState.Loaded;
                return true;
            }
        }

        internal override void Load()
        {
            if (!string.IsNullOrEmpty(assetBundleName))
            {
                bundle = Bundles.LoadAsync(assetBundleName);
                loadState = LoadState.LoadAssetBundle;
            }
            else
            {
                _request = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
                loadState = LoadState.LoadAsset;
            }
        }

        internal override void Unload()
        {
            base.Unload();
            _request = null;
        }
    }

    public class WebAsset : Asset
    {
#if UNITY_2018_3_OR_NEWER
        private UnityWebRequest _www;
#else
        private WWW _www;
#endif

        public override bool isDone
        {
            get
            {
                if (loadState == LoadState.Init)
                    return false;
                if (_www == null)
                    return true;

                if (!_www.isDone)
                    return _www.isDone;
                if (asset != null)
                    return _www.isDone;

#if UNITY_2018_3_OR_NEWER
                if (assetType != typeof(Texture2D))
                {
                    if (assetType != typeof(TextAsset))
                    {
                        if (assetType != typeof(AudioClip))
                            bytes = _www.downloadHandler.data;
                        else
                            asset = DownloadHandlerAudioClip.GetContent(_www);
                    }
                    else
                    {
                        text = _www.downloadHandler.text;
                    }
                }
                else
                {
                    asset = DownloadHandlerTexture.GetContent(_www);
                }
#else
                if (assetType != typeof(Texture2D))
                {
                    if (assetType != typeof(TextAsset))
                    {
                        if (assetType != typeof(AudioClip))
                            bytes = _www.bytes;
                        else
                            asset = _www.GetAudioClip();
                    }
                    else
                    {
                        text = _www.text;
                    }
                }
                else
                {
                    asset = _www.texture;
                }

                
#endif
                return _www.isDone;
            }
        }

        public override string error
        {
            get { return _www.error; }
        }

        public override float progress
        {
#if UNITY_2018_3_OR_NEWER
            get { return _www.downloadProgress; }
#else
            get { return _www.progress;}
#endif
        }

        internal override void Load()
        {
#if UNITY_2018_3_OR_NEWER
            if (assetType == typeof(AudioClip))
            {
                _www = UnityWebRequestMultimedia.GetAudioClip(name, AudioType.WAV);
            }
            else if (assetType == typeof(Texture2D))
            { 
                _www = UnityWebRequestTexture.GetTexture(name);
            } 
            else
            {
                _www = new UnityWebRequest(name);
                _www.downloadHandler = new DownloadHandlerBuffer();
            }
            _www.SendWebRequest();
#else
            _www = new WWW(name);
#endif
            loadState = LoadState.LoadAsset;
        }

        internal override void Unload()
        {
            if (asset != null)
            {
                Object.Destroy(asset);
                asset = null;
            }
            if (_www != null)
                _www.Dispose();
        }
    }
}