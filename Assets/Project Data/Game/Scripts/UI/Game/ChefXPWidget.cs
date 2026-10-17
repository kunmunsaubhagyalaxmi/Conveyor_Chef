using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    /// <summary>
    /// Chef XP strip at the bottom of the win banner: chef face with "LV n", a bar that fills
    /// from the old XP to the new (rolling over when the chef levels up) and "+60 XP", which
    /// turns into LEVEL UP! / NEW OUTFIT!. UIComplete places it where GET X3 was.
    /// It can live in the scene ("Chef XP" under UI Complete, added with
    /// Conveyor Chef > UI Panels > Edit Win Panel) so it can be moved and restyled in the
    /// editor; the game then uses that one as it is. Without it, the game builds one.
    /// Bar art: Resources/UI/ChefXP (9-sliced).
    /// </summary>
    public sealed class ChefXPWidget : MonoBehaviour
    {
        public const string ObjectName = "Chef XP";
        private const float TrackWidth = 500f;

                private const string SpriteFolder = "UI/ChefXP/";

        [SerializeField] private RectTransform fill;
        private Image fillImage;
        [SerializeField] private Image face;
        [SerializeField] private TextMeshProUGUI levelText, xpText, gainText;
        [SerializeField] private CanvasGroup group;
        [SerializeField] private Color gainColor = new Color32(40, 140, 40, 255);
        [SerializeField] private Color levelUpColor = new Color32(214, 120, 0, 255);

        private MenuPanelArt art;

        private ChefProgress.Gain pending;
        private bool queued;

        /// <param name="position">Centre of the strip, relative to the centre of <paramref name="parent"/>.</param>
        public static void Show(Transform parent, ChefProgress.Gain gain, Vector2 position)
        {
            if (parent == null)
                return;

            // One placed in the scene is used as it is (its position and look are the editor's).
            Transform existing = parent.Find(ObjectName);
            ChefXPWidget widget = existing != null ? existing.GetComponent<ChefXPWidget>() : null;
            if (widget == null || !widget.IsComplete || !widget.UsesCurrentArt)
            {
                // an older bar placed in the scene keeps its place, with the current look
                if (existing != null)
                {
                    position = ((RectTransform)existing).anchoredPosition;
                    existing.gameObject.SetActive(false);
                    existing.name = ObjectName + " (old)";
                    Destroy(existing.gameObject);
                }
                widget = Create(parent, position);
            }

            if (gain.XP <= 0)
            {
                widget.gameObject.SetActive(false);
                return;
            }
            widget.gameObject.SetActive(true);
            widget.transform.SetAsLastSibling();       // above the banner frame
            widget.StopAllCoroutines();
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

        private bool UsesCurrentArt => fill != null && fill.GetComponent<Image>() != null && fill.GetComponent<Image>().type == Image.Type.Filled;

        private bool IsComplete => fill != null && face != null && levelText != null && xpText != null && gainText != null && group != null;

        /// <summary>Makes the strip under <paramref name="parent"/> (also used by the editor menu).</summary>
        public static ChefXPWidget Create(Transform parent, Vector2 position)
        {
            var go = new GameObject(ObjectName, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.transform.SetAsLastSibling();
            var widget = go.AddComponent<ChefXPWidget>();
            widget.Build(position);
            return widget;
        }

        /// <summary>Editor preview: the bar at the player's current XP, fully visible.</summary>
        public void ShowPreview()
        {
            if (!IsComplete)
                return;
            group.alpha = 1f;
            ShowXP(ChefProgress.TotalXP);
            gainText.text = "+" + (ChefProgress.XPPerWin + 3 * ChefProgress.XPPerStar) + " XP";
        }

        private static Sprite Art(string name) => Resources.Load<Sprite>(SpriteFolder + name);

        private void Build(Vector2 position)
        {
            art = Resources.Load<MenuPanelArt>(MenuPanelArt.ResourcePath);
            var r = (RectTransform)transform;
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
            r.anchoredPosition = position;
            r.sizeDelta = new Vector2(760f, 120f);
            group = gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;

            // bar: the game's own progress bar art (a matched track + fill pair, same as the
            // Country Map), the fill revealed from the left
            Image track = Img("Track", r, Art("xp_track"));
            Place(track.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(45f, -6f), new Vector2(TrackWidth, 104f));
            fillImage = Img("Fill", track.transform, Art("xp_fill"));
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            fill = fillImage.rectTransform;
            Stretch(fill, 0f, 0f, 0f, 0f);
            xpText = MenuUI.OutlinedText("XP", track.transform, "0 / 100 XP", 30f, new Color32(16, 38, 90, 255));
            Stretch(xpText.rectTransform, 40f, 20f, 40f, 22f);

            // chef face in the gold ring, the level on a gold badge under it
            Image ring = Img("Ring", r, Art("xp_ring"));
            ring.preserveAspect = true;
            Place(ring.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(-272f, 4f), new Vector2(132f, 132f));
            face = Img("Chef", ring.transform, ChefProgress.Avatar(art));
            face.preserveAspect = true;
            Place(face.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 2f), new Vector2(100f, 100f));
            Image badge = Img("Level Badge", ring.transform, Art("xp_badge"));
            badge.preserveAspect = true;
            Place(badge.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, -8f), new Vector2(118f, 59f));
            levelText = Txt("Level", badge.transform, "LV 1", 28f, new Color32(92, 52, 24, 255));
            Stretch(levelText.rectTransform, 16f, 10f, 16f, 10f);

            gainText = MenuUI.OutlinedText("Gain", r, "+0 XP", 40f, new Color32(255, 255, 255, 255), TMPro.TextAlignmentOptions.Center, gainColor);
            Place(gainText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(45f, 62f), new Vector2(360f, 54f));
        }

        private IEnumerator Play(ChefProgress.Gain gain)
        {
            // reset from the last win (or the editor preview)
            if (art == null)
                art = Resources.Load<MenuPanelArt>(MenuPanelArt.ResourcePath);
            Sprite chef = ChefProgress.Avatar(art);
            if (chef != null)
                face.sprite = chef;
            group.alpha = 0f;
            gainText.color = gainColor;
            gainText.transform.localScale = Vector3.one;
            face.transform.localScale = Vector3.one;
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
                    StartCoroutine(Punch(face.transform));
                }
            });
            ShowXP(gain.After);
            gainText.text = "+" + gain.XP + " XP";

            if (gain.LevelUp)
            {
                yield return Wait(0.4f);
                try { AudioController.PlaySound(AudioController.Sounds.completeSound); } catch { }
                gainText.color = levelUpColor;
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
            if (fillImage == null && fill != null)
                fillImage = fill.GetComponent<Image>();
            float k = Mathf.Clamp01((float)into / need);
            // the fill picture's bar runs from 6.6% to 93.4% of its width
            if (fillImage != null)
                fillImage.fillAmount = k <= 0f ? 0f : Mathf.Lerp(143f / 2172f, 2028f / 2172f, k);
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
