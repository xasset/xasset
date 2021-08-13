using System;
using System.Collections.Generic;
using UnityEngine;

namespace VEngine
{
    public class Bundle : Loadable
    {
        public static Func<ManifestBundle, Bundle> customBundleCreator;

        public static readonly Dictionary<string, Bundle> Cache = new Dictionary<string, Bundle>();

        public static readonly List<Bundle> Unused = new List<Bundle>();

        public ManifestBundle info;

        public AssetBundle assetBundle { get; protected set; }

        protected void OnLoaded(AssetBundle bundle)
        {
            assetBundle = bundle;
            Finish(assetBundle == null ? "assetBundle == null" : null);
        }

        protected override void OnUnused()
        {
            Unused.Add(this);
        }

        internal static Bundle LoadInternal(ManifestBundle bundle, bool mustCompleteOnNextFrame)
        {
            if (bundle == null) throw new NullReferenceException();

            if (!Cache.TryGetValue(bundle.nameWithAppendHash, out var item))
            {
                var url = Versions.GetBundlePathOrURL(bundle);
                if (Application.platform == RuntimePlatform.WebGLPlayer)
                {
                    throw new NotImplementedException("开源版不提供 WebGL 支持");
                }
                else
                {
                    if (customBundleCreator != null) item = customBundleCreator(bundle);
                    if (item == null)
                    {
                        if (url.StartsWith("http://") || url.StartsWith("https://") || url.StartsWith("ftp://"))
                            item = new DownloadBundle {pathOrURL = url, info = bundle};
                        else
                            item = new LocalBundle {pathOrURL = url, info = bundle};
                    }
                }

                Cache.Add(bundle.nameWithAppendHash, item);
            }

            item.mustCompleteOnNextFrame = mustCompleteOnNextFrame;
            item.Load();
            if (mustCompleteOnNextFrame) item.LoadImmediate();

            return item;
        }

        internal static void UpdateBundles()
        {
            for (var index = 0; index < Unused.Count; index++)
            {
                var item = Unused[index];
                if (!item.isDone) continue;

                Unused.RemoveAt(index);
                index--;
                if (!item.reference.unused) continue;

                item.Unload();
                Cache.Remove(item.info.nameWithAppendHash);
                if (Updater.Instance.busy) return;
            }
        }

        protected override void OnUnload()
        {
            if (assetBundle == null) return;

            assetBundle.Unload(true);
            assetBundle = null;
        }
    }
}