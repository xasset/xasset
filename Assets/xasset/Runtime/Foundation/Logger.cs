using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace xasset
{
    public static class Logger
    {
        private const string TAG = "[xasset]";

        public static bool Enabled { get; set; } = true;

        [Conditional("DEBUG")]
        public static void D(object msg, Object context = null)
        {
            if (Enabled)
                Debug.Log($"{TAG} {msg}", context);
        }

        public static void I(object msg, Object context = null)
        {
            Debug.Log($"{TAG} {msg}", context);
        }

        public static void E(object msg, Object context = null)
        {
            Debug.LogError($"{TAG} {msg}", context);
        }

        public static void W(object msg, Object context = null)
        {
            Debug.LogWarning($"{TAG} {msg}", context);
        }
    }
}