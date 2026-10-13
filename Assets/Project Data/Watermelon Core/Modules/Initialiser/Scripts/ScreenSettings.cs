#pragma warning disable 0649
#pragma warning disable 0414

using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class ScreenSettings
    {
        [Header("Frame Rate")]
        [SerializeField] bool setFrameRateAutomatically = false;

        [Space]
        [SerializeField] AllowedFrameRates defaultFrameRate = AllowedFrameRates.Rate60;
        [SerializeField] AllowedFrameRates batterySaveFrameRate = AllowedFrameRates.Rate30;

        [Header("Sleep")]
        [SerializeField] int sleepTimeout = -1;

        public void Initialise()
        {
            Screen.sleepTimeout = sleepTimeout;

            // Frame pacing follows targetFrameRate (vSyncCount is ignored on mobile).
            QualitySettings.vSyncCount = 0;

            if (setFrameRateAutomatically)
            {
                // The fastest mode the display offers (60 / 90 / 120 Hz...). The current mode is
                // often reported as 60 on 90-120 Hz phones until an app asks for more, so the
                // list of modes is checked too. Android switches the display to the requested rate.
                double best = Screen.currentResolution.refreshRateRatio.value;
                foreach (Resolution mode in Screen.resolutions)
                {
                    double rate = mode.refreshRateRatio.value;
                    if (rate > best)
                        best = rate;
                }

                Application.targetFrameRate = best >= 29.0
                    ? Mathf.Clamp(Mathf.RoundToInt((float)best), 30, 120)
                    : (int)defaultFrameRate;
            }
            else
            {
#if UNITY_IOS
                if(UnityEngine.iOS.Device.lowPowerModeEnabled)
                {
                    Application.targetFrameRate = (int)batterySaveFrameRate;
                }
                else
                {
                    Application.targetFrameRate = (int)defaultFrameRate;
                }    
#else
                Application.targetFrameRate = (int)defaultFrameRate;
#endif
            }
        }

        private enum AllowedFrameRates
        {
            Rate30 = 30,
            Rate60 = 60,
            Rate90 = 90,
            Rate120 = 120,
        }
    }
}
