using System.Collections.Generic;
using UnityEngine;

namespace Versions
{
    public sealed class InstantiateObject : Operation
    {
        internal static readonly List<InstantiateObject> AllObjects = new List<InstantiateObject>();
        private Asset asset;
        public string path { get; internal set; }
        public GameObject result { get; private set; }

        public override void Start()
        {
            base.Start();
            asset = Asset.LoadAsync(path, typeof(GameObject));
            AllObjects.Add(this);
        }

        public static InstantiateObject InstantiateAsync(string assetPath)
        {
            var operation = new InstantiateObject
            {
                path = assetPath
            };
            operation.Start();
            return operation;
        }

        protected override void Update()
        {
            if (status == OperationStatus.Processing)
            {
                if (asset == null)
                {
                    Finish("asset == null");
                    return;
                }

                progress = asset.progress;
                if (!asset.isDone)
                {
                    return;
                }

                if (asset.status == LoadableStatus.FailedToLoad)
                {
                    Finish(asset.error);
                    return;
                }

                if (asset.asset == null)
                {
                    Finish("asset == null");
                    return;
                }

                result = Object.Instantiate(asset.asset as GameObject);
                Finish();
            }
        }


        public void Destroy()
        {
            if (!isDone)
            {
                Finish("User Cancelled");
                return;
            }

            if (status == OperationStatus.Success)
            {
                if (result != null)
                {
                    Object.DestroyImmediate(result);
                    result = null;
                }
            }

            if (asset != null)
            {
                if (string.IsNullOrEmpty(asset.error))
                {
                    asset.Release();
                }

                asset = null;
            }
        }

        public static void UpdateObjects()
        {
            for (var index = 0; index < AllObjects.Count; index++)
            {
                var item = AllObjects[index];
                if (Updater.Instance.busy)
                {
                    return;
                }

                if (!item.isDone || item.result != null)
                {
                    continue;
                }

                AllObjects.RemoveAt(index);
                index--;
                item.Destroy();
            }
        }
    }
}