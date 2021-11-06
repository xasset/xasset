using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VEngine
{
    public class Scene : Loadable, IEnumerator
    {
        public static Func<string, bool, Scene> Creator { get; set; } = BundledScene.Create;

        private static Scene CreateInstance(string path, bool additive)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException(nameof(path));
            }

            return Creator(path, additive);
        }

        public static Action<Scene> onSceneUnloaded;
        public static Action<Scene> onSceneLoaded;
        public Action<Scene> completed;
        public Action<Scene> updated;
        public readonly List<Scene> additives = new List<Scene>();
        protected string sceneName;
        public AsyncOperation operation { get; protected set; }
        public static Scene main { get; private set; }
        public static Scene current { get; private set; }
        protected Scene parent { get; set; }
        protected internal LoadSceneMode loadSceneMode { get; set; }

        public bool MoveNext()
        {
            return !isDone;
        }

        public void Reset()
        {
        }

        public object Current => null;

        public static Scene LoadAsync(string assetPath, Action<Scene> completed = null, bool additive = false)
        {
            if (string.IsNullOrEmpty(assetPath)) throw new ArgumentNullException(nameof(assetPath));

            var scene = CreateInstance(assetPath, additive);
            if (completed != null) scene.completed += completed;
            current = scene;
            scene.Load();
            return scene;
        }

        public static Scene LoadAdditiveAsync(string assetPath, Action<Scene> completed = null)
        {
            return LoadAsync(assetPath, completed, true);
        }

        public static Scene Load(string assetPath, bool additive = false)
        {
            var scene = CreateInstance(assetPath, additive);
            current = scene;
            scene.mustCompleteOnNextFrame = true;
            scene.Load();
            return scene;
        }

        protected override void OnUpdate()
        {
            if (status == LoadableStatus.Loading)
            {
                UpdateLoading();
                if (updated != null) updated(this);
            }
        }

        protected void UpdateLoading()
        {
            if (operation == null)
            {
                Finish("operation == null");
                return;
            }

            progress = 0.5f + operation.progress * 0.5f;

            if (operation.allowSceneActivation)
            {
                if (!operation.isDone) return;
            }
            else
            {
                // https://docs.unity3d.com/ScriptReference/AsyncOperation-allowSceneActivation.html
                if (operation.progress < 0.9f) return;
            }

            Finish();
        }

        protected override void OnLoad()
        {
            PrepareToLoad();
            if (mustCompleteOnNextFrame)
            {
                SceneManager.LoadScene(sceneName, loadSceneMode);
                Finish();
            }
            else
            {
                operation = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
            }
        }

        protected void PrepareToLoad()
        {
            sceneName = Path.GetFileNameWithoutExtension(pathOrURL);
            if (loadSceneMode == LoadSceneMode.Single)
            {
                if (main != null)
                {
                    main.Release();
                    main = null;
                }

                main = this;
            }
            else
            {
                if (main != null)
                {
                    main.additives.Add(this);
                    parent = main;
                }
            }
        }

        protected override void OnUnused()
        {
            completed = null;
            Unused.Add(this);
        }

        protected override void OnUnload()
        {
            if (loadSceneMode == LoadSceneMode.Additive)
            {
                if (main != null) main.additives.Remove(this);
                if (parent != null && string.IsNullOrEmpty(error))
                    SceneManager.UnloadSceneAsync(sceneName);
                parent = null;
            }
            else
            {
                foreach (var item in additives)
                {
                    item.Release();
                    item.parent = null;
                }

                additives.Clear();
            }

            if (onSceneUnloaded != null) onSceneUnloaded.Invoke(this);
        }

        protected override void OnComplete()
        {
            if (onSceneLoaded != null) onSceneLoaded.Invoke(this);

            if (completed == null) return;

            var saved = completed;
            if (completed != null) completed(this);

            completed -= saved;
        }
    }
}