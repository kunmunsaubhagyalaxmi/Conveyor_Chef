using UnityEngine;
using UnityEngine.SceneManagement;

namespace Watermelon
{
    /// <summary>
    /// Background music on / off (Settings and the in-game menu), apart from the sound effects:
    /// the audio module's music volume (AudioController.MusicVolume) is set here, and SOUND ON /
    /// OFF (AudioController.SetVolume) only changes the sound effects.
    /// </summary>
    public static class MusicSetting
    {
        private const string Key = "CC_Music";
        private const float Level = 0.7f;      // the music sits under the sound effects

        public static bool On => PlayerPrefs.GetInt(Key, 1) == 1;

        public static void Set(bool on)
        {
            PlayerPrefs.SetInt(Key, on ? 1 : 0);
            PlayerPrefs.Save();
            Apply();
        }

        public static void Apply()
        {
            AudioController.MusicVolume = On ? Level : 0f;
            try { AudioController.SetMusicVolume(AudioController.MusicVolume); }
            catch { }      // audio not set up yet (scene opened directly in the editor)
        }

        // before any music starts, so a track never begins at the wrong volume
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Early() => AudioController.MusicVolume = On ? Level : 0f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            AudioController.OnVolumeChanged -= OnVolumeChanged;
            AudioController.OnVolumeChanged += OnVolumeChanged;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            Apply();
        }

        private static void OnVolumeChanged(float volume) => Apply();

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Apply();
    }
}
