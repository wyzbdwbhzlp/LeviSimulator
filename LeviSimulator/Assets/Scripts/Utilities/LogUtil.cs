using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities.Danmaku;
using Debug = UnityEngine.Debug;

namespace Utilities
{

    public static class LogUtil
    {
        public enum LogLevel
        {
            Log,
            Warning,
            Error,
            Exception
        }

        public static bool EnableLog = true;

        public static void Log(object message, bool enablePopUpFeedback = false)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!EnableLog) return;
            var formattedMessage = Format(message, LogLevel.Log);
            Debug.Log(formattedMessage);
            if (enablePopUpFeedback)
            {
                CallPopup(formattedMessage);
            }
#endif
        }

        public static void LogWarning(object message, bool enablePopUpFeedback = false)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!EnableLog) return;
            var formattedMessage = Format(message, LogLevel.Log);
            Debug.LogWarning(formattedMessage);
            if (enablePopUpFeedback)
            {
                CallPopup(formattedMessage);
            }
#endif
        }

        public static void LogError(object message, bool enablePopUpFeedback = false)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!EnableLog) return;
            var formattedMessage = Format(message, LogLevel.Log);
            Debug.LogError(formattedMessage);
            if (enablePopUpFeedback)
            {
                CallPopup(formattedMessage);
            }
#endif
        }

        public static void LogException(Exception ex, bool enablePopUpFeedback = false)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!EnableLog) return;
            Debug.LogException(ex);
            if (enablePopUpFeedback)
            {
                CallPopup(ex.Message);
            }
#endif
        }


        private static string Format(object message, LogLevel level)
        {
            string caller = GetCallingClassName();
            return $"[{caller}] {message}";
        }

        private static string GetCallingClassName()
        {
            // 跳过 LogUtil 本身的调用栈
            var stackTrace = new StackTrace();
            for (int i = 2; i < stackTrace.FrameCount; i++)
            {
                var method = stackTrace.GetFrame(i).GetMethod();
                var type = method?.DeclaringType;
                if (type != typeof(LogUtil) && type != null)
                {
                    return type.Name;
                }
            }

            return "Unknown";
        }

        private static void CallPopup(string message)
        {
            var danmakuManager =DanmakuManager.Instance;
            if (danmakuManager == null)
            {
                Debug.LogWarning("DanmakuManager不存在，无法显示弹幕消息");
                return;
            }

            danmakuManager.ShowMessage(message);
        }
    }
}