using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// Release builds keep only warnings and errors in the log. Every Debug.Log on a phone
    /// captures a stack trace, and gameplay logs on each level load and button press, which
    /// shows up as small hitches. Development builds and the Editor keep the full log.
    /// </summary>
    public static class ReleaseLogging
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Apply()
        {
            if (!Debug.isDebugBuild)
                Debug.unityLogger.filterLogType = LogType.Warning;
        }
    }
}
