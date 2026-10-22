using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Watermelon
{
    /// <summary>
    /// OUT OF LIVES: shown instead of a level when there are no lives (PLAY on the maps, or the
    /// last life lost in a level). The time to the next free life, and two ways to play now:
    /// one life for coins, or all lives for diamonds. Its own art (Resources/UI/Lives) on its
    /// own overlay canvas, in any scene.
    /// </summary>
    public sealed class OutOfLivesPopup : MonoBehaviour
    {
        public const int LifeCoinPrice = 250;
        public const int RefillDiamondPrice = 25;

        private const int SortingOrder = 4300;          // above the scene UI, menu panels and power-up offer
        private const float FrameWidth = PopupArt.PanelWidth;
        private const float FrameHeight = PopupArt.PanelHeight;
        private const float FrameSliceScale = 1.25f;
        private const float OfferWidth = 580f;
        private const float OfferHeight = OfferWidth * 320f / 1024f;   // the button pictures are 1024x320
        private const float OfferGap = 18f;
        private const float MessageTime = 2.2f;

        private static OutOfLivesPopup instance;

        public static bool IsOpen => instance != null && instance.root != null && instance.root.gameObject.activeSelf;

        private MenuPanelArt art;
        private Sprite heart;
        private RectTransform root;
        private RectTransform frame;
        private CanvasGroup group;
        private TextMeshProUGUI timerText;
        private TextMeshProUGUI messageText;
        private Button coinsButton, refillButton;
        private RectTransform heartRect;
        private Image heartGlow;
        private float messageUntil;

        private Action onLifeGained;
        private Action onClosed;

        /// <summary>
        /// Opens the popup. <paramref name="lifeGained"/> runs once there is a life again (bought,
        /// or given back by the timer); <paramref name="closed"/> when the player
        /// closes it without one.
        /// </summary>
        public static void Show(Action lifeGained, Action closed)
        {
            if (instance == null)
            {
                GameObject go = new GameObject("[OUT OF LIVES]");
                DontDestroyOnLoad(go);
                instance = go.AddComponent<OutOfLivesPopup>();
                instance.Build();
            }

            instance.Open(lifeGained, closed);
        }

        private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;

        private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

        private void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        // a new scene: the popup belonged to the last one
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Hide();

        #region Open / close

        private void Open(Action lifeGained, Action closed)
        {
            onLifeGained = lifeGained;
            onClosed = closed;
            messageText.text = string.Empty;

            Refresh();
            root.gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(PopIn());
        }

        private void Hide()
        {
            onLifeGained = null;
            onClosed = null;
            if (root != null)
                root.gameObject.SetActive(false);
        }

        private void CloseButton()
        {
            MenuUI.PlayClick();
            Action closed = onClosed;
            Hide();
            closed?.Invoke();
        }

        private void LifeGained()
        {
            SaveNow();
            Action gained = onLifeGained;
            Hide();
            gained?.Invoke();
        }

        private System.Collections.IEnumerator PopIn()
        {
            const float duration = 0.22f;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / duration);
                float back = 1f + 2.2f * Mathf.Pow(k - 1f, 3f) + 1.2f * Mathf.Pow(k - 1f, 2f);   // ease-out-back
                frame.localScale = Vector3.one * Mathf.LerpUnclamped(0.86f, 1f, back);
                group.alpha = Mathf.Clamp01(k * 2f);
                yield return null;
            }
            frame.localScale = Vector3.one;
            group.alpha = 1f;
        }

        private void Update()
        {
            if (!IsOpen)
                return;

            // the timer gave a life back while the popup was open
            if (LivesManager.CurrentLives > 0)
            {
                LifeGained();
                return;
            }

            Refresh();
            Heartbeat();

            if (messageUntil > 0f && Time.unscaledTime >= messageUntil)
            {
                messageUntil = 0f;
                messageText.text = string.Empty;
            }
        }

        private void Refresh()
        {
            TimeSpan left = LivesManager.TimeToNextLife;
            string time = left >= TimeSpan.FromHours(1)
                ? string.Format("{0}:{1:00}:{2:00}", (int)left.TotalHours, left.Minutes, left.Seconds)
                : string.Format("{0:00}:{1:00}", (int)left.TotalMinutes, left.Seconds);
            timerText.text = Loc.F("Next life in {0}", time);

            SetUsable(coinsButton, HasAmount(CurrencyType.Coins, LifeCoinPrice));
            SetUsable(refillButton, HasAmount(CurrencyType.Diamonds, RefillDiamondPrice));
        }

        // A double beat about once a second; the glow swells with it.
        private void Heartbeat()
        {
            float t = Time.unscaledTime % 1.2f;
            float beat = Mathf.Max(Pulse(t, 0.08f), 0.6f * Pulse(t, 0.3f));
            heartRect.localScale = Vector3.one * (1f + 0.08f * beat);
            heartGlow.rectTransform.localScale = Vector3.one * (1f + 0.12f * beat);
            Color glow = heartGlow.color;
            glow.a = GlowAlpha * (0.75f + 0.25f * beat);
            heartGlow.color = glow;
        }

        private static float Pulse(float t, float at)
        {
            float d = (t - at) / 0.09f;
            return Mathf.Exp(-d * d);
        }

        private static Sprite glowSprite;

        // A round soft glow, made once in code.
        private static Sprite GlowSprite()
        {
            if (glowSprite != null)
                return glowSprite;

            const int size = 128;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            Color32[] pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x + 0.5f) / size * 2f - 1f, dy = (y + 0.5f) / size * 2f - 1f;
                    float k = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy));
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(255f * Mathf.Pow(k, 1.4f)));
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply();
            glowSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
            return glowSprite;
        }

        // Greyed out when it cannot be used right now; still tappable, to say why.
        private static void SetUsable(Button button, bool usable)
        {
            CanvasGroup canvasGroup = button.GetComponent<CanvasGroup>();
            canvasGroup.alpha = usable ? 1f : 0.5f;
        }

        #endregion

        #region Offers

        private void BuyLife()
        {
            MenuUI.PlayClick();
            if (!Spend(CurrencyType.Coins, LifeCoinPrice))
            {
                Message("Not enough coins");
                return;
            }
            LivesManager.GiveLives(1);
            LifeGained();
        }

        private void Refill()
        {
            MenuUI.PlayClick();
            if (!Spend(CurrencyType.Diamonds, RefillDiamondPrice))
            {
                Message("Not enough diamonds");
                return;
            }
            LivesManager.RefillLives();
            LifeGained();
        }

        private void Message(string key)
        {
            messageText.text = Loc.T(key);
            messageUntil = Time.unscaledTime + MessageTime;
        }

        private static bool HasAmount(CurrencyType currency, int amount)
        {
            try { return CurrenciesController.HasAmount(currency, amount); }
            catch { return false; }
        }

        private static bool Spend(CurrencyType currency, int amount)
        {
            try
            {
                if (!CurrenciesController.HasAmount(currency, amount))
                    return false;
                CurrenciesController.Substract(currency, amount);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[OutOfLives] Could not spend currency: " + ex.Message);
                return false;
            }
        }

        private static void SaveNow()
        {
            try
            {
                SaveController.MarkAsSaveIsRequired();
                SaveController.Save(true);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[OutOfLives] Could not save: " + ex.Message);
            }
        }

        #endregion

        #region Build

        private void Build()
        {
            art = Resources.Load<MenuPanelArt>(MenuPanelArt.ResourcePath);
            if (art == null)
                art = ScriptableObject.CreateInstance<MenuPanelArt>();
            if (art.font != null)
                MenuUI.Font = art.font;
            heart = Resources.Load<Sprite>("UI/Lives/heart");

            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = SortingOrder;

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;   // the whole panel always fits
            gameObject.AddComponent<GraphicRaycaster>();

            root = MenuUI.Rect("Out of Lives", transform);
            MenuUI.Stretch(root);
            group = root.gameObject.AddComponent<CanvasGroup>();

            // dim background: blocks the screen behind (taps on it do nothing)
            Image dim = MenuUI.Image("Dim", root, null, new Color(0.03f, 0.05f, 0.1f, 0.8f));
            MenuUI.Stretch(dim.rectTransform);
            dim.raycastTarget = true;

            // Its own art (Resources/UI/Lives): the heart-pattern frame, the painted title on the
            // arch, the X on the corner; the shared popup art when a picture is missing.
            Sprite panelSprite = Load("panel_lives");
            Image frameImage = MenuUI.Image("Frame", root, panelSprite != null ? panelSprite : PopupArt.Panel(art),
                panelSprite == null && art.frame == null ? MenuUI.CardFill : (Color?)null, false);
            if (panelSprite == null)
                frameImage.pixelsPerUnitMultiplier = FrameSliceScale;
            frameImage.raycastTarget = true;
            frame = frameImage.rectTransform;
            MenuUI.Place(frame, new Vector2(0f, -30f), new Vector2(FrameWidth, FrameHeight));

            Sprite titleSprite = Load("title_out_of_lives");
            Image title = MenuUI.Image("Title", frame, titleSprite != null ? titleSprite : PopupArt.TitleRibbon(art), null);
            Vector2 titleSize = new Vector2(PopupArt.TitleWidth, PopupArt.TitleHeight(title.sprite));
            MenuUI.Anchor(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -FrameHeight * TitleFromTop), titleSize);
            if (titleSprite == null)
            {
                TextMeshProUGUI words = MenuUI.OutlinedText("Words", title.transform, Loc.T("OUT OF LIVES"), 52f, PopupArt.RibbonOutline);
                Vector4 inset = PopupArt.RibbonTextInsets;
                MenuUI.Stretch(words.rectTransform, inset.x * titleSize.x, inset.y * titleSize.y, inset.z * titleSize.x, inset.w * titleSize.y);
            }

            Image close = MenuUI.Image("Close Button", frame, art.closeButton, art.closeButton == null ? new Color32(220, 60, 50, 255) : (Color?)null);
            MenuUI.Anchor(close.rectTransform, new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), -PopupArt.CloseFromCorner, new Vector2(PopupArt.CloseSize, PopupArt.CloseSize));
            MenuUI.Button(close, CloseButton);

            // the cream area, top to bottom: broken heart (glowing, beating), a line on what to
            // do, timer pill, message, the two offers
            float y = FrameHeight * CreamTop + 16f;

            // a soft warm glow behind the heart, under the title and the X
            heartGlow = MenuUI.Image("Heart Glow", frame, GlowSprite(), new Color(1f, 0.38f, 0.42f, GlowAlpha), false);
            heartGlow.raycastTarget = false;
            heartGlow.rectTransform.SetAsFirstSibling();
            MenuUI.Anchor(heartGlow.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -(y + HeartSize * 0.5f)),
                new Vector2(HeartSize * 1.6f, HeartSize * 1.6f));

            Image brokenHeart = MenuUI.Image("Broken Heart", frame, Load("heart_broken"), null);
            brokenHeart.raycastTarget = false;
            heartRect = brokenHeart.rectTransform;
            MenuUI.Anchor(heartRect, new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -(y + HeartSize * 0.5f)), new Vector2(HeartSize, HeartSize));
            y += HeartSize + 2f;

            // dark red-brown letters with a white edge: a light face disappears on the cream
            TextMeshProUGUI hint = MenuUI.OutlinedText("Hint", frame, "No lives left! Wait or refill now.", 34f, Color.white,
                TextAlignmentOptions.Center, new Color32(150, 38, 24, 255));
            MenuUI.Anchor(hint.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -y), new Vector2(580f, 44f));
            y += 48f;

            Sprite pillSprite = Load("timer_pill");
            Image pill = MenuUI.Image("Timer Pill", frame, pillSprite, pillSprite == null ? new Color32(70, 36, 16, 255) : (Color?)null, false);
            float pillHeight = pillSprite != null ? TimerWidth / PopupArt.Aspect(pillSprite) : 90f;
            MenuUI.Anchor(pill.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -y), new Vector2(TimerWidth, pillHeight));
            timerText = MenuUI.OutlinedText("Timer", pill.transform, "", 40f, new Color32(60, 26, 6, 255), TextAlignmentOptions.Center,
                new Color32(255, 214, 70, 255));
            timerText.enableAutoSizing = true;
            timerText.fontSizeMax = 40f;
            timerText.fontSizeMin = 18f;
            Stretch(timerText.rectTransform, TimerTextArea);
            y += pillHeight + 2f;

            messageText = MenuUI.OutlinedText("Message", frame, "", 30f, new Color32(120, 20, 20, 255), TextAlignmentOptions.Center,
                new Color32(255, 120, 100, 255));
            MenuUI.Anchor(messageText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -y), new Vector2(560f, 32f));
            y += 32f;

            // the two offers, centred in the space left in the cream area
            y += (FrameHeight * CreamBottom - y - (2f * OfferHeight + OfferGap)) * 0.5f;
            coinsButton = Offer("Coins Button", "btn_buy_life_baked", "btn_life_coins", PopupArt.ButtonOrange(art), "+1", Loc.T("BUY LIFE"),
                art.coin, LifeCoinPrice.ToString("N0"), new Color32(130, 50, 8, 255), BuyLife, y);
            y += OfferHeight + OfferGap;
            refillButton = Offer("Refill Button", "btn_refill_all_baked", "btn_life_refill", PopupArt.ButtonOrange(art), "+" + LivesManager.MaxLivesCount, Loc.T("REFILL ALL"),
                art.diamond, RefillDiamondPrice.ToString(), new Color32(60, 16, 100, 255), Refill, y);

            root.gameObject.SetActive(false);
        }

        // Shares of the pictures in Resources/UI/Lives (measured on the art).
        private const float TitleFromTop = 0.07f;                 // the title's centre on the frame's arch
        private const float CreamTop = 0.184f, CreamBottom = 0.913f;   // the frame's cream area (shares of the height)
        private const float TimerWidth = 560f;
        private const float HeartSize = 186f;
        private const float GlowAlpha = 0.95f;
        private static readonly Vector4 TimerTextArea = new Vector4(0.27f, 0.14f, 0.15f, 0.14f);      // the brown strip (25-87.5% of the width), padded
        private const float SocketX = 0.125f, SocketSize = 0.5f;  // the round socket on the left (share of width / height)
        private const float LabelX = 0.467f, LabelWidth = 0.44f;  // the plain middle of the button
        private const float PriceX = 0.84f, PriceWidth = 0.22f, PriceHeight = 0.34f;   // the dark price slot on the right

        private static Sprite Load(string name) => Resources.Load<Sprite>("UI/Lives/" + name);

        // left, bottom, right, top insets as shares of the parent
        private static void Stretch(RectTransform rect, Vector4 shares)
        {
            rect.anchorMin = new Vector2(shares.x, shares.y);
            rect.anchorMax = new Vector2(1f - shares.z, 1f - shares.w);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        // One offer. A baked picture (heart, words and price painted in) is used as it is;
        // otherwise its painted button gets the heart with the lives it gives in the round
        // socket, what it is in the middle, and the price (icon and amount) in the slot on the right.
        private Button Offer(string name, string bakedName, string pictureName, Sprite fallback, string reward, string label, Sprite priceIcon, string price,
            Color outline, UnityEngine.Events.UnityAction action, float fromTop)
        {
            Sprite baked = Load(bakedName);
            Sprite picture = baked != null ? baked : Load(pictureName);
            Image image = MenuUI.Image(name, frame, picture != null ? picture : fallback, null, false);
            if (picture == null)
                image.pixelsPerUnitMultiplier = 2.2f;
            Button button = MenuUI.Button(image, action);
            button.gameObject.AddComponent<CanvasGroup>();
            MenuUI.Anchor(image.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -fromTop), new Vector2(OfferWidth, OfferHeight));

            if (baked != null)
                return button;

            float socket = OfferHeight * SocketSize;
            Image lifeIcon = MenuUI.Image("Heart", image.transform, heart, heart == null ? new Color32(230, 40, 50, 255) : (Color?)null);
            lifeIcon.raycastTarget = false;
            MenuUI.Anchor(lifeIcon.rectTransform, new Vector2(SocketX, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 2f), new Vector2(socket * 0.92f, socket * 0.92f));
            TextMeshProUGUI rewardText = MenuUI.OutlinedText("Reward", lifeIcon.transform, reward, 34f, new Color32(110, 10, 20, 255));
            MenuUI.Stretch(rewardText.rectTransform);

            TextMeshProUGUI labelText = MenuUI.OutlinedText("Label", image.transform, label, 40f, outline);
            labelText.enableAutoSizing = true;
            labelText.fontSizeMax = 40f;
            labelText.fontSizeMin = 20f;
            MenuUI.Anchor(labelText.rectTransform, new Vector2(LabelX, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 4f),
                new Vector2(OfferWidth * LabelWidth, OfferHeight * 0.4f));

            RectTransform slot = MenuUI.Rect("Price", image.transform);
            MenuUI.Anchor(slot, new Vector2(PriceX, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(OfferWidth * PriceWidth, OfferHeight * PriceHeight));
            HorizontalLayoutGroup row = slot.gameObject.AddComponent<HorizontalLayoutGroup>();
            row.childAlignment = TextAnchor.MiddleCenter;
            row.spacing = 4f;
            row.childControlWidth = row.childControlHeight = true;
            row.childForceExpandWidth = row.childForceExpandHeight = false;

            float iconSize = OfferHeight * PriceHeight * 0.95f;
            if (priceIcon != null)
            {
                Image icon = MenuUI.Image("Icon", slot, priceIcon);
                icon.raycastTarget = false;
                LayoutElement iconLayout = icon.gameObject.AddComponent<LayoutElement>();
                iconLayout.preferredWidth = iconSize;
                iconLayout.preferredHeight = iconSize;
            }

            TextMeshProUGUI priceText = MenuUI.OutlinedText("Amount", slot, price, 30f, outline);
            priceText.enableAutoSizing = true;
            priceText.fontSizeMax = 30f;
            priceText.fontSizeMin = 16f;
            return button;
        }

        #endregion
    }
}
