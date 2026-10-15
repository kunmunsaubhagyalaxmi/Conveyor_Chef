using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    /// <summary>
    /// Chef XP strip at the bottom of the win banner: chef face with "LV n", a bar that fills
    /// from the old XP to the new (rolling over when the chef levels up) and "+60 XP", which
    /// turns into LEVEL UP! / NEW OUTFIT!. UIComplete places it under the GET X3 button.
    /// </summary>
    public sealed class ChefXPWidget : MonoBehaviour
    {
        private const string ObjectName = "Chef XP";
        private const float TrackWidth = 400f;

        private MenuPanelArt art;
        private RectTransform fill, face;
        private TextMeshProUGUI levelText, xpText, gainText;
        private CanvasGroup group;

        private ChefProgress.Gain pending;
        private bool queued;

        /// <param name="position">Centre of the strip, relative to the centre of <paramref name="parent"/>.</param>
        public static void Show(Transform parent, ChefProgress.Gain gain, Vector2 position)
        {
            if (parent == null || gain.XP <= 0)
                return;
            Transform old = parent.Find(ObjectName);
            if (old != null)
                Destroy(old.gameObject);

            var go = new GameObject(ObjectName, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.transform.SetAsLastSibling();           // above the banner frame
            var widget = go.AddComponent<ChefXPWidget>();
            widget.Build(position);
            // The win screen is often still hidden at this point: play once it shows.
            widget.pending = gain;
            widget.queued = true;
            if (widget.isActiveAndEnabled)
                widget.OnEnable();
        }

        private void OnEnable()
        {
            if (!queued)
                return;
            queued = false;
            StartCoroutine(Play(pending));
        }

        private void Build(Vector2 position)
        {
            art = Resources.Load<MenuPanelArt>(MenuPanelArt.ResourcePath);
            var r = (RectTransform)transform;
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
            r.anchoredPosition = position;
            r.sizeDelta = new Vector2(600f, 70f);
            group = gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;

            // bar
            Image track = Img("Track", r, MenuUI.Rounded(22, 4, new Color32(232, 214, 184, 255), new Color32(176, 120, 60, 255)));
            track.type = Image.Type.Sliced;
            Place(track.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(-10f, 0f), new Vector2(TrackWidth, 44f));
            Image fillImage = Img("Fill", track.transform, MenuUI.Rounded(18, 0, new Color32(84, 196, 64, 255), Color.clear));
            fillImage.type = Image.Type.Sliced;
            fill = fillImage.rectTransform;
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = new Vector2(0f, 1f);
            fill.pivot = new Vector2(0f, 0.5f);
            fill.offsetMin = new Vector2(4f, 4f);
            fill.offsetMax = new Vector2(-4f, -4f);
            xpText = Txt("XP", track.transform, "0 / 100 XP", 24f, new Color32(70, 40, 16, 255));
            Stretch(xpText.rectTransform, 10f, 2f, 10f, 2f);

            // chef face with the level under it, overlapping the bar's left end
            Image faceImage = Img("Chef", r, ChefProgress.Avatar(art));
            faceImage.preserveAspect = true;
            face = faceImage.rectTransform;
            Place(face, new Vector2(0.5f, 0.5f), new Vector2(-240f, 8f), new Vector2(80f, 80f));
            Image badge = Img("Level Badge", face, MenuUI.Rounded(16, 3, new Color32(64, 150, 236, 255), Color.white));
            badge.type = Image.Type.Sliced;
            Place(badge.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, -6f), new Vector2(76f, 32f));
            levelText = Txt("Level", badge.transform, "LV 1", 22f, Color.white);
            Stretch(levelText.rectTransform, 4f, 1f, 4f, 1f);

            gainText = Txt("Gain", r, "+0 XP", 32f, new Color32(40, 140, 40, 255));
            Place(gainText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(255f, 0f), new Vector2(130f, 50f));
        }

        private IEnumerator Play(ChefProgress.Gain gain)
        {
            ShowXP(gain.Before);
            yield return Wait(0.6f);                     // after the banner has popped in
            yield return Tween(0.3f, k => group.alpha = k);

            float duration = Mathf.Clamp(0.7f + 0.35f * (gain.NewLevel - gain.OldLevel), 0.7f, 2f);
            int lastLevel = gain.OldLevel;
            yield return Tween(duration, k =>
            {
                int xp = Mathf.RoundToInt(Mathf.Lerp(gain.Before, gain.After, EaseOut(k)));
                gainText.text = "+" + Mathf.RoundToInt(gain.XP * EaseOut(k)) + " XP";
                int level = ShowXP(xp);
                if (level > lastLevel)
                {
                    lastLevel = level;
                    StartCoroutine(Punch(face));
                }
            });
            ShowXP(gain.After);
            gainText.text = "+" + gain.XP + " XP";

            if (gain.LevelUp)
            {
                yield return Wait(0.4f);
                try { AudioController.PlaySound(AudioController.Sounds.completeSound); } catch { }
                gainText.color = new Color32(214, 120, 0, 255);
                gainText.text = "LEVEL UP!";
                yield return Punch(gainText.transform);

                foreach (ChefProgress.Outfit o in ChefProgress.Outfits)
                {
                    if (o.UnlockLevel > gain.OldLevel && o.UnlockLevel <= gain.NewLevel)
                    {
                        yield return Wait(1.2f);
                        gainText.text = "NEW OUTFIT!";
                        yield return Punch(gainText.transform);
                        break;
                    }
                }
            }
        }

        private int ShowXP(int xp)
        {
            int level = ChefProgress.LevelAt(xp, out int into, out int need);
            levelText.text = "LV " + level;
            xpText.text = into + " / " + need + " XP";
            fill.anchorMax = new Vector2(Mathf.Clamp01((float)into / need), 1f);
            return level;
        }

        private static IEnumerator Punch(Transform t)
        {
            if (t == null)
                yield break;
            yield return Tween(0.3f, k => t.localScale = Vector3.one * (1f + 0.25f * Mathf.Sin(k * Mathf.PI)));
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

        private static float EaseOut(float k) { k = 1f - Mathf.Clamp01(k); return 1f - k * k * k; }

        private static Image Img(string name, Transform parent, Sprite sprite)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            Image image = go.AddComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            return image;
        }

        private TextMeshProUGUI Txt(string name, Transform parent, string value, float size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<TextMeshProUGUI>();
            if (art != null && art.font != null)
                t.font = art.font;
            t.text = value;
            t.fontSize = size;
            t.enableAutoSizing = true;
            t.fontSizeMax = size;
            t.fontSizeMin = 14f;
            t.color = color;
            t.alignment = TextAlignmentOptions.Center;
            t.raycastTarget = false;
            return t;
        }

        private static void Stretch(RectTransform r, float left, float bottom, float right, float top)
        {
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = new Vector2(left, bottom);
            r.offsetMax = new Vector2(-right, -top);
        }

        private static void Place(RectTransform r, Vector2 anchor, Vector2 position, Vector2 size)
        {
            r.anchorMin = r.anchorMax = anchor;
            r.anchoredPosition = position;
            r.sizeDelta = size;
        }
    }
}
