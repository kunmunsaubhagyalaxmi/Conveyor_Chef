using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.BusStop;

namespace Watermelon
{
    /// <summary>
    /// Two small helpers while a level is played, drawn on their own overlay canvas:
    ///
    /// Idle hint: after <see cref="IdleSeconds"/> without a tap, a hand taps on a good dish: one
    /// the customer at the counter wants, else one whose type already waits on the tray, else
    /// one the next customer wants, else any dish that can be picked. Any tap hides it.
    ///
    /// Tray warning: when only one spot of the tray is left, a red glow pulses around the tray,
    /// the tray gives a short shake and "TRAY ALMOST FULL!" pops up, so a full tray never comes
    /// as a surprise.
    ///
    /// Both stay hidden while the board does not take taps (paused, won, lost).
    /// LevelController starts it when a level loads and removes it when the level unloads.
    /// </summary>
    public sealed class GameplayHelpers : MonoBehaviour
    {
        private const float IdleSeconds = 6f;
        private const float TapCycle = 1.2f;

        private static GameplayHelpers instance;

        private RectTransform root;
        private MenuPanelArt art;

        // idle hint
        private RectTransform hand, ring;
        private CanvasGroup handGroup;
        private Image ringImage;
        private float idle, handAlpha, handTime;
        private BaseCharacterBehavior hinted;

        // tray warning
        private RectTransform frame, label;
        private Image frameImage, glowImage;
        private CanvasGroup warnGroup;
        private TextMeshProUGUI labelText;
        private bool warning;
        private float warnAlpha, warnTime;
        private Vector3 dockHome;
        private float shakeTime = -1f;

        public static void Begin()
        {
            End();
            if (!Application.isPlaying)
                return;
            var go = new GameObject("Gameplay Helpers", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            instance = go.AddComponent<GameplayHelpers>();
            instance.Build();
        }

        public static void End()
        {
            if (instance != null)
            {
                instance.StopShake();
                Destroy(instance.gameObject);
            }
            instance = null;
        }

        private void Build()
        {
            var canvas = GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 80;
            var scaler = GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;
            root = (RectTransform)transform;
            art = Resources.Load<MenuPanelArt>(MenuPanelArt.ResourcePath);

            // ---- tray warning: soft red glow + bright outline around the tray, label above it
            RectTransform warn = NewRect("Tray Warning", root);
            warnGroup = warn.gameObject.AddComponent<CanvasGroup>();
            warnGroup.alpha = 0f;
            warnGroup.blocksRaycasts = false;
            frame = NewRect("Frame", warn);
            glowImage = NewImage("Glow", frame, MenuUI.Rounded(40, 0, Color.white, Color.white));
            glowImage.type = Image.Type.Sliced;
            glowImage.color = new Color(1f, 0.15f, 0.1f, 0.28f);
            Stretch(glowImage.rectTransform, -26f);
            frameImage = NewImage("Outline", frame, MenuUI.Rounded(30, 8, new Color(1f, 1f, 1f, 0f), Color.white));
            frameImage.type = Image.Type.Sliced;
            frameImage.color = new Color(1f, 0.2f, 0.15f, 1f);
            Stretch(frameImage.rectTransform, -6f);

            label = NewRect("Label", warn);
            label.sizeDelta = new Vector2(620f, 74f);
            Image pill = NewImage("Back", label, MenuUI.Rounded(36, 5, new Color32(200, 30, 24, 255), Color.white));
            pill.type = Image.Type.Sliced;
            Stretch(pill.rectTransform, 0f);
            labelText = NewText("Text", label, "TRAY ALMOST FULL!", 40f, Color.white);
            Stretch(labelText.rectTransform, 14f);

            // ---- idle hint: tap ring + hand pivoted at its fingertip
            ringImage = NewImage("Tap Ring", root, art != null ? art.tapRing : null);
            ringImage.preserveAspect = true;
            ring = ringImage.rectTransform;
            ring.sizeDelta = new Vector2(140f, 140f);
            ringImage.color = new Color(1f, 1f, 1f, 0f);

            hand = NewRect("Hint Hand", root);
            hand.sizeDelta = new Vector2(124f, 160f);
            hand.pivot = new Vector2(0.371f, 0.983f);
            handGroup = hand.gameObject.AddComponent<CanvasGroup>();
            handGroup.alpha = 0f;
            handGroup.blocksRaycasts = false;
            Image handShadow = NewImage("Shadow", hand, Cine2.SoftCircle());
            handShadow.color = new Color(0f, 0f, 0f, 0.25f);
            handShadow.rectTransform.anchorMin = handShadow.rectTransform.anchorMax = new Vector2(0.55f, 0.06f);
            handShadow.rectTransform.sizeDelta = new Vector2(120f, 38f);
            Image handImage = NewImage("Hand", hand, art != null ? art.handPointer : null);
            handImage.preserveAspect = true;
            Stretch(handImage.rectTransform, 0f);
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            bool playing = RaycastController.IsEnabled && Time.timeScale > 0.01f && LevelController.Dock != null;

            // the tutorial points at dishes itself: no second (idle) hand while it runs
            UpdateHint(dt, playing && !TutorialGuide.IsRunning);
            UpdateWarning(dt, playing);
        }

        // ------------------------------------------------------------ idle hint
        private void UpdateHint(float dt, bool playing)
        {
            if (Input.GetMouseButtonDown(0) || Input.touchCount > 0 || !playing)
            {
                idle = 0f;
                hinted = null;
            }
            else
            {
                idle += dt;
            }

            if (idle >= IdleSeconds && (hinted == null || !CanPick(hinted)))
            {
                hinted = FindGoodDish();
                handTime = 0f;
            }

            bool show = idle >= IdleSeconds && hinted != null && CanPick(hinted);
            handAlpha = Mathf.MoveTowards(handAlpha, show ? 1f : 0f, dt * (show ? 3f : 8f));
            handGroup.alpha = handAlpha;
            if (handAlpha <= 0f || hinted == null)
            {
                ringImage.color = new Color(1f, 1f, 1f, 0f);
                return;
            }

            Vector2 target = ToLocal(ScreenCentre(hinted.gameObject));
            handTime += dt;
            float cycle = (handTime % TapCycle) / TapCycle;
            // lift, press, release, rest
            float press = cycle < 0.3f ? 0f : cycle < 0.42f ? Ease((cycle - 0.3f) / 0.12f) : cycle < 0.6f ? 1f - Ease((cycle - 0.42f) / 0.18f) : 0f;
            float lift = cycle < 0.3f ? 0.4f + 0.6f * Smooth(cycle / 0.3f) : cycle < 0.42f ? 1f - Ease((cycle - 0.3f) / 0.12f) : cycle < 0.6f ? 0f : 0.4f * Smooth((cycle - 0.6f) / 0.4f);
            hand.anchoredPosition = target + new Vector2(22f, -30f) * lift;
            hand.localScale = Vector3.one * (1f - 0.12f * press);
            hand.localEulerAngles = new Vector3(0f, 0f, 8f + 6f * lift);

            float ringK = cycle < 0.36f ? -1f : (cycle - 0.36f) / 0.5f;
            if (ringK >= 0f && ringK <= 1f)
            {
                ring.anchoredPosition = target;
                ring.localScale = Vector3.one * Mathf.Lerp(0.25f, 1.2f, Ease(ringK));
                ringImage.color = new Color(1f, 1f, 1f, 0.85f * (1f - ringK) * handAlpha);
            }
            else
            {
                ringImage.color = new Color(1f, 1f, 1f, 0f);
            }
        }

        private static bool CanPick(BaseCharacterBehavior c)
        {
            return c != null && c.gameObject.activeInHierarchy && c.IsHighlighted && !c.IsSubmitted && !c.IsMovementActive;
        }

        internal static BaseCharacterBehavior FindGoodDish()
        {
            IReadOnlyList<LevelElementBehavior> elements = LevelController.Elements;
            if (elements == null)
                return null;

            var wanted = new List<LevelElement.Type>();
            if (EnvironmentBehavior.CollectingBus != null)
                wanted.Add(EnvironmentBehavior.CollectingBus.Type);
            IReadOnlyList<SlotBehavior> slots = DockBehavior.Slots;
            if (slots != null)
            {
                // the type with the most dishes already waiting on the tray
                var counts = new Dictionary<LevelElement.Type, int>();
                foreach (SlotBehavior slot in slots)
                    if (slot.IsOccupied && slot.SlotCase.Behavior != null)
                    {
                        LevelElement.Type t = slot.SlotCase.Behavior.LevelElement.ElementType;
                        counts[t] = counts.TryGetValue(t, out int n) ? n + 1 : 1;
                    }
                var ordered = new List<KeyValuePair<LevelElement.Type, int>>(counts);
                ordered.Sort((a, b) => b.Value.CompareTo(a.Value));
                foreach (var pair in ordered)
                    wanted.Add(pair.Key);
            }
            if (EnvironmentBehavior.WaitingBus != null)
                wanted.Add(EnvironmentBehavior.WaitingBus.Type);

            foreach (LevelElement.Type type in wanted)
            {
                BaseCharacterBehavior found = Lowest(elements, type, true);
                if (found != null)
                    return found;
            }
            return Lowest(elements, LevelElement.Type.Empty, false);
        }

        // the pickable dish nearest the bottom of the screen (closest to the player's thumb)
        private static BaseCharacterBehavior Lowest(IReadOnlyList<LevelElementBehavior> elements, LevelElement.Type type, bool matchType)
        {
            Camera cam = Camera.main;
            BaseCharacterBehavior best = null;
            float bestY = float.MaxValue;
            foreach (LevelElementBehavior e in elements)
            {
                if (!(e is BaseCharacterBehavior c) || !CanPick(c))
                    continue;
                if (matchType && (c.LevelElement == null || c.LevelElement.ElementType != type))
                    continue;
                float y = cam != null ? cam.WorldToScreenPoint(c.transform.position).y : 0f;
                if (y < bestY)
                {
                    bestY = y;
                    best = c;
                }
            }
            return best;
        }

        // ------------------------------------------------------------ tray warning
        private void UpdateWarning(float dt, bool playing)
        {
            IReadOnlyList<SlotBehavior> slots = DockBehavior.Slots;
            int free = 0;
            if (slots != null)
                foreach (SlotBehavior s in slots)
                    if (!s.IsOccupied)
                        free++;
            bool now = playing && slots != null && slots.Count > 1 && free == 1;

            if (now && !warning)
            {
                warnTime = 0f;
                StartShake();
                try { AudioController.PlaySound(AudioController.Sounds.clickBlockedSound); } catch { }
                try { Vibration.Vibrate(VibrationIntensity.Medium); } catch { }
            }
            warning = now;

            warnAlpha = Mathf.MoveTowards(warnAlpha, warning ? 1f : 0f, dt * (warning ? 6f : 3f));
            warnGroup.alpha = warnAlpha;
            UpdateShake();
            if (warnAlpha <= 0f || slots == null || slots.Count == 0)
                return;

            // frame around the tray's spots
            Rect r = TrayScreenRect(slots);
            Vector2 a = ToLocal(r.min), b = ToLocal(r.max);
            frame.anchoredPosition = (a + b) * 0.5f;
            frame.sizeDelta = new Vector2(Mathf.Abs(b.x - a.x) + 28f, Mathf.Abs(b.y - a.y) + 28f);

            warnTime += dt;
            float pulse = 0.5f + 0.5f * Mathf.Sin(warnTime * 7f);
            frameImage.color = new Color(1f, 0.2f, 0.15f, Mathf.Lerp(0.55f, 1f, pulse));
            glowImage.color = new Color(1f, 0.15f, 0.1f, Mathf.Lerp(0.15f, 0.35f, pulse));

            // the label pops in over the tray, then breathes
            label.anchoredPosition = new Vector2(frame.anchoredPosition.x, Mathf.Max(a.y, b.y) + 60f);
            float pop = Mathf.Clamp01(warnTime / 0.35f);
            float scale = pop < 1f ? BackOut(pop) : 1f + 0.04f * Mathf.Sin(warnTime * 6f);
            label.localScale = Vector3.one * scale;
        }

        private static Rect TrayScreenRect(IReadOnlyList<SlotBehavior> slots)
        {
            Camera cam = Camera.main;
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue), max = new Vector2(float.MinValue, float.MinValue);
            foreach (SlotBehavior s in slots)
            {
                if (s == null || cam == null)
                    continue;
                Renderer r = s.BackImage;
                Bounds bounds = r != null ? r.bounds : new Bounds(s.transform.position, Vector3.one * 0.5f);
                for (int i = 0; i < 8; i++)
                {
                    Vector3 corner = bounds.center + Vector3.Scale(bounds.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                    Vector2 p = cam.WorldToScreenPoint(corner);
                    min = Vector2.Min(min, p);
                    max = Vector2.Max(max, p);
                }
            }
            return new Rect(min, max - min);
        }

        // a short shake of the tray (its home position is restored exactly)
        private void StartShake()
        {
            DockBehavior dock = LevelController.Dock;
            if (dock == null)
                return;
            if (shakeTime < 0f)
                dockHome = dock.transform.localPosition;
            shakeTime = 0f;
        }

        private void UpdateShake()
        {
            if (shakeTime < 0f)
                return;
            DockBehavior dock = LevelController.Dock;
            if (dock == null)
            {
                shakeTime = -1f;
                return;
            }
            shakeTime += Time.unscaledDeltaTime;
            const float duration = 0.4f;
            if (shakeTime >= duration)
            {
                StopShake();
                return;
            }
            float k = shakeTime / duration;
            float amount = 0.06f * (1f - k);
            dock.transform.localPosition = dockHome + new Vector3(Mathf.Sin(shakeTime * 70f) * amount, 0f, 0f);
        }

        private void StopShake()
        {
            DockBehavior dock = LevelController.Dock;
            if (shakeTime >= 0f && dock != null)
                dock.transform.localPosition = dockHome;
            shakeTime = -1f;
        }

        private void OnDestroy() => StopShake();

        // ------------------------------------------------------------ helpers
        private Vector2 ToLocal(Vector2 screen)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(root, screen, null, out Vector2 local);
            return local;
        }

        private static Vector2 ScreenCentre(GameObject go)
        {
            Camera cam = Camera.main;
            if (cam == null)
                return Vector2.zero;
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
            Bounds b = new Bounds(go.transform.position, Vector3.zero);
            bool any = false;
            foreach (Renderer r in renderers)
            {
                if (!r.enabled || r is ParticleSystemRenderer)
                    continue;
                if (!any) { b = r.bounds; any = true; }
                else b.Encapsulate(r.bounds);
            }
            return cam.WorldToScreenPoint(b.center);
        }

        private static float Ease(float k) { k = 1f - Mathf.Clamp01(k); return 1f - k * k * k; }
        private static float Smooth(float k) { k = Mathf.Clamp01(k); return k * k * (3f - 2f * k); }
        private static float BackOut(float k) { const float c = 1.9f; float x = k - 1f; return 1f + (c + 1f) * x * x * x + c * x * x; }

        private static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static Image NewImage(string name, Transform parent, Sprite sprite)
        {
            Image image = NewRect(name, parent).gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            return image;
        }

        private static void Stretch(RectTransform r, float pad)
        {
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = new Vector2(-pad, -pad);
            r.offsetMax = new Vector2(pad, pad);
        }

        private TextMeshProUGUI NewText(string name, Transform parent, string value, float size, Color color)
        {
            var t = NewRect(name, parent).gameObject.AddComponent<TextMeshProUGUI>();
            if (art != null && art.font != null)
                t.font = art.font;
            t.text = value;
            t.fontSize = size;
            t.enableAutoSizing = true;
            t.fontSizeMax = size;
            t.fontSizeMin = 18f;
            t.color = color;
            t.alignment = TextAlignmentOptions.Center;
            t.raycastTarget = false;
            return t;
        }
    }

    /// <summary>Generated soft circle (shadows), shared by the gameplay helpers.</summary>
    internal static class Cine2
    {
        private static Sprite circle;

        public static Sprite SoftCircle()
        {
            if (circle != null)
                return circle;
            const int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.DontSave };
            var px = new Color32[size * size];
            float c = (size - 1) * 0.5f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c)) / c;
                    float a = Mathf.Clamp01(1f - d);
                    a = a * a * (3f - 2f * a);
                    px[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255f));
                }
            tex.SetPixels32(px);
            tex.Apply(false, true);
            circle = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            return circle;
        }
    }
}
