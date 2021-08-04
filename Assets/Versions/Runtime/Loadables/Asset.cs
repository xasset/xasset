using System;
using System.Collections;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace VEngine
{
    public class Asset : Loadable, IEnumerator
    {
        public static readonly Dictionary<string, Asset> Cache = new Dictionary<string, Asset>();

        public static readonly List<Asset> Unused = new List<Asset>();

        public Action<Asset> completed;

        public Object asset { get; protected set; }

        protected Type type { get; set; }

        public bool MoveNext()
        {
            return !isDone;
        }

        public void Reset()
        {
        }

        public object Current => null;

        public T Get<T>() where T : Object
        {
            return asset as T;
        }

        protected override void OnComplete()
        {
            if (completed == null) return;

            var saved = completed;
            if (completed != null) completed(this);

            completed -= saved;
        }

        protected override void OnUnused()
        {
            completed = null;
            Unused.Add(this);
        }

        public static Asset LoadAsync(string path, Type type, Action<Asset> completed = null)
        {
            return LoadInternal(path, type, false, completed);
        }

        public static Asset Load(string path, Type type)
        {
            return LoadInternal(path, type, true);
        }

        internal static Asset LoadInternal(string path, Type type, bool mustCompleteOnNextFrame,
            Action<Asset> completed = null)
        {
            if (!Versions.Contains(path))
            {
                Logger.E("FileNotFoundException {0}", path);
                return null;
            }

            if (!Cache.TryGetValue(path, out var item))
            {
                item = Versions.CreateAsset(path, type);
                Cache.Add(path, item);
            }

            if (completed != null) item.completed += completed;

            item.mustCompleteOnNextFrame = mustCompleteOnNextFrame;
            item.Load();
            if (mustCompleteOnNextFrame) item.LoadImmediate();

            return item;
        }

        public static void UpdateAssets()
        {
            for (var index = 0; index < Unused.Count; index++)
            {
                var item = Unused[index];
                if (!item.isDone) continue;

                Unused.RemoveAt(index);
                index--;
                if (!item.reference.unused) continue;

                item.Unload();
                Cache.Remove(item.pathOrURL);
            }
        }
    }
}