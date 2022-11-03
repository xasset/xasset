using System.Collections.Generic;
using UnityEngine;

namespace xasset
{
    public class AutoreleaseCache
    {
        private static readonly Queue<AutoreleaseCache> Unused = new Queue<AutoreleaseCache>();
        private static readonly Dictionary<int, AutoreleaseCache> Caches = new Dictionary<int, AutoreleaseCache>();
        private static readonly List<AutoreleaseCache> Processing = new List<AutoreleaseCache>();
        private readonly List<QueueCache> _queueCaches = new List<QueueCache>();
        private readonly List<LoadRequest> _requests = new List<LoadRequest>();

        private int key;
        private Object target { get; set; }

        public static void UpdateAllCaches()
        {
            for (var index = 0; index < Processing.Count; index++)
            {
                var cache = Processing[index];
                if (!cache.IsUnused()) continue;
                Processing.RemoveAt(index);
                index--;
                Caches.Remove(cache.key);
                Unused.Enqueue(cache);
                cache.Release();
            }
        }

        private bool IsUnused()
        {
            return target == null;
        }

        public void Clear()
        {
            target = null;
        }

        private void Release()
        {
            foreach (var request in _requests)
                if (string.IsNullOrEmpty(request.error))
                    request.Release();

            _requests.Clear();
            foreach (var assets in _queueCaches) assets.Clear();
            _queueCaches.Clear();
        }

        public static AutoreleaseCache Get(Object target)
        {
            if (Caches.TryGetValue(target.GetHashCode(), out var value)) return value;
            value = Unused.Count > 0 ? Unused.Dequeue() : new AutoreleaseCache();
            value.target = target;
            value.key = target.GetHashCode();
            Caches[value.key] = value;
            Processing.Add(value);
            return value;
        }

        public void Add(QueueCache assets)
        {
            _queueCaches.Add(assets);
        }

        public void Add(LoadRequest request)
        {
            _requests.Add(request);
        }

        public void Remove(LoadRequest request)
        {
            _requests.Remove(request);
        }
    }
}