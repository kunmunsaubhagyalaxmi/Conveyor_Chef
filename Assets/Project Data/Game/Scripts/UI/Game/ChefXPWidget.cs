using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    /// <summary>
    /// Chef XP on the win screen: level badge, a bar that fills from the old XP to the new
    /// (rolling over when the chef levels up), "+60 XP", and LEVEL UP! with the coins earned.
    /// UIComplete shows it under the coin reward.
    /// </summary>
    public sealed class ChefXPWidget : MonoBehaviour
    {
        private const string ObjectName = "Chef XP";

        private MenuPanelArt art;
        private RectTransform fill, levelUp;
        private TextMeshProUGUI levelText, xpText, gainText, levelUpText;
        private CanvasGroup group;

        public static void Show(Transform parent, ChefProgress.Gain gain)
        {
            if (parent == null || gain.XP <= 0)
                return;
            Transform old = parent.Find(ObjectName);
            if (old != null)
                Destroy(old.gameObject);

            var go = new GameObject(ObjectName, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var widget = go.AddComponent<ChefXPWidget>();
            widget.Build();
            // The win screen is often still hidden at this point: play once it shows.
            widget.pending = gain;
            widget.queued = true;
            if (widget.isActiveAndEnabled)
                widget.OnEnable();
        }

        private ChefProgress.Gain pending;
        private bool queued;

        private void OnEnable()
        {
            if (!queued)
                return;
            queued = false;
            StartCoroutine(Play(pending));
        }

        private void Build()
        {
            art = Resources.Load<MenuPanelArt>(MenuPanelArt.ResourcePath);
            var r = (RectTransform)transform;
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
            r.anchoredPosition = new Vector2(0f, -175f);
            r.sizeDelta = new Vector2(760f, 130f);
            group = gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;

            Image back = Img("Back", r, MenuUI.Rounded(40, 5, new Color32(255, 248, 230, 245), MenuUI.Gold));
            back.type = Image.Type.Sliced;
            Stretch(back.rectTransform, 0f, 0f, 0f, 0f);

            // chef face with the level number
            Image face = Img("Chef", r, ChefProgress.Avatar(art));
            face.preserveAspect = true;
            Place(face.rectTransform, new Vector2(0f, 0.5f), new Vector2(76f, 6f), new Vector2(108f, 108f));
            Image badge = Img("Level Badge", r, MenuUI.Rounded(30, 4, new Color32(64, 150, 236, 255), Color.white));
            badge.type = Image.Type.Sliced;
            Place(badge.rectTransform, new Vector2(0f, 0.5f), new Vector2(76f, -48f), new Vector2(104f, 44f));
            levelText = Txt("Level", badge.transform, "LV 1", 26f, Color.white);
            Stretch(levelText.rectTransform, 6f, 2f, 6f, 2f);

            TextMeshProUGUI title = Txt("Title", r, "CHEF XP", 28f, new Color32(140, 70, 20, 255));
            title.alignment = TextAlignmentOptions.Left;
            Place(title.rectTransform, new Vector2(0f, 0.5f), new Vector2(330f, 34f), new Vector2(300f, 40f));
            gainText = Txt("Gain", r, "+0 XP", 34f, new Color32(40, 140, 40, 255));
            gainText.alignment = TextAlignmentOptions.Right;
            Place(gainText.rectTransform, new Vector2(1f, 0.5f), new Vector2(-120f, 34f), new Vector2(200f, 44f));

            Image track = Img("Track", r, MenuUI.Rounded(20, 0, new Color32(226, 210, 180, 255), Color.clear));
            track.type = Image.Type.Sliced;
            Place(track.rectTransform, new Vector2(0f, 0.5f), new Vector2(455f, -18f), new Vector2(560f, 40f));
            Image fillImage = Img("Fill", track.transform, MenuUI.Rounded(20, 0, new Color32(70, 190, 70, 255), Color.clear));
            fillImage.type = Image.Type.Sliced;
            fill = fillImage.rectTransform;
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = new Vector2(0f, 1f);
            fill.pivot = new Vector2(0f, 0.5f);
            fill.offsetMin = fill.offsetMax = Vector2.zero;
            xpText = Txt("XP", track.transform, "0 / 100", 24f, new Color32(60, 40, 20, 255));
            Stretch(xpText.rectTransform, 4f, 2f, 4f, 2f);

            levelUp = (RectTransform)new GameObject("Level Up", typeof(RectTransform)).transform;
            levelUp.SetParent(r, false);
            Place(levelUp, new Vector2(0.5f, 1f), new Vector2(0f, 26f), new Vector2(520f, 66f));
            Image upBack = Img("Back", levelUp, MenuUI.Rounded(32, 4, new Color32(255, 170, 30, 255), Color.white));
            upBack.type = Image.Type.Sliced;
            Stretch(upBack.rectTransform, 0f, 0f, 0f, 0f);
            levelUpText = Txt("Text", levelUp, "LEVEL UP!", 32f, Color.white);
            Stretch(levelUpText.rectTransform, 10f, 4f, 10f, 4f);
            levelUp.localScale = Vector3.zero;
        }

        private IEnumerator Play(ChefProgress.Gain gain)
        {
            ShowXP(gain.Before);
            yield return Wait(0.9f);                     // after the coin reward has landed
            yield return Tween(0.35f, k =>
            {
                group.alpha = k;
                transform.localScale = Vector3.one * Mathf.Lerp(0.85f, 1f, BackOut(k));
            });

            float duration = Mathf.Clamp(0.6f + 0.35f * (gain.NewLevel - gain.OldLevel), 0.6f, 2f);
            int lastLevel = gain.OldLevel;
            yield return Tween(duration, k =>
            {
                int xp = Mathf.RoundToInt(Mathf.Lerp(gain.Before, gain.After, EaseOut(k)));
                gainText.text = "+" + Mathf.RoundToInt(gain.XP * EaseOut(k)) + " XP";
                int level = ShowXP(xp);
                if (level > lastLevel)
                {
                    lastLevel = level;
                    StartCoroutine(Punch(levelText.transform.parent));
                }
            });
            ShowXP(gain.After);
            gainText.text = "+" + gain.XP + " XP";

            if (gain.LevelUp)
            {
                levelUpText.text = "LEVEL UP!  +" + gain.Coins.ToString("N0") + " COINS";
                try { AudioController.PlaySound(AudioController.Sounds.completeSound); } catch { }
                yield return Tween(0.4f, k => levelUp.localScale = Vector3.one * BackOut(k));
                ChefProgress.Outfit unlocked = null;
                foreach (ChefProgress.Outfit o in ChefProgress.Outfits)
                    if (o.UnlockLevel > gain.OldLevel && o.UnlockLevel <= gain.NewLevel)
                        unlocked = o;
                if (unlocked != null)
                {
                    yield return Wait(1.2f);
                    levelUpText.text = "NEW OUTFIT: " + unlocked.Name.ToUpperInvariant();
                    yield return Punch(levelUp);
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
        private static float BackOut(float k) { const float c = 1.8f; float x = Mathf.Clamp01(k) - 1f; return 1f + (c + 1f) * x * x * x + c * x * x; }

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
            t.fontSizeMin = 16f;
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
