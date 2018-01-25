using UnityEngine;


namespace XAsset
{
    public enum LogMode
    {
        All,
        JustErrors
    }

    public enum LogType
    {
        Info,
        Warning,
        Error
    }

    public class Logger
    {
        static LogMode logMode = LogMode.All;

        protected Logger()
        {

        }
#if !UNITY_EDITOR
		[System.Diagnostics.Conditional("DEVELOPMENT_BUILD")] 
#endif
        public static void L(LogType type, string tag, string message)
        {
            if (logMode == LogMode.JustErrors && type != LogType.Error)
            {
                return;
            }
            switch (type)
            {
                case LogType.Info:
                    Debug.Log(string.Format("[{0}] {1}", tag, message));
                    break;
                case LogType.Warning:
                    Debug.LogWarning(string.Format("[{0}] {1}", tag, message));
                    break;
                case LogType.Error:
                    Debug.LogError(string.Format("[{0}] {1}", tag, message));
                    break;
                default:
                    Debug.Log(string.Format("[{0}] {1}", tag, message)); 
                    break;
            }
        }

#if !UNITY_EDITOR
		[System.Diagnostics.Conditional("DEVELOPMENT_BUILD")] 
#endif
        protected void I(string message)
        {
            L(LogType.Info, GetType().Name, message);
        }

#if !UNITY_EDITOR
		[System.Diagnostics.Conditional("DEVELOPMENT_BUILD")] 
#endif
        protected void W(string message)
        {
            L(LogType.Warning, GetType().Name, message);
        }

#if !UNITY_EDITOR
		[System.Diagnostics.Conditional("DEVELOPMENT_BUILD")] 
#endif
        protected void E(string message)
        {
            L(LogType.Error, GetType().Name, message);
        }
    }

}