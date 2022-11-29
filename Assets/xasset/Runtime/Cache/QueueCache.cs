using System.Collections.Generic;
using UnityEngine;

namespace xasset
{
    public class QueueCache // First in first out
    {
        private static readonly Queue<QueueCache> Unused = new Queue<QueueCache>();
        private readonly Queue<LoadRequest> _queue = new Queue<LoadRequest>();
        public byte maxQueueSize { get; set; } = 5;

        public void Clear()
        {
            while (_queue.Count > 0)
            {
                var request = _queue.Dequeue();
                request.Release();
            }

            Unused.Enqueue(this);
        }

        public static QueueCache Get(Object target)
        {
            var fifoAutoreleaseAssets = Unused.Count > 0 ? Unused.Dequeue() : new QueueCache();
            var autoreleaseAssets = AutoreleaseCache.Get(target);
            autoreleaseAssets.Add(fifoAutoreleaseAssets);
            return fifoAutoreleaseAssets;
        }

        public LoadRequest Add(LoadRequest asset)
        {
            if (asset != null) _queue.Enqueue(asset);
            if (_queue.Count <= maxQueueSize) return asset;
            var request = _queue.Dequeue();
            request.Release();
            return asset;
        }
    }
}