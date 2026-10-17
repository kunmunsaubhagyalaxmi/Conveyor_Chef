using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.BusStop;

namespace Watermelon
{
    /// <summary>
    /// Teaches new players (Hinglish text, the chef in a speech bubble):
    ///  - Level 1, once: the tray's colour -> tap the dish of that colour; 3 dishes fill a tray;
    ///    wrong colours wait in the 7-spot row (full row = lose); serve every order to win.
    ///    The rest of the screen dims around what is being explained.
    ///  - Later, once each, the first time they appear: hidden ("?") dishes, the spawner box
    ///    and each power-up.
    /// Every step can be skipped (SKIP turns the whole tutorial off). Seen steps are kept in
    /// PlayerPrefs. LevelController calls <see cref="OnLevelStarted"/> / <see cref="End"/>.
    /// </summary>
    public sealed class TutorialGuide : MonoBehaviour
    {
        private const string KeyPrefix = "CC_Tutorial_";
        private const string Level1Key = KeyPrefix + "Level1";
        private const string SkippedKey = KeyPrefix + "Skipped";

        private static TutorialGuide instance;

        private MenuPanelArt art;
        private RectTransform root;
        private readonly Image[] dims = new Image[4];
        private Image frame, hand, chef, bubble;
        private TextMeshProUGUI message;
        private Button nextButton, skipButton, blocker;
        private TextMeshProUGUI nextLabel;
        private bool advance;
        private Rect? spot;

        // ------------------------------------------------------------------ entry points

        /// <summary>True when the chef lesson runs on this level (level 1, not seen yet).</summary>
        public static bool TeachesLevel(int levelIndex) =>
            levelIndex == 0 && PlayerPrefs.GetInt(SkippedKey, 0) == 0 && PlayerPrefs.GetInt(Level1Key, 0) == 0;

        public static void OnLevelStarted(int levelIndex)
        {
            End();
            if (PlayerPrefs.GetInt(SkippedKey, 0) == 1)
                return;

            bool level1 = TeachesLevel(levelIndex);
            if (!level1 && levelIndex < 2)
                return;

            var go = new GameObject("Tutorial Guide", typeof(RectTransform));
            instance = go.AddComponent<TutorialGuide>();
            instance.Build();
            instance.StartCoroutine(level1 ? instance.Level1() : instance.Tips(levelIndex));
        }

        public static void End()
        {
            if (instance == null)
                return;
            instance.ReleaseInput();
            Destroy(instance.gameObject);
            instance = null;
        }

        private void OnDestroy()
        {
            ReleaseInput();
            if (instance == this)
                instance = null;
        }

        // Taps on the board are switched off only while an explanation shows, and always
        // switched back on by whoever switched them off (also at the start of a level, before
        // the first tap has started the game).
        private bool inputHeld;

        private void HoldInput()
        {
            if (inputHeld)
                return;
            inputHeld = true;
            RaycastController.Disable();
        }

        private void ReleaseInput()
        {
            if (!inputHeld)
                return;
            inputHeld = false;
            RaycastController.Enable();
        }

        // ------------------------------------------------------------------ level 1

        private IEnumerator Level1()
        {
            yield return WaitForTray();

            // 1. the tray's colour -> the dish of that colour (the chef's hand points at one)
            int start = OrdersDone();
            int lastSeats = -1;
            Show("Tray ka colour dekho! Neeche wahi colour ki dish par tap karo.", Tray(), interactive: true);
            while (OrdersDone() == start)
            {
                PointAtDish(GameplayHelpers.FindGoodDish());
                BusBehavior tray = EnvironmentBehavior.CollectingBus;
                int seats = tray != null ? tray.PassengersCount : 0;
                if (seats != lastSeats && seats > 0)
                {
                    lastSeats = seats;
                    message.text = seats == 1 ? "Shabaash! Ek tray mein 3 dishes jaati hain - 2 aur!"
                        : seats == 2 ? "Bas ek aur!" : message.text;
                }
                SetSpot(Tray());
                yield return null;
            }
            yield return Wait(0.6f);
            hand.gameObject.SetActive(false);

            // 2. trays
            yield return WaitForTray();
            yield return Step("Shabaash! Tray bhar gayi aur customer khush hokar chala gaya. Har nayi tray ko uske colour ki 3 dishes chahiye.",
                Tray(), "AAGE");

            // 3. the waiting row
            yield return Step("Galat colour ki dish yahan neeche ruk jaati hai. Sirf 7 jagah hain - sab bhar gayi to level haar jaoge!",
                Dock(), "AAGE");
            // 4. the orders
            yield return Step("Upar ORDERS mein dekho kya chahiye. Saare orders poore karo aur level jeeto!",
                Orders(), "AAGE");
            // 5. the power-ups, one by one (also marks their later tips as seen)
            foreach (var tip in PowerUpTips(int.MaxValue))
            {
                yield return Step(tip.text, tip.target(), "AAGE");
                PlayerPrefs.SetInt(KeyPrefix + tip.key, 1);
            }
            yield return Step("Bas itna hi! Ab tum khud try karo.", null, "CHALO KHELEIN!");

            PlayerPrefs.SetInt(Level1Key, 1);
            PlayerPrefs.Save();
            End();
        }

        // ------------------------------------------------------------------ one-time tips

        private IEnumerator Tips(int levelIndex)
        {
            var tips = new List<(string key, string text, System.Func<Rect?> target)>();

            if (HasHiddenDishes())
                tips.Add(("Hidden", "Yeh dish dhakki hui hai! Iske aas-paas ki dishes hatao, tab yeh khulegi.", HiddenDish));
            if (LevelController.LoadedStageData != null && LevelController.LoadedStageData.SpawnerData != null &&
                LevelController.LoadedStageData.SpawnerData.Length > 0)
                tips.Add(("Spawner", "Yeh hara dabba nayi dishes deta hai! Number batata hai iske andar kitni dishes baaki hain. Iske saamne wali dish uthao, agli apne aap bahar aayegi.", Spawner));
            tips.AddRange(PowerUpTips(levelIndex));

            tips.RemoveAll(t => PlayerPrefs.GetInt(KeyPrefix + t.key, 0) == 1);
            if (tips.Count == 0)
            {
                End();
                yield break;
            }

            yield return WaitForTray();
            yield return Wait(0.4f);
            foreach (var tip in tips)
            {
                yield return Step(tip.text, tip.target(), "SAMAJH GAYA!");
                PlayerPrefs.SetInt(KeyPrefix + tip.key, 1);
            }
            PlayerPrefs.Save();
            End();
        }

        // The power-ups available by this level (all of them for the level 1 lesson).
        private static List<(string key, string text, System.Func<Rect?> target)> PowerUpTips(int levelIndex)
        {
            var tips = new List<(string key, string text, System.Func<Rect?> target)>();
            PUUIController controller = PUController.PowerUpsUIController;
            if (controller == null || controller.UIBehaviors == null)
                return tips;
            foreach (PUUIBehavior pu in controller.UIBehaviors)
            {
                if (pu == null || pu.Settings == null || pu.Settings.RequiredLevel > levelIndex)
                    continue;
                RectTransform rect = pu.transform as RectTransform;
                string text = pu.Settings is PUUndoSettings ? "UNDO: galti ho gayi? Aakhri dish wapas board par chali jaayegi."
                    : pu.Settings is PUHintSettings ? "HINT: phas gaye? Yeh sahi dishes khud tray tak bhej deta hai."
                    : pu.Settings is PUShuffleSettings ? "SHUFFLE: board ki dishes mix ho jaati hain, naye raste khul jaate hain."
                    : null;
                if (text != null)
                    tips.Add(("PowerUp_" + pu.Settings.GetType().Name, text, () => UiRect(rect)));
            }
            return tips;
        }

        // ------------------------------------------------------------------ steps

        // An explanation the player reads and confirms; the game waits meanwhile.
        private IEnumerator Step(string text, Rect? target, string button)
        {
            Show(text, target, interactive: false, button: button);
            advance = false;
            while (!advance)
                yield return null;
        }

        private void Show(string text, Rect? target, bool interactive, string button = null)
        {
            message.text = text;
            SetSpot(target);
            float dimAlpha = interactive ? 0.28f : 0.62f;      // the board stays visible while playing
            foreach (Image d in dims)
                d.color = new Color(0f, 0f, 0f, dimAlpha);

            // explanations stop the game; the first step lets the player tap the board
            blocker.gameObject.SetActive(!interactive);
            if (interactive)
                ReleaseInput();
            else
                HoldInput();

            nextButton.gameObject.SetActive(button != null);
            if (button != null)
                nextLabel.text = button;
            hand.gameObject.SetActive(!interactive && target.HasValue);
            PlaceBubble(target);
        }

        // Interactive step: the hand rests on a dish of the tray's colour.
        private void PointAtDish(BaseCharacterBehavior dish)
        {
            Rect? r = dish != null ? WorldRect(dish.gameObject) : null;
            hand.gameObject.SetActive(r.HasValue);
            if (!r.HasValue)
                return;
            Vector2 a = ToLocal(r.Value.min), b = ToLocal(r.Value.max);
            Rect local = Rect.MinMaxRect(a.x, a.y, b.x, b.y);
            hand.rectTransform.anchorMin = hand.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            hand.rectTransform.localEulerAngles = Vector3.zero;
            hand.rectTransform.anchoredPosition = new Vector2(local.center.x + 10f, local.center.y - local.height * 0.1f);
        }

        private void Next()
        {
            advance = true;
            try { AudioController.PlaySound(AudioController.Sounds.buttonSound); } catch { }
        }

        private void Skip()
        {
            PlayerPrefs.SetInt(SkippedKey, 1);
            PlayerPrefs.SetInt(Level1Key, 1);
            PlayerPrefs.Save();
            End();
        }

        // ------------------------------------------------------------------ targets (screen rects)

        private static IEnumerator WaitForTray()
        {
            float t = 0f;
            while (t < 6f)
            {
                BusBehavior tray = EnvironmentBehavior.CollectingBus;
                if (tray != null && tray.IsAvailableToEnter)
                    yield break;
                t += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        private static IEnumerator Wait(float seconds)
        {
            for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime)
                yield return null;
        }

        private static int OrdersDone()
        {
            int done = 0;
            if (LevelController.OrderTracker == null)
                return 0;
            foreach (var order in LevelController.OrderTracker.GetOrders().Values)
                done += order.completedAmount;
            return done;
        }

        private static Rect? Tray()
        {
            BusBehavior tray = EnvironmentBehavior.CollectingBus;
            return tray != null ? WorldRect(tray.gameObject) : null;
        }

        private static Rect? Dock()
        {
            IReadOnlyList<SlotBehavior> slots = DockBehavior.Slots;
            if (slots == null || slots.Count == 0)
                return null;
            Rect? all = null;
            for (int i = 0; i < slots.Count && i < 7; i++)
            {
                if (slots[i] == null || slots[i].BackImage == null)
                    continue;
                Rect? r = BoundsRect(slots[i].BackImage.bounds);
                if (r.HasValue)
                    all = all.HasValue ? Union(all.Value, r.Value) : r;
            }
            return all;
        }

        private static Rect? Orders()
        {
            UIOrderPanel panel = FindFirstObjectByType<UIOrderPanel>();
            if (panel == null)
                return null;
            Transform frame = panel.transform.parent != null && panel.transform.parent.name.Contains("Orders") ? panel.transform.parent : panel.transform;
            return UiRect(frame as RectTransform);
        }

        private static bool HasHiddenDishes()
        {
            LevelData data = LevelController.LoadedStageData;
            if (data == null || data.ElementsData == null)
                return false;
            foreach (ElementData e in data.ElementsData)
                if (e.SpecialEffectType == LevelData.SpecialEffectType.Hidden)
                    return true;
            return false;
        }

        private static Rect? HiddenDish()
        {
            foreach (ElementSpecialEffect effect in FindObjectsByType<ElementSpecialEffect>(FindObjectsSortMode.None))
            {
                if (effect != null && effect.isActiveAndEnabled && effect.EffectType == LevelData.SpecialEffectType.Hidden)
                    return WorldRect(effect.transform.root == effect.transform ? effect.gameObject : effect.transform.parent.gameObject);
            }
            return null;
        }

        private static Rect? Spawner()
        {
            SpawnerElementBehavior spawner = FindFirstObjectByType<SpawnerElementBehavior>();
            return spawner != null ? WorldRect(spawner.gameObject) : null;
        }

        private static Rect? WorldRect(GameObject go)
        {
            Bounds? bounds = null;
            foreach (Renderer r in go.GetComponentsInChildren<Renderer>())
            {
                if (!r.enabled || r is ParticleSystemRenderer)
                    continue;
                if (bounds.HasValue) { Bounds b = bounds.Value; b.Encapsulate(r.bounds); bounds = b; }
                else bounds = r.bounds;
            }
            return bounds.HasValue ? BoundsRect(bounds.Value) : null;
        }

        private static Rect? BoundsRect(Bounds b)
        {
            Camera cam = Camera.main;
            if (cam == null)
                return null;
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue), max = new Vector2(float.MinValue, float.MinValue);
            for (int i = 0; i < 8; i++)
            {
                Vector3 corner = b.center + Vector3.Scale(b.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                Vector3 s = cam.WorldToScreenPoint(corner);
                if (s.z < 0f)
                    continue;
                min = Vector2.Min(min, s);
                max = Vector2.Max(max, s);
            }
            return min.x <= max.x ? Rect.MinMaxRect(min.x, min.y, max.x, max.y) : (Rect?)null;
        }

        private static Rect? UiRect(RectTransform rect)
        {
            if (rect == null || !rect.gameObject.activeInHierarchy)
                return null;
            Canvas canvas = rect.GetComponentInParent<Canvas>();
            Camera cam = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.rootCanvas.worldCamera : null;
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            Vector2 a = RectTransformUtility.WorldToScreenPoint(cam, corners[0]);
            Vector2 c = RectTransformUtility.WorldToScreenPoint(cam, corners[2]);
            return Rect.MinMaxRect(Mathf.Min(a.x, c.x), Mathf.Min(a.y, c.y), Mathf.Max(a.x, c.x), Mathf.Max(a.y, c.y));
        }

        private static Rect Union(Rect a, Rect b) => Rect.MinMaxRect(Mathf.Min(a.xMin, b.xMin), Mathf.Min(a.yMin, b.yMin), Mathf.Max(a.xMax, b.xMax), Mathf.Max(a.yMax, b.yMax));

        // ------------------------------------------------------------------ drawing

        private void Build()
        {
            art = Resources.Load<MenuPanelArt>(MenuPanelArt.ResourcePath);

            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 90;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();
            root = (RectTransform)transform;

            for (int i = 0; i < 4; i++)
                dims[i] = Img("Dim " + i, root, null, new Color(0f, 0f, 0f, 0.6f));

            // invisible full-screen catcher: while an explanation shows, taps don't reach the game
            Image catcher = Img("Blocker", root, null, new Color(0f, 0f, 0f, 0f));
            Stretch(catcher.rectTransform);
            catcher.raycastTarget = true;
            blocker = catcher.gameObject.AddComponent<Button>();
            blocker.transition = Selectable.Transition.None;

            frame = Img("Spotlight Frame", root, MenuUI.Rounded(26, 6, new Color(0f, 0f, 0f, 0f), new Color32(255, 214, 70, 255)), Color.white);
            frame.type = Image.Type.Sliced;

            hand = Img("Hand", root, art != null ? art.handPointer : null, Color.white);
            hand.preserveAspect = true;
            hand.rectTransform.pivot = new Vector2(0.371f, 0.983f);
            hand.rectTransform.sizeDelta = new Vector2(150f, 150f);

            bubble = Img("Bubble", root, art != null ? art.speechBubble : null, art != null && art.speechBubble != null ? Color.white : new Color32(255, 248, 230, 255));
            bubble.rectTransform.sizeDelta = new Vector2(1000f, 333f);
            // the bubble picture's cream area: 14-94% across, 25-66% up (the chef hat sits above)
            message = Txt("Message", bubble.transform, "", 40f);
            message.rectTransform.anchorMin = new Vector2(0.115f, 0.265f);
            message.rectTransform.anchorMax = new Vector2(0.925f, 0.645f);
            message.rectTransform.offsetMin = message.rectTransform.offsetMax = Vector2.zero;

            chef = Img("Chef", bubble.transform, ChefProgress.Avatar(art), Color.white);
            chef.preserveAspect = true;
            chef.rectTransform.anchorMin = chef.rectTransform.anchorMax = new Vector2(0f, 0f);
            chef.rectTransform.anchoredPosition = new Vector2(40f, -70f);
            chef.rectTransform.sizeDelta = new Vector2(150f, 170f);

            // AAGE: the game's green gold-trimmed button with an outlined label (like CONTINUE)
            Image nextImage = Img("Next", bubble.transform, art != null ? art.tutorialButton : null, Color.white);
            nextImage.preserveAspect = true;
            nextImage.raycastTarget = true;
            nextButton = nextImage.gameObject.AddComponent<Button>();
            nextButton.targetGraphic = nextImage;
            nextButton.onClick.AddListener(Next);
            var nr = nextImage.rectTransform;
            nr.anchorMin = nr.anchorMax = new Vector2(1f, 0f);
            nr.pivot = new Vector2(1f, 1f);
            nr.anchoredPosition = new Vector2(-40f, 6f);
            nr.sizeDelta = new Vector2(330f, 110f);
            nextLabel = Txt("Label", nextImage.transform, "AAGE", 46f);
            nextLabel.color = Color.white;
            OutlinedText.Apply(nextLabel, new Color32(18, 80, 10, 255), 0.3f);
            Stretch(nextLabel.rectTransform);
            nextLabel.rectTransform.offsetMin = new Vector2(30f, 18f);
            nextLabel.rectTransform.offsetMax = new Vector2(-30f, -16f);

            // SKIP: just outlined text under the bubble, next to the chef (no button shape)
            TextMeshProUGUI skipText = Txt("Skip", bubble.transform, "SKIP TUTORIAL >>", 30f);
            skipText.color = Color.white;
            skipText.textWrappingMode = TextWrappingModes.NoWrap;
            OutlinedText.Apply(skipText, new Color32(40, 24, 10, 255), 0.3f);
            skipText.raycastTarget = true;
            skipButton = skipText.gameObject.AddComponent<Button>();
            skipButton.transition = Selectable.Transition.None;
            skipButton.onClick.AddListener(Skip);
            var sr = skipText.rectTransform;
            sr.anchorMin = sr.anchorMax = new Vector2(0f, 0f);
            sr.pivot = new Vector2(0f, 1f);
            sr.anchoredPosition = new Vector2(130f, -18f);
            sr.sizeDelta = new Vector2(300f, 60f);
            skipText.alignment = TextAlignmentOptions.Left;
        }

        private void SetSpot(Rect? target)
        {
            spot = target;
            Vector2 size = root.rect.size;
            Rect r;
            if (target.HasValue)
            {
                Vector2 a = ToLocal(target.Value.min), b = ToLocal(target.Value.max);
                r = Rect.MinMaxRect(a.x - 18f, a.y - 18f, b.x + 18f, b.y + 18f);
            }
            else
            {
                r = new Rect(0f, 0f, 0f, 0f);
            }

            // four dark panels around the spotlight (the whole screen when there is none)
            float l = -size.x * 0.5f, rgt = size.x * 0.5f, btm = -size.y * 0.5f, top = size.y * 0.5f;
            if (!target.HasValue)
            {
                Place(dims[0], l, btm, rgt, top);
                for (int i = 1; i < 4; i++) Place(dims[i], 0f, 0f, 0f, 0f);
                frame.gameObject.SetActive(false);
                return;
            }
            Place(dims[0], l, r.yMax, rgt, top);              // above
            Place(dims[1], l, btm, rgt, r.yMin);              // below
            Place(dims[2], l, r.yMin, r.xMin, r.yMax);        // left
            Place(dims[3], r.xMax, r.yMin, rgt, r.yMax);      // right

            frame.gameObject.SetActive(true);
            frame.rectTransform.anchorMin = frame.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            frame.rectTransform.anchoredPosition = r.center;
            frame.rectTransform.sizeDelta = r.size + new Vector2(8f, 8f);

            // The hand's pivot is its fingertip. Below the middle of the screen (power-up
            // buttons, the tray row) it hangs down from above the target, so it never goes off
            // the bottom of the screen; higher up it points up from below.
            hand.rectTransform.anchorMin = hand.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            bool fromAbove = r.center.y < 0f;
            hand.rectTransform.localEulerAngles = new Vector3(0f, 0f, fromAbove ? 180f : 0f);
            hand.rectTransform.anchoredPosition = fromAbove
                ? new Vector2(r.center.x - 10f, r.yMax - 14f)
                : new Vector2(r.center.x + 20f, r.yMin + 14f);
        }

        // The bubble goes in the half of the screen away from the spotlight.
        private void PlaceBubble(Rect? target)
        {
            float y = -420f;
            if (target.HasValue)
            {
                Vector2 c = ToLocal(target.Value.center);
                y = c.y > 0f ? -330f : 330f;
                if (spot.HasValue && c.y < -200f)
                    y = 380f;
            }
            bubble.rectTransform.anchorMin = bubble.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            bubble.rectTransform.anchoredPosition = new Vector2(30f, y);
        }

        private Vector2 ToLocal(Vector2 screen)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(root, screen, null, out Vector2 local);
            return local;
        }

        private float pulse;

        private void Update()
        {
            pulse += Time.unscaledDeltaTime;
            if (frame != null && frame.gameObject.activeSelf)
                frame.rectTransform.localScale = Vector3.one * (1f + 0.025f * Mathf.Sin(pulse * 5f));
            if (hand != null && hand.gameObject.activeSelf)
                hand.rectTransform.localScale = Vector3.one * (1f - 0.08f * Mathf.Abs(Mathf.Sin(pulse * 3.2f)));
        }


        private static void Place(Image image, float xMin, float yMin, float xMax, float yMax)
        {
            RectTransform r = image.rectTransform;
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
            r.pivot = new Vector2(0.5f, 0.5f);
            r.anchoredPosition = new Vector2((xMin + xMax) * 0.5f, (yMin + yMax) * 0.5f);
            r.sizeDelta = new Vector2(Mathf.Max(0f, xMax - xMin), Mathf.Max(0f, yMax - yMin));
        }

        private static void Stretch(RectTransform r)
        {
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = r.offsetMax = Vector2.zero;
        }

        private static Image Img(string name, Transform parent, Sprite sprite, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            Image image = go.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private TextMeshProUGUI Txt(string name, Transform parent, string value, float size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<TextMeshProUGUI>();
            if (art != null && art.font != null)
                t.font = art.font;
            t.text = value;
            t.fontSize = size;
            t.enableAutoSizing = true;
            t.fontSizeMin = 22f;
            t.fontSizeMax = size;
            t.color = new Color32(92, 52, 24, 255);
            t.alignment = TextAlignmentOptions.Center;
            t.textWrappingMode = TextWrappingModes.Normal;
            t.raycastTarget = false;
            return t;
        }
    }
}
