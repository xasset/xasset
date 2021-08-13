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
        internal static readonly List<Scene> Unused = new List<Scene>();

        public static Action<Scene> onSceneUnloaded;
        public static Action<Scene> onSceneLoaded;
        internal readonly List<Scene> additives = new List<Scene>();
        public Action<Scene> completed;
        protected string sceneName;
        public Action<Scene> updated;

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

            var scene = Versions.CreateScene(assetPath, additive);
            if (completed != null) scene.completed += completed;
            current = scene;
            scene.Load();
            return scene;
        }

        public static Scene LoadAdditiveAsync(string assetPath, Action<Scene> completed = null)
        {
            return LoadAsync(assetPath, completed, true);
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
            operation = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
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

        public static void UpdateScenes()
        {
            if (current == null || !current.isDone) return;

            for (var index = 0; index < Unused.Count; index++)
            {
                var item = Unused[index];
                if (Updater.Instance.busy) break;

                if (!item.isDone) continue;

                Unused.RemoveAt(index);
                index--;
                item.Unload();
            }
        }
    }
}