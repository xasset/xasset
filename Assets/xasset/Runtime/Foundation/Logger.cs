using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace xasset
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        None
    }

    public static class Logger
    {
        private const string TAG = "[xasset]";
        public static LogLevel LogLevel { get; set; } = LogLevel.Debug;


        [Conditional("DEBUG")]
        public static void D(object msg, Object context = null)
        {
            if (LogLevel <= LogLevel.Debug)
                Debug.Log($"{TAG} {msg}", context);
        }

        [Conditional("DEBUG")]
        public static void I(object msg, Object context = null)
        {
            if (LogLevel <= LogLevel.Info)
                Debug.Log($"{TAG} {msg}", context);
        }

        public static void E(object msg, Object context = null)
        {
            if (LogLevel <= LogLevel.Error)
                Debug.LogError($"{TAG} {msg}", context);
        }

        public static void W(object msg, Object context = null)
        {
            if (LogLevel <= LogLevel.Warning)
                Debug.LogWarning($"{TAG} {msg}", context);
        }
    }
}