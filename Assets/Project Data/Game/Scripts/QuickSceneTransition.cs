using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Watermelon
{
    /// <summary>
    /// Switching scenes without a cover: the current screen stays on screen (and keeps animating)
    /// while the next scene loads in the background, then the next one replaces it at once. No
    /// blue screen, no loading screen and no fades. Taps are blocked while it loads, so a
    /// button can not be pressed twice. Unscaled time, so it also works while the game is paused.
    /// </summary>
    public sealed class QuickSceneTransition : MonoBehaviour
    {
        private static QuickSceneTransition running;

        public static bool IsRunning => running != null;

        public static void Load(string sceneName, System.Action onFinished = null)
        {
            if (running != null)
                return;

            // an invisible layer over everything that only catches taps
            var go = new GameObject("Quick Scene Transition", typeof(Canvas), typeof(GraphicRaycaster));
            DontDestroyOnLoad(go);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32000;

            var blocker = new GameObject("Tap Blocker", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            blocker.transform.SetParent(go.transform, false);
            var rect = (RectTransform)blocker.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = blocker.GetComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0f);
            image.raycastTarget = true;

            running = go.AddComponent<QuickSceneTransition>();
            running.StartCoroutine(running.Run(sceneName, onFinished));
        }

        private IEnumerator Run(string sceneName, System.Action onFinished)
        {
            AsyncOperation load = SceneManager.LoadSceneAsync(sceneName);
            while (load != null && !load.isDone)
                yield return null;

            running = null;
            onFinished?.Invoke();
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (running == this)
                running = null;
        }
    }
}
