using UnityEngine;
using UnityEngine.UI;
using Watermelon.IAPStore;

namespace Watermelon
{
    /// <summary>
    /// Small runtime bridge for NEW GAME SCENE [ACTIVE].
    /// Existing UIGame, LivesIndicator, CurrencyUIPanelSimple, UIOrderPanel and
    /// PUUIController remain the actual gameplay/UI logic.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameSceneHUDController : MonoBehaviour
    {
        [Header("Currency Logic")]
        [SerializeField] private CurrencyUIPanelSimple coinPanel;
        [SerializeField] private CurrencyUIPanelSimple diamondPanel;
        [SerializeField] private Image coinIconImage;
        [SerializeField] private Image diamondIconImage;

        [Header("Extra Buttons")]
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button homeButton;

        [Header("Existing Pause Logic")]
        [SerializeField] private UILevelQuitPopUp pausePopup;

        [Header("Power-up Artwork")]
        [SerializeField] private Sprite undoButtonSprite;
        [SerializeField] private Sprite hintButtonSprite;
        [SerializeField] private Sprite shuffleButtonSprite;
        [SerializeField] private Sprite countBadgeSprite;

        [Header("Currency Artwork")]
        [SerializeField] private Sprite coinIconSprite;
        [SerializeField] private Sprite diamondIconSprite;

        private PUUIController lastPowerUpsController;
        private int lastPowerUpCount = -1;
        private UIGame game;

        private void Awake()
        {
            game = GetComponent<UIGame>();

            if (coinPanel != null && coinPanel.AddButton != null)
                coinPanel.AddButton.onClick.AddListener(OpenCurrencyStore);

            if (diamondPanel != null && diamondPanel.AddButton != null)
                diamondPanel.AddButton.onClick.AddListener(OpenCurrencyStore);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(ConveyorSettingsPanel.Show);

            if (homeButton != null)
                homeButton.onClick.AddListener(ReturnToLevelSelection);
        }

        private void Start()
        {
            // CurrencyUIPanelSimple keeps the original live update logic.
            CurrenciesController.InvokeOrSubcrtibe(InitialiseCurrencyPanels);
            TrySkinPowerUps(true);
        }

        private void Update()
        {
            TrySkinPowerUps(false);
            UpdateToolbarSpots();
        }

        private void InitialiseCurrencyPanels()
        {
            if (coinPanel != null)
                coinPanel.Initialise();

            if (diamondPanel != null)
                diamondPanel.Initialise();

            // CurrencyUIPanelSimple assigns the database icons while initialising.
            // Re-apply the generated art after that so logic stays original while
            // the visible icon follows the redesigned Game UI.
            if (coinIconImage != null && coinIconSprite != null)
            {
                coinIconImage.sprite = coinIconSprite;
                coinIconImage.preserveAspect = true;
            }

            if (diamondIconImage != null && diamondIconSprite != null)
            {
                diamondIconImage.sprite = diamondIconSprite;
                diamondIconImage.preserveAspect = true;
            }
        }

        private static void OpenCurrencyStore()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);
            UIController.ShowPage<UIIAPStore>();
        }

        private void OpenPauseOptions()
        {
            if (pausePopup == null)
                return;

            AudioController.PlaySound(AudioController.Sounds.buttonSound);
            pausePopup.Show();
        }

        private static void ReturnToLevelSelection()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);
            GameController.ReturnToLevelSelection();
        }

        private void TrySkinPowerUps(bool force)
        {
            PUUIController controller = game != null ? game.PowerUpsUIController : null;

            if (controller == null || controller.UIBehaviors == null)
                return;

            PUUIBehavior[] items = controller.UIBehaviors;
            if (!force && controller == lastPowerUpsController && items.Length == lastPowerUpCount)
                return;

            lastPowerUpsController = controller;
            lastPowerUpCount = items.Length;

            foreach (PUUIBehavior item in items)
            {
                if (item == null || item.Settings == null)
                    continue;

                Sprite fullButtonSprite = null;

                if (item.Settings is PUUndoSettings)
                    fullButtonSprite = undoButtonSprite;
                else if (item.Settings is PUHintSettings)
                    fullButtonSprite = hintButtonSprite;
                else if (item.Settings is PUShuffleSettings)
                    fullButtonSprite = shuffleButtonSprite;
                else
                    fullButtonSprite = item.Settings.Icon;     // newer power-ups bring their full button as the icon

                // In PU Behavior UI the visible art is "Icon" (full size); "Background" is
                // a disabled 10px image. The full-button sprite therefore goes on Icon.
                Transform icon = item.transform.Find("Icon");
                if (icon != null && fullButtonSprite != null)
                {
                    Image image = icon.GetComponent<Image>();
                    if (image != null)
                    {
                        image.sprite = fullButtonSprite;
                        image.type = Image.Type.Simple;
                        image.preserveAspect = true;
                    }

                    icon.gameObject.SetActive(true);
                }

                Transform amountBackground = item.transform.Find("Amount Background");
                if (amountBackground != null && countBadgeSprite != null)
                {
                    Image badge = amountBackground.GetComponent<Image>();
                    if (badge != null)
                    {
                        badge.sprite = countBadgeSprite;
                        badge.color = Color.white;
                        badge.type = Image.Type.Simple;
                        badge.preserveAspect = true;
                    }
                }
            }
        }

        // ------------------------------------------------------------ Extra Plate spot
        // The toolbar's first spot held the menu button (the pause button opens the same menu).
        // It is hidden and EXTRA PLATE sits there instead; Undo/Hint/Shuffle keep spots 2-4.
        // The button lives in a holder scaled like the power-ups' row, because the button
        // itself is animated to scale 1 when it appears.
        private RectTransform extraPlateSpot;
        private PUUIBehavior extraPlateItem;
        private RectTransform firstSpotButton;

        private void UpdateToolbarSpots()
        {
            PUUIController controller = lastPowerUpsController;
            if (controller == null || controller.UIBehaviors == null)
                return;

            // Already in place: nothing to do this frame.
            if (extraPlateItem != null && extraPlateItem.transform.parent == extraPlateSpot
                && firstSpotButton != null && !firstSpotButton.gameObject.activeSelf
                && extraPlateSpot.localScale == controller.transform.localScale)
                return;

            if (firstSpotButton == null)
                firstSpotButton = FindFirstSpotButton(controller);
            RectTransform toolbar = firstSpotButton != null ? firstSpotButton.parent as RectTransform : null;
            if (toolbar == null)
                return;

            if (firstSpotButton.gameObject.activeSelf)
                firstSpotButton.gameObject.SetActive(false);

            foreach (PUUIBehavior item in controller.UIBehaviors)
            {
                if (item == null || !(item.Settings is PUExtraSlotSettings))
                    continue;

                extraPlateItem = item;

                if (extraPlateSpot == null)
                {
                    extraPlateSpot = new GameObject("Extra Plate Spot", typeof(RectTransform)).GetComponent<RectTransform>();
                    extraPlateSpot.SetParent(toolbar, false);
                    extraPlateSpot.anchorMin = extraPlateSpot.anchorMax = firstSpotButton.anchorMin;
                    extraPlateSpot.pivot = new Vector2(0.5f, 0.5f);
                    extraPlateSpot.sizeDelta = Vector2.zero;
                }

                extraPlateSpot.anchoredPosition = firstSpotButton.anchoredPosition;
                extraPlateSpot.localScale = controller.transform.localScale;

                if (item.transform.parent != extraPlateSpot)
                {
                    RectTransform rect = (RectTransform)item.transform;
                    rect.SetParent(extraPlateSpot, false);
                    rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.anchoredPosition = Vector2.zero;
                }
            }
        }

        // The old HUD wires its "Home Button"; GAME MAIN CANVAS [NEW UI] has no reference here and
        // calls the same toolbar spot "Menu Button" (a sibling of the power-ups' row).
        private RectTransform FindFirstSpotButton(PUUIController controller)
        {
            if (homeButton != null)
                return homeButton.transform as RectTransform;

            Transform toolbar = controller.transform.parent;
            if (toolbar == null)
                return null;

            Transform menu = toolbar.Find("Menu Button");
            if (menu == null)
                menu = toolbar.Find("Home Button");
            return menu as RectTransform;
        }

#if UNITY_EDITOR
        public void EditorConfigure(
            CurrencyUIPanelSimple coins,
            CurrencyUIPanelSimple diamonds,
            Image coinIcon,
            Image diamondIcon,
            Button settings,
            Button home,
            UILevelQuitPopUp popup,
            Sprite undo,
            Sprite hint,
            Sprite shuffle,
            Sprite badge,
            Sprite coinSprite,
            Sprite diamondSprite)
        {
            coinPanel = coins;
            diamondPanel = diamonds;
            coinIconImage = coinIcon;
            diamondIconImage = diamondIcon;
            settingsButton = settings;
            homeButton = home;
            pausePopup = popup;
            undoButtonSprite = undo;
            hintButtonSprite = hint;
            shuffleButtonSprite = shuffle;
            countBadgeSprite = badge;
            coinIconSprite = coinSprite;
            diamondIconSprite = diamondSprite;
        }
#endif
    }
}
