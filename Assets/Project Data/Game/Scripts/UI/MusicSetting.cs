using UnityEngine;
using UnityEngine.SceneManagement;

namespace Watermelon
{
    /// <summary>
    /// Background music on / off (Settings and the in-game menu), apart from the sound effects.
    /// The audio module only has one volume for both, so the music sources are muted here and
    /// muted again whenever that volume changes or a scene loads (new music sources start at the
    /// shared volume).
    /// </summary>
    public static class MusicSetting
    {
        private const string Key = "CC_Music";

        public static bool On => PlayerPrefs.GetInt(Key, 1) == 1;

        public static void Set(bool on)
        {
            PlayerPrefs.SetInt(Key, on ? 1 : 0);
            PlayerPrefs.Save();
            Apply();
        }

        public static void Apply()
        {
            try { AudioController.SetMusicVolume(On ? AudioController.GetVolume() : 0f); }
            catch { }      // audio not set up yet (scene opened directly in the editor)
        }

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
