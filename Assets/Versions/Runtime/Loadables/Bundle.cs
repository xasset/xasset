using System;
using System.Collections.Generic;
using UnityEngine;

namespace Versions
{
    public class Bundle : Loadable
    {
        public static readonly Dictionary<string, Bundle> Cache = new Dictionary<string, Bundle>();

        public static readonly List<Bundle> Unused = new List<Bundle>();

        protected ManifestBundle info;
        public AssetBundle assetBundle { get; protected set; }

        protected override void OnUnused()
        {
            Unused.Add(this);
        }

        internal static Bundle LoadInternal(ManifestBundle info, bool mustCompleteOnNextFrame)
        {
            if (info == null) throw new NullReferenceException();

            if (!Cache.TryGetValue(info.nameWithAppendHash, out var item))
            {
                var url = Versions.GetBundlePathOrURL(info);
                if (Application.platform == RuntimePlatform.WebGLPlayer) throw new NotImplementedException();
                if (url.StartsWith("http://") || url.StartsWith("https://") || url.StartsWith("ftp://"))
                    item = new DownloadBundle
                    {
                        pathOrURL = url,
                        info = info
                    };
                else
                    item = new LocalBundle
                    {
                        pathOrURL = url,
                        info = info
                    };

                Cache.Add(info.nameWithAppendHash, item);
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