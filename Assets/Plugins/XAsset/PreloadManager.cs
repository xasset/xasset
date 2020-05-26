//
// LoadManager.cs
//
// Author:
//       fjy <jiyuan.feng@live.com>
//
// Copyright (c) 2020 fjy
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
using System.Collections.Generic;
using UnityEngine;

namespace libx
{
    public interface ILoader
    {
        void Load();
        void Unload();
        bool IsDone();
        float Progress();
    }

    public class AssetLoader : ILoader
    {
        AssetRequest request;

        string url;
        Type type;

        public AssetLoader(string assetPath, Type assetType)
        {
            url = assetPath;
            type = assetType;
        }

        public bool IsDone()
        {
            if (request == null)
            {
                return false;
            }
            return request.isDone;
        }

        public void Load()
        {
            request = Assets.LoadAssetAsync(url, type);
        }

        public float Progress()
        {
            if (request == null)
            {
                return 0;
            }
            return request.progress;
        }

        public void Unload()
        {
            request.Release();
            request = null;
        }

        public UnityEngine.Object GetAsset() { return request.asset; }
    }

    public class SceneLoader : ILoader
    {
        SceneAssetRequest request;
        string url;
        bool isAdditive;

        public SceneLoader(string assetPath, bool additive)
        {
            url = assetPath;
            isAdditive = additive;
        }

        public bool IsDone()
        {
            return request.isDone;
        }

        public void Load()
        {
            request = Assets.LoadSceneAsync(url, isAdditive);
        }

        public float Progress()
        {
            return request.progress;
        }

        public void Unload()
        {
            request.Unload();
            request = null;
        }
    }

    public class GameObjectLoader : ILoader
    {
        public string url;
        AssetRequest request;
        public GameObject gameObject { get; private set; }
        public GameObjectLoader(string path)
        {
            url = path;
        }

        public bool IsDone()
        {
            return request.isDone;
        }

        public void Load()
        {
            request = Assets.LoadAssetAsync(url, typeof(GameObject));
            request.completed += OnAssetLoaded;
        }

        private void OnAssetLoaded(AssetRequest obj)
        {
            if(! string.IsNullOrEmpty(obj.error))
            {
                Debug.LogError(obj.error);
                return;
            }
            gameObject = GameObject.Instantiate((GameObject)obj.asset);
        }

        public float Progress()
        {
            return request.progress;
        }

        public void Unload()
        {
            if(gameObject != null)
            {
                GameObject.Destroy(gameObject);
                gameObject = null;
            }
            request.Release();
            request = null;
        }
    }

    public class PreloadManager : MonoBehaviour
    {
        List<ILoader> loaders = new List<ILoader>();
        private bool loading = false;

        Dictionary<string, AssetLoader> assets = new Dictionary<string, AssetLoader>();
        Dictionary<string, GameObjectLoader> gameObjects = new Dictionary<string, GameObjectLoader>();

        private void AddLoader(ILoader loader)
        {
            loaders.Add(loader);
        }

        public AssetLoader AddAsset(string path, Type type)
        {
            var loader = new AssetLoader(path, type);
            AddLoader(loader);
            assets.Add(path, loader);
            return loader;
        }

        public SceneLoader AddScene(string path, bool additive)
        {
            var loader = new SceneLoader(path, additive);
            AddLoader(loader);
            return loader;
        }

        public GameObjectLoader AddGameObject(string path)
        {
            var loader = new GameObjectLoader(path);
            AddLoader(loader);
            gameObjects.Add(path, loader);
            return loader;
        }

        public GameObject GetGameObject(string path)
        {
            GameObjectLoader loader;
            if(gameObjects.TryGetValue(path, out loader))
            {
                return loader.gameObject;
            }
            return null;
        }

        public T GetAsset<T>(string path) where T : UnityEngine.Object
        {
            AssetLoader loader;
            if(assets.TryGetValue(path, out loader))
            {
                return (T)loader.GetAsset();
            }
            return null;
        }

        public void StartLoad()
        {
            loaders.ForEach((o) => o.Load());
            Loading = true;
        }

        public void Clear()
        {
            loaders.ForEach((o) => o.Unload());
            loaders.Clear();
        }

        readonly Predicate<ILoader> matchIsDone = delegate (ILoader o) { return o.IsDone(); };

        public bool Loading { get { return loading; } private set { loading = value; } } 

        public bool IsDone()
        {
            return loaders.TrueForAll(matchIsDone);
        }

        public float Progress()
        {
            float progress = 0;

            Action<ILoader> action = delegate (ILoader o) { progress += o.Progress(); };

            loaders.ForEach(action);

            return progress / loaders.Count;
        }

        private void Update()
        {
            if (Loading)
            {
                if (IsDone())
                {
                    Loading = false;
                }
            }
        }
    }
}