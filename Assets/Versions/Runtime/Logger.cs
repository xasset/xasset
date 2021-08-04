using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace VEngine
{
    public static class Logger
    {
        public static bool Loggable = true;

        public static void E(string format, params object[] args)
        {
            Debug.LogErrorFormat(format, args);
        }

        public static void W(string format, params object[] args)
        {
            Debug.LogWarningFormat(format, args);
        }

        [Conditional("DEBUG")]
        public static void I(string format, params object[] args)
        {
            if (!Loggable) return;

            Debug.LogFormat(format, args);
        }
    }
}