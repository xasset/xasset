using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace xasset
{
    public class SceneRequest : LoadRequest
    {
        private static readonly List<SceneRequest> UnActives = new List<SceneRequest>();
        private static readonly Queue<SceneRequest> Unused = new Queue<SceneRequest>();
        private static readonly List<SceneRequest> Additives = new List<SceneRequest>();
        private bool _allowSceneActivation = true;
        private AsyncOperation _loadAsync;

        private Step _step = Step.Waiting;
        private AsyncOperation _unloadAsync;
        private ISceneHandler handler { get; set; }
        public ManifestAsset info { get; private set; }
        public bool withAdditive { get; private set; }
        public override int priority => 1;

        public bool allowSceneActivation
        {
            get => _allowSceneActivation;
            set
            {
                _allowSceneActivation = value;
                if (!_allowSceneActivation)
                    UnActives.Add(this);
                if (_loadAsync == null) return;
                _loadAsync.allowSceneActivation = value;
            }
        }

        public static Func<ISceneHandler> CreateHandler { get; set; } =
            RuntimeSceneHandler.CreateInstance;

        // ReSharper disable once MemberCanBePrivate.Global
        public static SceneRequest main { get; private set; }

        protected override void OnStart()
        {
            handler.OnStart(this);
            ReferencesCounter.Retain(path);
        }

        protected override void OnWaitForCompletion()
        {
            handler.WaitForCompletion(this);
        }

        public override void RecycleAsync()
        {
            if (!withAdditive || !Additives.Contains(this)) return;
            for (var i = SceneManager.sceneCount - 1; i > 0; i--)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.IsValid() || !scene.isLoaded || scene.path != path) continue;
                _unloadAsync = SceneManager.UnloadSceneAsync(scene);
                return;
            }
        }

        public override bool CanRecycle()
        {
            if (!isDone || refCount > 0) return false;
            if (main != null && !main.isDone) return false;
            if (!allowSceneActivation)
                allowSceneActivation = true;
            return _loadAsync == null || _loadAsync.isDone;
        }

        public override bool Recycling()
        {
            if (_unloadAsync == null) return false;
            if (_loadAsync != null && !_loadAsync.isDone) return true;
            return !_unloadAsync.isDone;
        }

        protected override void OnUpdated()
        {
            if (isDone) return;
            handler.Update(this);
            switch (_step)
            {
                case Step.Waiting:
                    UpdateWaiting();
                    break;

                case Step.Loading:
                    UpdateLoading();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UpdateLoading()
        {
            progress = _loadAsync.progress * handler.progressRate + (1 - handler.progressRate);
            if (!allowSceneActivation && _loadAsync.progress >= 0.9f)
            {
                SetResult(Result.Success);
                return;
            }

            if (!_loadAsync.isDone) return;
            SetResult(Result.Success);
        }

        private void UpdateWaiting()
        {
            if (allowSceneActivation)
            {
                foreach (var request in UnActives)
                    request.allowSceneActivation = true;
                UnActives.Clear();
            }

            if (!handler.IsReady(this)) return;
            _loadAsync = handler.LoadSceneAsync(this);
            if (_loadAsync == null)
            {
                SetResult(Result.Failed, "_loadAsync == null");
                return;
            }

            _loadAsync.allowSceneActivation = allowSceneActivation;
            _step = Step.Loading;
        }

        protected override void OnDispose()
        {
            ReferencesCounter.Release(path);
            allowSceneActivation = true;
            _loadAsync = null;
            _unloadAsync = null;
            if (withAdditive) Additives.Remove(this);
            Unused.Enqueue(this);
            _step = Step.Waiting;
            handler.Release(this);
        }

        internal static SceneRequest LoadInternal(string path, bool withAdditive)
        {
            if (!Assets.TryGetAsset(ref path, out var info)) 
            {
                Logger.E($"File not found {path}.");    
                return null;
            }

            var request = Unused.Count > 0 ? Unused.Dequeue() : new SceneRequest();
            request.Reset();
            request.info = info;
            request.path = path;
            request.withAdditive = withAdditive;
            request.handler = CreateHandler();
            request.LoadAsync();

            if (!withAdditive)
            {
                if (main != null)
                {
                    main.Release();
                    main = null;
                }

                main = request;
                foreach (var additive in Additives) additive.Release();

                Additives.Clear();
            }
            else
            {
                Additives.Add(request);
            }

            return request;
        }

        private enum Step
        {
            Waiting,
            Loading
        }
    }
}