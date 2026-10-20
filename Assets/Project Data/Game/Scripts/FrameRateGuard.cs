using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// The game asks for the display's top refresh rate (90 / 120 Hz, see ScreenSettings). A phone
    /// or tablet that can't keep that up shows uneven frames (stutter) and heats up, while a steady
    /// 60 looks smoother. This watches the typical frame time (median, so loading spikes don't
    /// count) and, when the device clearly can't hold its target, drops to 60 for the session.
    /// </summary>
    public sealed class FrameRateGuard : MonoBehaviour
    {
        private const int Window = 180;          // frames per check
        private const float StartDelay = 6f;     // seconds after start-up (loading) before checking
        private const float Keep = 0.85f;        // must reach 85% of the target

        private readonly float[] frames = new float[Window];
        private int count;
        private float startAt;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (!Application.isMobilePlatform)
                return;
            var go = new GameObject("Frame Rate Guard");
            DontDestroyOnLoad(go);
            go.AddComponent<FrameRateGuard>();
        }

        private void Start()
        {
            startAt = Time.realtimeSinceStartup + StartDelay;
        }

        private void Update()
        {
            int target = Application.targetFrameRate;
            if (target <= 60 || Time.realtimeSinceStartup < startAt)
            {
                count = 0;               // nothing to guard yet (the rate may be raised later)
                return;
            }

            frames[count++] = Time.unscaledDeltaTime;
            if (count < Window)
                return;
            count = 0;

            System.Array.Sort(frames);
            float median = frames[Window / 2];
            if (median > 0f && 1f / median < target * Keep)
            {
                // A 120 Hz screen that can't hold 120 often still holds 90: try that before 60.
                float fps = 1f / median;
                if (target > 90 && fps >= 90 * Keep && DisplayOffers(90))
                {
                    Application.targetFrameRate = 90;
                    startAt = Time.realtimeSinceStartup + 2f;   // let the new rate settle, then check it too
                    Debug.Log("[FrameRate] " + target + " fps not reachable (typical " + Mathf.RoundToInt(fps) + "), using 90.");
                    return;
                }

                Application.targetFrameRate = 60;
                Debug.Log("[FrameRate] " + target + " fps not reachable (typical " + Mathf.RoundToInt(fps) + "), using a steady 60.");
                enabled = false;
            }
        }

        private static bool DisplayOffers(int rate)
        {
            foreach (Resolution mode in Screen.resolutions)
                if (Mathf.Abs((float)mode.refreshRateRatio.value - rate) < 1.5f)
                    return true;
            return false;
        }
    }
}
