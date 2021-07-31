using System;
using System.Collections.Generic;
using System.IO;

namespace Versions
{
    public enum LoadableStatus
    {
        Wait,
        Loading,
        DependentLoading,
        SuccessToLoad,
        FailedToLoad,
        Unloaded,
        CheckVersion,
        Downloading
    }

    public class Loadable
    {
        protected internal static readonly List<Loadable> Loading = new List<Loadable>();
        protected readonly Reference reference = new Reference();
        public LoadableStatus status { get; protected set; } = LoadableStatus.Wait;
        public string pathOrURL { get; set; }
        protected bool mustCompleteOnNextFrame { get; set; }
        public string error { get; internal set; }

        public bool isDone => status == LoadableStatus.SuccessToLoad || status == LoadableStatus.Unloaded ||
                              status == LoadableStatus.FailedToLoad;

        public float progress { get; protected set; }


        protected void Finish(string errorCode = null)
        {
            error = errorCode;
            status = string.IsNullOrEmpty(errorCode) ? LoadableStatus.SuccessToLoad : LoadableStatus.FailedToLoad;
            progress = 1;
        }


        public static void UpdateAll()
        {
            for (var index = 0; index < Loading.Count; index++)
            {
                var item = Loading[index];
                if (Updater.Instance.busy)
                {
                    return;
                }

                item.Update();
                if (!item.isDone)
                {
                    continue;
                }

                Loading.RemoveAt(index);
                index--;
                item.Complete();
            }

            Asset.UpdateAssets();
            Scene.UpdateScenes();
            Bundle.UpdateBundles();
            ManifestAsset.UpdateManifestAssets();
        }

        internal static void Add(Loadable loadable)
        {
            Loading.Add(loadable);
        }

        internal void Update()
        {
            OnUpdate();
        }

        internal void Complete()
        {
            if (status == LoadableStatus.FailedToLoad)
            {
                Logger.E("Unable to load {0} {1} with error: {2}", GetType().Name, pathOrURL, error);
                Release();
            }

            OnComplete();
        }

        protected virtual void OnUpdate()
        {
        }

        protected virtual void OnLoad()
        {
        }

        protected virtual void OnUnload()
        {
        }

        protected virtual void OnComplete()
        {
        }

        public virtual void LoadImmediate()
        {
            throw new InvalidOperationException();
        }

        protected internal void Load()
        {
            reference.Retain();
            Add(this);
            if (status != LoadableStatus.Wait)
            {
                return;
            }

            Logger.I("Load {0} {1}.", GetType().Name, Path.GetFileName(pathOrURL));
            status = LoadableStatus.Loading;
            progress = 0;
            OnLoad();
        }

        protected internal void Unload()
        {
            if (status == LoadableStatus.Unloaded)
            {
                return;
            }

            Logger.I("Unload {0} {1}.", GetType().Name, Path.GetFileName(pathOrURL), error);
            OnUnload();
            status = LoadableStatus.Unloaded;
        }

        public void Release()
        {
            if (reference.count <= 0)
            {
                Logger.W("Release {0} {1}.", GetType().Name, Path.GetFileName(pathOrURL));
                return;
            }

            reference.Release();
            if (!reference.unused)
            {
                return;
            }

            OnUnused();
        }

        protected virtual void OnUnused()
        {
        }
    }
}