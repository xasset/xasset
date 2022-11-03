using System.Collections.Generic;
using UnityEngine;

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
        private static readonly Dictionary<int, IRecyclable> Recyclables = new Dictionary<int, IRecyclable>();
        private static readonly List<IRecyclable> Progressing = new List<IRecyclable>();
        private static readonly Queue<IRecyclable> Unused = new Queue<IRecyclable>();


        private void Update()
        {
            if (Scheduler.Working) return; // 有加载的时候不回收资源，防止 Unity 引擎底层出异常。 

            foreach (var pair in Recyclables)
            {
                var request = pair.Value;
                if (!request.CanRecycle()) continue;
                request.RecycleAsync();
                Unused.Enqueue(request);
            }

            while (Unused.Count > 0)
            {
                var request = Unused.Dequeue();
                Recyclables.Remove(request.GetHashCode());
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
            Recyclables[recyclable.GetHashCode()] = recyclable;
        }

        public static void CancelRecycle(IRecyclable recyclable)
        {
            Recyclables.Remove(recyclable.GetHashCode());
        }
    }
}