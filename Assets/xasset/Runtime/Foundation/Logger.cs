using System.Diagnostics;
using UnityEngine;
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

    [DisallowMultipleComponent]
    public class Logger : MonoBehaviour
    {
        [SerializeField] private LogLevel logLevel = LogLevel.Debug;
        private const string TAG = "[xasset]";


        public static LogLevel LogLevel = LogLevel.Info;

        private void Start()
        {
            LogLevel = logLevel;
        }

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

        [Conditional("UNITY_EDITOR")]
        private void Update()
        {
            LogLevel = logLevel;
        }
    }
}