using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Watermelon
{
    /// <summary>
    /// Single-line texts saved in the scenes at a fixed size ran out of their boxes when the
    /// words were long (SOUTH KOREA on the Level Selection banner, other country names, longer
    /// translations). Once a scene has loaded, every one-line text that is wider than its box
    /// is set to shrink to fit; texts that fit are left exactly as they are.
    /// </summary>
    public sealed class TextOverflowGuard : MonoBehaviour
    {
        private const float MinWidth = 20f;          // smaller boxes are icons / counters sized on purpose
        private const float MinShare = 0.35f;        // never smaller than 35% of the designed size

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            GameObject go = new GameObject("Text Overflow Guard");
            DontDestroyOnLoad(go);
            go.AddComponent<TextOverflowGuard>();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            StartCoroutine(FitSoon());
        }

        private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => StartCoroutine(FitSoon());

        // after the scene's own Start (texts filled in, layouts fitted), and once more when the
        // screens that build themselves a moment later are up
        private IEnumerator FitSoon()
        {
            yield return null;
            FitAll();
            yield return new WaitForSecondsRealtime(0.5f);
            FitAll();
        }

        public static void FitAll()
        {
            foreach (TMP_Text text in Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                Fit(text);
        }

        /// <summary>Makes <paramref name="text"/> shrink to fit when it is wider than its box.</summary>
        public static bool Fit(TMP_Text text)
        {
            if (text == null || text.enableAutoSizing || text.textWrappingMode != TextWrappingModes.NoWrap || string.IsNullOrEmpty(text.text))
                return false;

            float width = text.rectTransform.rect.width - text.margin.x - text.margin.z;
            if (width < MinWidth)
                return false;

            float preferred = text.GetPreferredValues(text.text, float.PositiveInfinity, float.PositiveInfinity).x;
            if (preferred <= width * 1.02f)
                return false;

            float size = text.fontSize;
            text.fontSizeMax = size;
            text.fontSizeMin = Mathf.Max(8f, size * MinShare);
            text.enableAutoSizing = true;
            return true;
        }
    }
}
