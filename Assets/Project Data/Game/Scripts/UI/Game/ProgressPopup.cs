using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Watermelon
{
    /// <summary>
    /// The "something new" moments of progress, in the game's panel style (the same frame, title
    /// and buttons as the power-up offer): NEW POWER-UP! when a power-up unlocks, and LEVEL UP! when
    /// the chef reaches a new level (with its coins and, at some levels, a new outfit to wear).
    /// One at a time: more are queued and shown one after another.
    /// Baked titles are used when present (Resources/UI/Panels/title_new_powerup.png,
    /// title_level_up.png); until then the blue ribbon with the words written by the game.
    /// </summary>
    public sealed class ProgressPopup : MonoBehaviour
    {
        private const int SortingOrder = 4300;          // above the scene UI, menu panels and the offer
        // the shared popup layout (as MENU): frame, title on the arch, content in the cream area
        private const float FrameWidth = PopupArt.PanelWidth;
        private const float FrameHeight = PopupArt.PanelHeight;
        private const float FrameSliceScale = 1.25f;
        private const float CardTop = FrameHeight * PopupArt.CreamTop + 14f;            // from the frame's top
        private const float ButtonsBottom = FrameHeight * (1f - PopupArt.CreamBottom);  // from the frame's bottom

        private sealed class Entry
        {
            public string titleKey, title, name, text, primary, secondary;
            public Sprite picture;
            public Vector2 pictureSize = new Vector2(240f, 240f);
            public System.Action onPrimary;
            public System.Action onClosed;     // either button
        }

        private static ProgressPopup instance;
        private static readonly Queue<Entry> queue = new Queue<Entry>();

        public static bool IsOpen => instance != null && instance.root != null && instance.root.gameObject.activeSelf;

        private MenuPanelArt art;
        private RectTransform root, frame;
        private CanvasGroup group;
        private Image ribbon, picture;
        private TextMeshProUGUI titleText, nameText, bodyText;
        private Button primaryButton, secondaryButton;
        private TextMeshProUGUI primaryLabel, secondaryLabel;
        private Entry current;

        #region Moments

        /// <summary>A power-up becomes available: its picture, what it does and how many are free.</summary>
        public static void ShowPowerUp(PUSettings settings, Sprite buttonPicture)
        {
            if (settings == null)
                return;

            int owned = 0;
            try { owned = Mathf.Max(0, settings.Save.Amount); } catch { }
            Enqueue(new Entry
            {
                titleKey = "new_powerup",
                title = "NEW POWER-UP!",
                picture = buttonPicture != null ? buttonPicture : settings.Icon,
                name = Loc.T(PowerUpName(settings)),
                text = Loc.T(settings.Description) + (owned > 0 ? "\n" + Loc.F("{0} free to start!", owned) : string.Empty),
                primary = "GOT IT",
            });
        }

        /// <summary>The chef reached a new level: the coins earned and any outfit it unlocked.</summary>
        public static void ShowChefLevel(ChefProgress.Gain gain, System.Action onClosed = null)
        {
            if (!gain.LevelUp)
            {
                onClosed?.Invoke();
                return;
            }

            ChefProgress.Outfit outfit = null;
            foreach (ChefProgress.Outfit o in ChefProgress.Outfits)
            {
                if (o.UnlockLevel > gain.OldLevel && o.UnlockLevel <= gain.NewLevel && ChefProgress.HasArt(o))
                    outfit = o;
            }

            MenuPanelArt menuArt = Resources.Load<MenuPanelArt>(MenuPanelArt.ResourcePath);
            Sprite full = outfit != null ? ChefProgress.LoadFull(outfit) : null;
            string text = gain.Coins > 0 ? Loc.F("+{0} coins", gain.Coins.ToString("N0")) : string.Empty;
            if (outfit != null)
                text += (text.Length > 0 ? "\n" : string.Empty) + Loc.F("New outfit: {0}", Loc.T(outfit.Name));

            Enqueue(new Entry
            {
                titleKey = "level_up",
                title = "LEVEL UP!",
                picture = full != null ? full : ChefProgress.Avatar(menuArt),
                pictureSize = full != null ? new Vector2(260f, 260f) : new Vector2(230f, 230f),
                name = Loc.F("CHEF LEVEL {0}", gain.NewLevel),
                text = text,
                // wearing the new outfit is the main action; LATER keeps the current one
                primary = outfit != null ? "WEAR IT" : "GREAT!",
                secondary = outfit != null ? "LATER" : null,
                onPrimary = outfit != null ? () => ChefProgress.Select(outfit) : (System.Action)null,
                onClosed = onClosed,
            });
        }

        public static string PowerUpName(PUSettings powerUp)
        {
            if (powerUp is PUUndoSettings) return "UNDO";
            if (powerUp is PUHintSettings) return "HINT";
            if (powerUp is PUShuffleSettings) return "SHUFFLE";
            if (powerUp is PUExtraSlotSettings) return "EXTRA PLATE";
            return "POWER-UP";
        }

        #endregion

        #region Queue

        private static void Enqueue(Entry entry)
        {
            queue.Enqueue(entry);
            if (instance == null)
            {
                GameObject go = new GameObject("[PROGRESS POPUP]");
                DontDestroyOnLoad(go);
                instance = go.AddComponent<ProgressPopup>();
                instance.Build();
            }
            if (!IsOpen)
                instance.ShowNext();
        }

        private void ShowNext()
        {
            if (queue.Count == 0)
            {
                current = null;
                root.gameObject.SetActive(false);
                return;
            }

            current = queue.Dequeue();
            Apply(current);
            root.gameObject.SetActive(true);
            try { AudioController.PlaySound(AudioController.Sounds.completeSound); } catch { }
            StopAllCoroutines();
            StartCoroutine(PopIn());
        }

        private void Finish(bool primary)
        {
            MenuUI.PlayClick();
            Entry done = current;
            if (primary)
                done?.onPrimary?.Invoke();
            try { done?.onClosed?.Invoke(); }
            catch (System.Exception ex) { Debug.LogWarning("[ProgressPopup] " + ex.Message); }
            ShowNext();
        }

        private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;

        private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

        private void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        // a level up shown on the win screen stays up over the next scene until it is answered;
        // nothing is lost, it is only closed when the queue is empty
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) { }

        #endregion

        #region Build

        private void Apply(Entry e)
        {
            Sprite baked = PopupArt.BakedTitle(e.titleKey);
            if (baked != null)
            {
                ribbon.sprite = baked;
                ribbon.rectTransform.sizeDelta = new Vector2(PopupArt.TitleWidth, PopupArt.TitleHeight(baked));
                titleText.gameObject.SetActive(false);
            }
            else
            {
                ribbon.sprite = PopupArt.TitleRibbon(art);
                ribbon.rectTransform.sizeDelta = new Vector2(PopupArt.TitleWidth, PopupArt.TitleHeight(ribbon.sprite));
                titleText.gameObject.SetActive(true);
                titleText.text = Loc.T(e.title);
            }

            picture.sprite = e.picture;
            picture.gameObject.SetActive(e.picture != null);
            picture.rectTransform.sizeDelta = e.pictureSize;
            nameText.text = e.name;
            bodyText.text = e.text;

            primaryLabel.text = Loc.T(e.primary);
            bool two = !string.IsNullOrEmpty(e.secondary);
            secondaryButton.gameObject.SetActive(two);
            if (two)
                secondaryLabel.text = Loc.T(e.secondary);
            // two buttons stack on the cream area's bottom; a single one sits centred under the card
            ((RectTransform)primaryButton.transform).anchoredPosition = new Vector2(0f, ButtonsBottom + (two ? 190f : 120f));
        }

        private System.Collections.IEnumerator PopIn()
        {
            const float duration = 0.25f;
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                float k = Mathf.Clamp01(t / duration);
                float back = 1f + 2.2f * Mathf.Pow(k - 1f, 3f) + 1.2f * Mathf.Pow(k - 1f, 2f);   // ease-out-back
                frame.localScale = Vector3.one * Mathf.LerpUnclamped(0.8f, 1f, back);
                group.alpha = Mathf.Clamp01(k * 2f);
                yield return null;
            }
            frame.localScale = Vector3.one;
            group.alpha = 1f;
        }

        private void Build()
        {
            art = Resources.Load<MenuPanelArt>(MenuPanelArt.ResourcePath);
            if (art == null)
                art = ScriptableObject.CreateInstance<MenuPanelArt>();
            if (art.font != null)
                MenuUI.Font = art.font;

            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = SortingOrder;
            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            gameObject.AddComponent<GraphicRaycaster>();

            root = MenuUI.Rect("Progress", transform);
            MenuUI.Stretch(root);
            group = root.gameObject.AddComponent<CanvasGroup>();

            // dim: blocks everything behind; a moment is closed with its button
            Image dim = MenuUI.Image("Dim", root, null, new Color(0.03f, 0.05f, 0.1f, 0.78f));
            MenuUI.Stretch(dim.rectTransform);
            dim.raycastTarget = true;

            Image frameImage = MenuUI.Image("Frame", root, PopupArt.Panel(art), art.frame == null ? MenuUI.CardFill : (Color?)null);
            frameImage.pixelsPerUnitMultiplier = FrameSliceScale;
            frameImage.raycastTarget = true;
            frame = frameImage.rectTransform;
            MenuUI.Place(frame, new Vector2(0f, -30f), new Vector2(FrameWidth, FrameHeight));

            Sprite ribbonSprite = PopupArt.TitleRibbon(art);
            Vector2 ribbonSize = new Vector2(PopupArt.TitleWidth, PopupArt.TitleHeight(ribbonSprite));
            ribbon = MenuUI.Image("Title", frame, ribbonSprite);
            MenuUI.Anchor(ribbon.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -FrameHeight * PopupArt.TitleFromTop), ribbonSize);
            titleText = MenuUI.OutlinedText("Words", ribbon.transform, "", 52f, PopupArt.RibbonOutline);
            Vector4 inset = PopupArt.RibbonTextInsets;
            MenuUI.Stretch(titleText.rectTransform, inset.x * ribbonSize.x, inset.y * ribbonSize.y, inset.z * ribbonSize.x, inset.w * ribbonSize.y);

            // the picture, name and text on a card in the cream area
            Image card = MenuUI.Image("Card", frame, MenuUI.Rounded(30, 4, MenuUI.CardFill, MenuUI.CardBorder), null, false);
            MenuUI.Anchor(card.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -CardTop), new Vector2(560f, 470f));

            picture = MenuUI.Image("Picture", card.transform, null);
            MenuUI.Anchor(picture.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -16f), new Vector2(240f, 240f));

            nameText = MenuUI.OutlinedText("Name", card.transform, "", 54f, new Color32(110, 52, 10, 255), TextAlignmentOptions.Center,
                new Color32(255, 200, 45, 255));
            MenuUI.Anchor(nameText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -268f), new Vector2(520f, 60f));

            bodyText = MenuUI.Text("Text", card.transform, "", 38f, MenuUI.TextDark, TextAlignmentOptions.Center, true);
            MenuUI.Anchor(bodyText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -336f), new Vector2(520f, 120f));

            primaryButton = MenuUI.LabelButton("Main Button", frame, PopupArt.ButtonOrange(art), "OK", 50f, () => Finish(true), null, new Color32(140, 60, 10, 255));
            MenuUI.Anchor((RectTransform)primaryButton.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), new Vector2(0f, ButtonsBottom + 190f), new Vector2(420f, 133f));
            primaryLabel = primaryButton.GetComponentInChildren<TextMeshProUGUI>(true);

            secondaryButton = MenuUI.LabelButton("Second Button", frame, PopupArt.ButtonGreen(art), "LATER", 40f, () => Finish(false), null, new Color32(30, 90, 20, 255));
            MenuUI.Anchor((RectTransform)secondaryButton.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), new Vector2(0f, ButtonsBottom + 64f), new Vector2(300f, 95f));
            secondaryLabel = secondaryButton.GetComponentInChildren<TextMeshProUGUI>(true);

            root.gameObject.SetActive(false);
        }

        #endregion
    }

    /// <summary>
    /// Which power-ups the player has unlocked, kept once reached so that replaying an early level
    /// does not take them away again.
    /// </summary>
    public static class PowerUpUnlocks
    {
        private const string Prefix = "CC_PU_Unlocked_";

        public static bool IsUnlocked(PUSettings settings) =>
            settings != null && PlayerPrefs.GetInt(Prefix + settings.Type, 0) == 1;

        /// <summary>
        /// Available while playing <paramref name="levelIndex"/>: that level is at or past its unlock
        /// level, it was unlocked before, or the player's progress has reached its unlock level (so
        /// replaying an early level keeps everything already reached).
        /// </summary>
        public static bool IsAvailable(PUSettings settings, int levelIndex)
        {
            if (settings == null)
                return false;
            int required = settings.RequiredLevel;
            if (required <= levelIndex || IsUnlocked(settings))
                return true;
            try
            {
                return BusStop.GameProgress.IsLevelCompleted(required - 1) || BusStop.GameProgress.IsLevelCompleted(required);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>Remembers the unlock; true the first time.</summary>
        public static bool MarkUnlocked(PUSettings settings)
        {
            if (settings == null || IsUnlocked(settings))
                return false;
            PlayerPrefs.SetInt(Prefix + settings.Type, 1);
            PlayerPrefs.Save();
            return true;
        }
    }
}
