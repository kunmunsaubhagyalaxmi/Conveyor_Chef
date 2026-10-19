using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Watermelon
{
    /// <summary>
    /// Letters for every language. The game's Fredoka / Bangers font assets only hold the 96
    /// English characters, so ñ ã ç é ü ... showed as empty boxes. At start every such font gets
    /// a fallback: the full Fredoka One font built from its .ttf (the same one outlined text uses,
    /// so accents look like the rest of the text), and behind it TextMeshPro's LiberationSans
    /// for the few letters Fredoka has not got (Turkish ğ ş İ). Play mode only adds them in
    /// memory; leaving Play mode in the editor takes them out again, so no asset changes.
    /// </summary>
    public static class LocFonts
    {
        private const string LiberationFallback = "Fonts & Materials/LiberationSans SDF - Fallback";

        private static readonly List<TMP_FontAsset> patched = new List<TMP_FontAsset>();
        private static TMP_FontAsset fredoka;
        private static TMP_FontAsset liberation;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Boot()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeChanged;
#endif
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Ensure();

        /// <summary>Adds the fallbacks to every loaded static font asset (cheap when already done).</summary>
        public static void Ensure()
        {
            if (fredoka == null)
                fredoka = OutlinedText.Font;
            if (liberation == null)
                liberation = Resources.Load<TMP_FontAsset>(LiberationFallback);

            if (fredoka != null && liberation != null)
                AddFallback(fredoka, liberation);

            foreach (TMP_FontAsset font in Resources.FindObjectsOfTypeAll<TMP_FontAsset>())
            {
                if (font == null || font == fredoka || font == liberation)
                    continue;
                if (font.atlasPopulationMode == AtlasPopulationMode.Dynamic)
                    continue;      // dynamic fonts add their own letters
                if (fredoka != null)
                    AddFallback(font, fredoka);
                if (liberation != null)
                    AddFallback(font, liberation);
            }
        }

        private static void AddFallback(TMP_FontAsset font, TMP_FontAsset fallback)
        {
            if (font.fallbackFontAssetTable == null)
                font.fallbackFontAssetTable = new List<TMP_FontAsset>();
            if (font.fallbackFontAssetTable.Contains(fallback))
                return;
            font.fallbackFontAssetTable.Add(fallback);
            if (!patched.Contains(font))
                patched.Add(font);
        }

#if UNITY_EDITOR
        private static void OnPlayModeChanged(UnityEditor.PlayModeStateChange change)
        {
            if (change != UnityEditor.PlayModeStateChange.ExitingPlayMode)
                return;
            foreach (TMP_FontAsset font in patched)
            {
                if (font == null || font.fallbackFontAssetTable == null)
                    continue;
                font.fallbackFontAssetTable.Remove(fredoka);
                font.fallbackFontAssetTable.Remove(liberation);
                font.fallbackFontAssetTable.RemoveAll(f => f == null);
            }
            patched.Clear();
            fredoka = null;
            liberation = null;
        }
#endif
    }
}
