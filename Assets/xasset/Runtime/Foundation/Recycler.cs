using System.Collections.Generic;
using UnityEngine;

namespace xasset
{
    public interface IRecyclable
    {
        void EndRecycle();
        bool CanRecycle();
        bool IsUnused();
        void RecycleAsync();
        bool Recycling();
    }

    public interface IAutorelease
    {
        bool IsUnused();
        bool CanRelease();
        void Release();
    }

    [DisallowMultipleComponent]
    public class Recycler : MonoBehaviour
    {
        private static readonly List<IAutorelease> Autoreleases = new List<IAutorelease>();
        private static readonly List<IRecyclable> Recyclables = new List<IRecyclable>();
        private static readonly List<IRecyclable> Progressing = new List<IRecyclable>();
        private static readonly Queue<Object> UnusedAssets = new Queue<Object>();
        private float _lastUpdateTime;
        public static float AutoreleaseTimestep { get; set; } = 0.3f; // 300ms 一次  

        private void Update()
        {
            if (UnusedAssets.Count > 0)
            {
                while (UnusedAssets.Count > 0)
                {
                    var item = UnusedAssets.Dequeue();
                    Resources.UnloadAsset(item);
                }

                Resources.UnloadUnusedAssets();
            }

            UpdateAutorelease();
            
            for (var index = 0; index < Recyclables.Count; index++)
            {
                var request = Recyclables[index];
                if (!request.CanRecycle()) continue;

                Recyclables.RemoveAt(index);
                index--;

                // 卸载的资源加载好后，可能会被再次使用
                if (!request.IsUnused()) continue;
                request.RecycleAsync();
                Progressing.Add(request);
            }
            
            if (Scheduler.Working) return; // 有加载的时候不回收资源，防止 Unity 引擎底层出异常。 

            for (var index = 0; index < Progressing.Count; index++)
            {
                var request = Progressing[index];
                if (request.Recycling()) continue;
                Progressing.RemoveAt(index);
                index--;
                if (request.CanRecycle() && request.IsUnused()) request.EndRecycle();
                if (Scheduler.Busy) return;
            }
        }

        private void UpdateAutorelease()
        {
            if (!(Time.realtimeSinceStartup - _lastUpdateTime > AutoreleaseTimestep)) return;
            for (var index = 0; index < Autoreleases.Count; index++)
            {
                var autorelease = Autoreleases[index];
                if (!autorelease.CanRelease())
                    continue;
                Autoreleases.RemoveAt(index);
                index--;
                if (autorelease.IsUnused())
                    continue;
                autorelease.Release();
            }

            _lastUpdateTime = Time.realtimeSinceStartup;
        }

        public static void UnloadAsset(Object asset)
        {
            UnusedAssets.Enqueue(asset);
        }

        public static void RecycleAsync(IRecyclable recyclable)
        {
            // 防止重复回收
            if (Recyclables.Contains(recyclable) || Progressing.Contains(recyclable))
                return;
            Recyclables.Add(recyclable);
        }

        public static void Autorelease(IAutorelease autorelease)
        {
            Autoreleases.Add(autorelease);
        }
    }
}