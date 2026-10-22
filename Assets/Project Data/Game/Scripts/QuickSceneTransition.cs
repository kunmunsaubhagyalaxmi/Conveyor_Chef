using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Watermelon
{
    /// <summary>
    /// A short fade between scenes, used instead of the full loading screen for quick hops
    /// (menu, World Map, Country Map, Level Selection, and every level after the first one).
    /// The screen fades to the dark kitchen blue, the scene loads, then it fades back in.
    /// Unscaled time, so it also works while the game is paused.
    /// </summary>
    public sealed class QuickSceneTransition : MonoBehaviour
    {
        private const float FadeOut = 0.16f;
        private const float FadeIn = 0.24f;
        private static readonly Color Cover = new Color(0.07f, 0.11f, 0.2f, 1f);
        private const float IndicatorDelay = 0.25f;   // quick hops never show it

        private RectTransform indicator;
        private CanvasGroup indicatorGroup;

        private static QuickSceneTransition running;

        public static bool IsRunning => running != null;

        public static void Load(string sceneName, System.Action onFinished = null)
        {
            if (running != null)
                return;

            var go = new GameObject("Quick Scene Transition", typeof(Canvas), typeof(CanvasGroup));
            DontDestroyOnLoad(go);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32000;

            var cover = new GameObject("Cover", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            cover.transform.SetParent(go.transform, false);
            var rect = (RectTransform)cover.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            cover.GetComponent<Image>().color = Cover;

            running = go.AddComponent<QuickSceneTransition>();
            running.BuildIndicator(go.transform);
            running.StartCoroutine(running.Run(sceneName, go.GetComponent<CanvasGroup>(), onFinished));
        }

        private IEnumerator Run(string sceneName, CanvasGroup group, System.Action onFinished)
        {
            group.alpha = 0f;
            group.blocksRaycasts = true;            // no double taps while switching
            for (float t = 0f; t < FadeOut; t += Time.unscaledDeltaTime)
            {
                group.alpha = t / FadeOut;
                yield return null;
            }
            group.alpha = 1f;

            AsyncOperation load = SceneManager.LoadSceneAsync(sceneName);
            float waited = 0f;
            while (load != null && !load.isDone)
            {
                waited += Time.unscaledDeltaTime;
                Animate(waited);
                yield return null;
            }
            yield return null;                       // let the new scene lay itself out

            for (float t = 0f; t < FadeIn; t += Time.unscaledDeltaTime)
            {
                group.alpha = 1f - t / FadeIn;
                yield return null;
            }

            running = null;
            onFinished?.Invoke();
            Destroy(gameObject);
        }

        // A spinning coin and LOADING on the cover, so a longer load does not look like a frozen
        // blue screen. It fades in only after IndicatorDelay.
        private void BuildIndicator(Transform parent)
        {
            MenuPanelArt art = Resources.Load<MenuPanelArt>(MenuPanelArt.ResourcePath);

            var go = new GameObject("Loading", typeof(RectTransform), typeof(CanvasGroup));
            go.transform.SetParent(parent, false);
            indicator = (RectTransform)go.transform;
            indicator.anchorMin = indicator.anchorMax = indicator.pivot = new Vector2(0.5f, 0.5f);
            indicator.sizeDelta = new Vector2(600f, 300f);
            indicatorGroup = go.GetComponent<CanvasGroup>();
            indicatorGroup.alpha = 0f;
            indicatorGroup.blocksRaycasts = false;

            var scaler = parent.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

            var coin = new GameObject("Coin", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            coin.transform.SetParent(indicator, false);
            var coinRect = (RectTransform)coin.transform;
            coinRect.anchoredPosition = new Vector2(0f, 50f);
            coinRect.sizeDelta = new Vector2(130f, 130f);
            Image coinImage = coin.GetComponent<Image>();
            coinImage.sprite = art != null ? art.coin : null;
            coinImage.preserveAspect = true;
            coinImage.enabled = coinImage.sprite != null;

            var label = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TMPro.TextMeshProUGUI));
            label.transform.SetParent(indicator, false);
            var labelRect = (RectTransform)label.transform;
            labelRect.anchoredPosition = new Vector2(0f, -70f);
            labelRect.sizeDelta = new Vector2(600f, 80f);
            var text = label.GetComponent<TMPro.TextMeshProUGUI>();
            if (art != null && art.font != null)
                text.font = art.font;
            text.text = Loc.T("LOADING");
            text.fontSize = 52f;
            text.alignment = TMPro.TextAlignmentOptions.Center;
            text.color = new Color(1f, 0.86f, 0.4f, 1f);
            text.raycastTarget = false;
        }

        private void Animate(float waited)
        {
            if (indicator == null)
                return;
            indicatorGroup.alpha = Mathf.Clamp01((waited - IndicatorDelay) / 0.2f);
            Transform coin = indicator.Find("Coin");
            if (coin != null)
                coin.localScale = new Vector3(Mathf.Cos(waited * 5f), 1f, 1f);   // a coin turning over
        }

        private void OnDestroy()
        {
            if (running == this)
                running = null;
        }
    }
}
