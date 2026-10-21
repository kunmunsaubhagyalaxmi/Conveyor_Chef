using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Watermelon
{
    /// <summary>
    /// "Out of power-ups" offer, shown when a power-up button with none left is tapped. Built from
    /// the game's own panel art (Resources/MenuPanelArt: frame, title ribbon, close and buttons),
    /// like the Shop, instead of the template purchase panel, whose own coin counter sat on the
    /// ORDERS panel. On its own overlay canvas, sized for the design phone (tablets included).
    /// </summary>
    public sealed class PowerUpOfferPopup : MonoBehaviour
    {
        private const int SortingOrder = 4200;          // above the scene UI and the menu panels
        private const float FrameWidth = 800f;
        private const float FrameHeight = 1060f;
        private const float FrameSliceScale = 1.25f;    // as the menu panels

        private static PowerUpOfferPopup instance;

        public static bool IsOpen => instance != null && instance.root != null && instance.root.gameObject.activeSelf;

        private MenuPanelArt art;
        private RectTransform root;
        private RectTransform frame;
        private CanvasGroup group;
        private Image icon;
        private TextMeshProUGUI amountText;
        private TextMeshProUGUI nameText;
        private TextMeshProUGUI descriptionText;
        private TextMeshProUGUI priceText;
        private Image priceIcon;
        private TextMeshProUGUI messageText;
        private PUSettings settings;

        /// <summary>Shows the offer for <paramref name="powerUp"/>; <paramref name="picture"/> is its button art.</summary>
        public static void Show(PUSettings powerUp, Sprite picture)
        {
            if (powerUp == null)
                return;

            if (instance == null)
            {
                GameObject go = new GameObject("[POWER-UP OFFER]");
                DontDestroyOnLoad(go);
                instance = go.AddComponent<PowerUpOfferPopup>();
                instance.Build();
            }

            instance.Open(powerUp, picture);
        }

        public static void Hide()
        {
            if (instance != null)
                instance.Close();
        }

        private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;

        private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

        private void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Close();

        #region Open / close

        private void Open(PUSettings powerUp, Sprite picture)
        {
            settings = powerUp;

            icon.sprite = picture != null ? picture : powerUp.Icon;
            amountText.text = "x" + powerUp.PurchaseAmount;
            nameText.text = Loc.T(NameOf(powerUp));
            descriptionText.text = Loc.T(powerUp.Description);
            priceText.text = powerUp.Price.ToString("N0");
            priceIcon.sprite = powerUp.CurrencyType == CurrencyType.Diamonds ? art.diamond : art.coin;
            messageText.text = string.Empty;

            root.gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(PopIn());
        }

        private void Close()
        {
            if (root != null)
                root.gameObject.SetActive(false);
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

        #endregion

        #region Actions

        private void Buy()
        {
            if (settings == null)
                return;

            if (!settings.HasEnoughCurrency())
            {
                messageText.text = Loc.T(settings.CurrencyType == CurrencyType.Diamonds ? "Not enough diamonds" : "Not enough coins");
                return;
            }

            if (PUController.PurchasePowerUp(settings.Type))
            {
                try { AudioController.PlaySound(AudioController.Sounds.buttonSound); } catch { }
                Close();
            }
        }

        private void OpenShop()
        {
            Close();
            MenuPanel.Show<ShopPanel>();
        }

        private static string NameOf(PUSettings powerUp)
        {
            if (powerUp is PUUndoSettings) return "UNDO";
            if (powerUp is PUHintSettings) return "HINT";
            if (powerUp is PUShuffleSettings) return "SHUFFLE";
            if (powerUp is PUExtraSlotSettings) return "EXTRA PLATE";
            return "POWER-UP";
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

            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = SortingOrder;

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;   // the whole panel always fits
            gameObject.AddComponent<GraphicRaycaster>();

            root = MenuUI.Rect("Power-up Offer", transform);
            MenuUI.Stretch(root);
            group = root.gameObject.AddComponent<CanvasGroup>();

            // dim background: blocks the board, tapping it closes the offer
            Image dim = MenuUI.Image("Dim", root, null, new Color(0.03f, 0.05f, 0.1f, 0.75f));
            MenuUI.Stretch(dim.rectTransform);
            MenuUI.Button(dim, Close, false).transition = Selectable.Transition.None;

            Image frameImage = MenuUI.Image("Frame", root, art.frame, art.frame == null ? MenuUI.CardFill : (Color?)null, false);
            frameImage.pixelsPerUnitMultiplier = FrameSliceScale;
            frameImage.raycastTarget = true;     // taps on the panel do not close it
            frame = frameImage.rectTransform;
            MenuUI.Place(frame, new Vector2(0f, -30f), new Vector2(FrameWidth, FrameHeight));

            // blue title ribbon across the top edge (matches the blue and gold frame)
            Sprite ribbonSprite = PopupArt.TitleRibbon(art);
            Image ribbon = MenuUI.Image("Title Ribbon", frame, ribbonSprite, ribbonSprite == null ? new Color32(30, 100, 220, 255) : (Color?)null);
            Vector2 ribbonSize = new Vector2(600f, 200f);
            MenuUI.Anchor(ribbon.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -40f), ribbonSize);
            TextMeshProUGUI title = MenuUI.OutlinedText("Title", ribbon.transform, "OUT OF POWER-UPS", 52f, PopupArt.RibbonOutline);
            Vector4 inset = PopupArt.RibbonTextInsets;
            MenuUI.Stretch(title.rectTransform, inset.x * ribbonSize.x, inset.y * ribbonSize.y, inset.z * ribbonSize.x, inset.w * ribbonSize.y);

            Image close = MenuUI.Image("Close Button", frame, art.closeButton, art.closeButton == null ? new Color32(220, 60, 50, 255) : (Color?)null);
            MenuUI.Anchor(close.rectTransform, new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(-34f, -34f), new Vector2(118f, 118f));
            MenuUI.Button(close, Close);

            // the power-up: its button art with the amount on offer
            Image card = MenuUI.Image("Card", frame, MenuUI.Rounded(30, 4, MenuUI.CardFill, MenuUI.CardBorder), null, false);
            MenuUI.Anchor(card.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -150f), new Vector2(640f, 520f));

            icon = MenuUI.Image("Icon", card.transform, null);
            MenuUI.Anchor(icon.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(240f, 240f));

            amountText = MenuUI.OutlinedText("Amount", icon.transform, "x2", 64f, new Color32(110, 52, 10, 255), TextAlignmentOptions.Center,
                new Color32(255, 214, 70, 255));
            MenuUI.Anchor(amountText.rectTransform, new Vector2(1f, 0f), new Vector2(0.5f, 0.5f), new Vector2(-6f, 26f), new Vector2(150f, 84f));

            nameText = MenuUI.OutlinedText("Name", card.transform, "", 56f, new Color32(110, 52, 10, 255), TextAlignmentOptions.Center,
                new Color32(255, 200, 45, 255));
            MenuUI.Anchor(nameText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -272f), new Vector2(580f, 64f));

            descriptionText = MenuUI.Text("Description", card.transform, "", 40f, MenuUI.TextDark, TextAlignmentOptions.Center, true);
            MenuUI.Anchor(descriptionText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -344f), new Vector2(580f, 150f));

            // BUY for coins, and the way to the Shop for more
            // glossy gold-rimmed buttons like the main menu's (exact picture proportions: 955x302)
            Button buy = MenuUI.LabelButton("Buy Button", frame, PopupArt.ButtonOrange(art), "000", 50f, Buy, art.coin, new Color32(140, 60, 10, 255));
            MenuUI.Anchor((RectTransform)buy.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 200f), new Vector2(420f, 133f));
            priceText = buy.GetComponentInChildren<TextMeshProUGUI>(true);
            priceIcon = buy.transform.Find("Label Row/Icon")?.GetComponent<Image>();
            if (priceIcon == null)
                priceIcon = MenuUI.Image("Icon", buy.transform, art.coin);

            Button shop = MenuUI.LabelButton("Shop Button", frame, PopupArt.ButtonGreen(art), "SHOP", 40f, OpenShop, null, new Color32(30, 90, 20, 255));
            MenuUI.Anchor((RectTransform)shop.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 84f), new Vector2(300f, 95f));

            messageText = MenuUI.OutlinedText("Message", frame, "", 38f, new Color32(120, 20, 20, 255), TextAlignmentOptions.Center,
                new Color32(255, 120, 100, 255));
            MenuUI.Anchor(messageText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 362f), new Vector2(620f, 50f));

            root.gameObject.SetActive(false);
        }

        #endregion
    }
}
