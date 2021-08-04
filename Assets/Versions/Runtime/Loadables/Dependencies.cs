using System.Collections.Generic;
using UnityEngine;

namespace VEngine
{
    public class Dependencies : Loadable
    {
        protected readonly List<Bundle> bundles = new List<Bundle>();
        protected Bundle mainBundle;

        public AssetBundle assetBundle => mainBundle?.assetBundle;

        protected override void OnLoad()
        {
            if (!Versions.GetDependencies(pathOrURL, out var info, out var infos))
            {
                Finish("Dependencies not found");
                return;
            }

            if (info == null)
            {
                Finish("info == null");
                return;
            }

            mainBundle = Bundle.LoadInternal(info, mustCompleteOnNextFrame);
            bundles.Add(mainBundle);
            if (infos != null && infos.Length > 0)
                foreach (var item in infos)
                    bundles.Add(Bundle.LoadInternal(item, mustCompleteOnNextFrame));
        }

        public override void LoadImmediate()
        {
            if (isDone) return;

            foreach (var request in bundles) request.LoadImmediate();
        }

        protected override void OnUnload()
        {
            if (bundles.Count > 0)
            {
                foreach (var item in bundles)
                    if (string.IsNullOrEmpty(item.error))
                        item.Release();

                bundles.Clear();
            }

            mainBundle = null;
        }

        protected override void OnUpdate()
        {
            if (status == LoadableStatus.Loading)
            {
                var totalProgress = 0f;
                var allDone = true;
                foreach (var child in bundles)
                {
                    totalProgress += child.progress;
                    if (!string.IsNullOrEmpty(child.error))
                    {
                        status = LoadableStatus.FailedToLoad;
                        error = child.error;
                        progress = 1;
                        return;
                    }

                    if (child.isDone) continue;

                    allDone = false;
                    break;
                }

                progress = totalProgress / bundles.Count * 0.5f;
                if (allDone)
                {
                    if (assetBundle == null)
                    {
                        Finish("assetBundle == null");
                        return;
                    }

                    Finish();
                }
            }
        }
    }
}