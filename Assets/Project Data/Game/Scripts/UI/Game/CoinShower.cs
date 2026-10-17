using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    /// <summary>
    /// Win reward coins: they burst out of the reward, then fly one after another in an arc
    /// to the coin counter. Each coin that lands makes the counter pop and count up, so the
    /// number climbs with the coins instead of jumping. The reward itself is added before the
    /// flight starts (nothing is lost if the player leaves early); only the shown number is
    /// held back and counted up.
    /// </summary>
    public sealed class CoinShower : MonoBehaviour
    {
        private const string ObjectName = "Coin Shower";

        public static void Play(Transform layer, RectTransform from, CurrencyUIPanelSimple counter, int amountBefore, int reward, float delay = 0f, System.Action onFinished = null)
        {
            if (layer == null || from == null || counter == null || reward <= 0)
            {
                onFinished?.Invoke();
                return;
            }
            Transform old = layer.Find(ObjectName);
            if (old != null)
                Destroy(old.gameObject);

            var go = new GameObject(ObjectName, typeof(RectTransform));
            go.transform.SetParent(layer, false);
            go.transform.SetAsLastSibling();
            var r = (RectTransform)go.transform;
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = r.offsetMax = Vector2.zero;
            var shower = go.AddComponent<CoinShower>();
            counter.SetAmount(amountBefore);
            shower.StartCoroutine(shower.Fly(from, counter, amountBefore, reward, delay, onFinished));
        }

        /// <summary>
        /// The coin counter the player can actually see (the game screen's top bar), not one
        /// on a hidden layer. <paramref name="exclude"/>: a page whose own counters are skipped.
        /// </summary>
        public static CurrencyUIPanelSimple VisibleCoinCounter(Transform exclude) => VisibleCounter(CurrencyType.Coins, exclude);

        /// <summary>The counter of <paramref name="type"/> the player can see (the game screen's top bar).</summary>
        public static CurrencyUIPanelSimple VisibleCounter(CurrencyType type, Transform exclude)
        {
            CurrencyUIPanelSimple best = null;
            foreach (CurrencyUIPanelSimple panel in FindObjectsByType<CurrencyUIPanelSimple>(FindObjectsSortMode.None))
            {
                if (panel == null || !panel.isActiveAndEnabled || panel.Currency == null || panel.Currency.CurrencyType != type)
                    continue;
                if (exclude != null && panel.transform.IsChildOf(exclude))
                    continue;
                Canvas canvas = panel.GetComponentInParent<Canvas>();
                if (canvas == null || !canvas.isActiveAndEnabled)
                    continue;
                // the gameplay HUD's counter is the one at the top of the screen
                if (best == null || panel.GetComponentInParent<GameMainCanvas>() != null)
                    best = panel;
            }
            return best;
        }

        // Position of `target` in this layer's space, through the screen (the counter may be
        // on another canvas).
        private Vector2 LocalPoint(RectTransform target)
        {
            var layer = (RectTransform)transform;
            Canvas targetCanvas = target.GetComponentInParent<Canvas>();
            Canvas ownCanvas = layer.GetComponentInParent<Canvas>();
            Camera targetCamera = targetCanvas != null && targetCanvas.renderMode != RenderMode.ScreenSpaceOverlay ? targetCanvas.worldCamera : null;
            Camera ownCamera = ownCanvas != null && ownCanvas.renderMode != RenderMode.ScreenSpaceOverlay ? ownCanvas.worldCamera : null;
            Vector2 screen = RectTransformUtility.WorldToScreenPoint(targetCamera, target.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(layer, screen, ownCamera, out Vector2 local);
            return local;
        }

        private IEnumerator Fly(RectTransform from, CurrencyUIPanelSimple counter, int before, int reward, float delay, System.Action onFinished)
        {
            yield return Wait(delay);
            Sprite icon = CurrenciesController.GetCurrency(CurrencyType.Coins).Icon;
            var layer = (RectTransform)transform;
            Vector2 start = LocalPoint(from);
            RectTransform target = counter.RectTransform != null ? counter.RectTransform : (RectTransform)counter.transform;
            RectTransform aim = IconOf(target);   // looked up once, not every frame per coin

            int count = Mathf.Clamp(reward, 5, 12);
            int landed = 0;
            float pitch = 0.95f;
            for (int i = 0; i < count; i++)
            {
                int index = i;
                StartCoroutine(Coin(icon, start, aim, index, () =>
                {
                    landed++;
                    counter.SetAmount(before + Mathf.RoundToInt((float)reward * landed / count));
                    StartCoroutine(Punch(target));
                    try { AudioController.PlaySound(AudioController.Sounds.clickSound, 0.6f, pitch, 0.02f); } catch { }
                    pitch += 0.03f;
                }));
                yield return Wait(0.07f);
            }

            while (landed < count)
                yield return null;
            counter.SetAmount(before + reward);
            onFinished?.Invoke();
            yield return Wait(0.3f);
            Destroy(gameObject);
        }

        private IEnumerator Coin(Sprite icon, Vector2 start, RectTransform aim, int index, System.Action onLanded)
        {
            var go = new GameObject("Coin", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var image = go.AddComponent<Image>();
            image.sprite = icon;
            image.preserveAspect = true;
            image.raycastTarget = false;
            var r = (RectTransform)go.transform;
            r.sizeDelta = new Vector2(96f, 96f);
            r.anchoredPosition = start;

            // burst out around the reward
            Vector2 burst = start + Random.insideUnitCircle.normalized * Random.Range(90f, 170f);
            yield return Tween(0.22f, k =>
            {
                float e = 1f - (1f - k) * (1f - k);
                r.anchoredPosition = Vector2.Lerp(start, burst, e);
                r.localScale = Vector3.one * Mathf.Lerp(0.4f, 1.1f, e);
            });
            yield return Wait(0.05f + index * 0.015f);

            // arc to the counter (the end is read every frame, in case it moves)
            Vector2 from = r.anchoredPosition;
            float side = (index % 2 == 0 ? 1f : -1f) * Random.Range(120f, 260f);
            float spin = Random.Range(-260f, 260f);
            yield return Tween(0.55f, k =>
            {
                Vector2 end = LocalPoint(aim);
                Vector2 control = (from + end) * 0.5f + new Vector2(side, 220f);
                float e = k * k * (3f - 2f * k);
                r.anchoredPosition = Bezier(from, control, end, e);
                r.localScale = Vector3.one * Mathf.Lerp(1.1f, 0.55f, e);
                r.localEulerAngles = new Vector3(0f, 0f, spin * k);
            });
            onLanded();
            Destroy(go);
        }

        // Aim at the coin picture of the counter when it has one, not the middle of the bar.
        private static RectTransform IconOf(RectTransform counter)
        {
            foreach (Image image in counter.GetComponentsInChildren<Image>())
            {
                if (image.sprite != null && image.sprite == CurrenciesController.GetCurrency(CurrencyType.Coins).Icon)
                    return image.rectTransform;
            }
            return counter;
        }

        private static Vector2 Bezier(Vector2 a, Vector2 b, Vector2 c, float t)
        {
            float u = 1f - t;
            return u * u * a + 2f * u * t * b + t * t * c;
        }

        private static IEnumerator Punch(Transform t)
        {
            yield return Tween(0.16f, k => t.localScale = Vector3.one * (1f + 0.14f * Mathf.Sin(k * Mathf.PI)));
            t.localScale = Vector3.one;
        }

        private static IEnumerator Tween(float duration, System.Action<float> step)
        {
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                step(t / duration);
                yield return null;
            }
            step(1f);
        }

        private static IEnumerator Wait(float seconds)
        {
            for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime)
                yield return null;
        }
    }
}
