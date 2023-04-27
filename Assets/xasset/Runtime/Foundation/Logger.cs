using System.Diagnostics;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace xasset
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }

    public class Logger
    {
        private const string TAG = "[xasset]";

        public static LogLevel LogLevel = LogLevel.Info;

        [Conditional("DEBUG")]
        public static void D(object msg, Object context = null)
        {
            if (LogLevel <= LogLevel.Debug)
                Debug.Log($"{TAG} {msg}", context);
        }

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