using UnityEngine;
using UnityEngine.SceneManagement;

namespace Watermelon
{
    /// <summary>
    /// Lobby music in the menus (main menu, World Map, Country Map, Level Selection) and gameplay
    /// music in the levels. The lobby track is the Audio Settings' game music; the gameplay track is
    /// Resources/Audio/music_gameplay (any audio format). Without a gameplay track the lobby track
    /// simply keeps playing everywhere. No music on the start-up and loading screens (Init,
    /// loading): it starts when the game itself opens. Switching happens when a scene loads; the
    /// MUSIC / SOUND settings still apply to whichever track plays.
    /// </summary>
    public static class MusicDirector
    {
        private const string GameplayScene = "Game";
        private const string GameplayTrack = "Audio/music_gameplay";
        private static readonly string[] SilentScenes = { "Init", "loading" };

        private static AudioClip playing;
        private static AudioClip gameplay;
        private static bool gameplayLoaded;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            Apply(SceneManager.GetActiveScene());
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Single)
                Apply(scene);
        }

        private static void Apply(Scene scene)
        {
            Music music = AudioController.Music;
            AudioClip lobby = music != null ? music.gameMusic : null;
            if (lobby == null)
                return;                       // audio not set up yet (a scene opened on its own in the editor)

            // start-up and loading screens: silence (the music starts when the game opens)
            if (System.Array.IndexOf(SilentScenes, scene.name) >= 0)
            {
                try { AudioController.ReleaseMusic(); } catch { }
                playing = null;
                return;
            }

            if (!gameplayLoaded)
            {
                gameplay = Resources.Load<AudioClip>(GameplayTrack);
                gameplayLoaded = true;
            }

            AudioClip wanted = scene.name == GameplayScene && gameplay != null ? gameplay : lobby;
            if (wanted == playing)
                return;

            try
            {
                AudioController.ReleaseMusic();
                AudioController.PlayMusic(wanted);
                playing = wanted;
                MusicSetting.Apply();         // keep MUSIC OFF / SOUND OFF as set
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[Music] Could not switch track: " + ex.Message);
            }
        }
    }
}
