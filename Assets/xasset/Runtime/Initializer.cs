using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace xasset
{
    public static class Initializer
    {
        public static Action<GameObject> OnLoad { get; set; } = null;

        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            const string name = "Global";
            var go = new GameObject(name,
                typeof(Scheduler),
                typeof(Downloader),
                typeof(Recycler),
                typeof(AutoreleaseTimer),
                typeof(Logger)
            );
            Object.DontDestroyOnLoad(go);
            OnLoad?.Invoke(go);
        }
    }
}