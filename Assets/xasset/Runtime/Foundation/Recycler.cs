using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace xasset
{
    public interface IRecyclable
    {
        void EndRecycle();
        bool CanRecycle();
        void RecycleAsync();
        bool Recycling();
    }

    public class Recycler : MonoBehaviour
    {
        private static readonly List<IRecyclable> Recyclables = new List<IRecyclable>();
        private static readonly List<IRecyclable> Progressing = new List<IRecyclable>();
        private static readonly Queue<Object> UnusedAssets = new Queue<Object>();

        public static void UnloadAsset(Object asset)
        {
            UnusedAssets.Enqueue(asset);
        }


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

            if (Scheduler.Working) return; // 有加载的时候不回收资源，防止 Unity 引擎底层出异常。 

            for (var index = 0; index < Recyclables.Count; index++)
            {
                var request = Recyclables[index];
                if (!request.CanRecycle()) continue;

                Recyclables.RemoveAt(index);
                index--;
                request.RecycleAsync();
                Progressing.Add(request);
            }

            for (var index = 0; index < Progressing.Count; index++)
            {
                var request = Progressing[index];
                if (request.Recycling()) continue;
                Progressing.RemoveAt(index);
                index--;
                if (request.CanRecycle()) request.EndRecycle();
                if (Scheduler.Busy) return;
            }
        }

        public static void RecycleAsync(IRecyclable recyclable)
        {
            Recyclables.Add(recyclable);
        }

        public static void CancelRecycle(IRecyclable recyclable)
        {
            Progressing.Remove(recyclable);
            Recyclables.Remove(recyclable);
        }
    }
}