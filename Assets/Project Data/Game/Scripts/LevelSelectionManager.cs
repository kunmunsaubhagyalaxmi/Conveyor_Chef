using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

namespace Watermelon.BusStop
{
    /// <summary>
    /// Runtime behaviour for the serialized, designer-editable LevelSelection scene.
    /// The scene owns every RectTransform and visual position. Runtime only updates
    /// text, sprites, visibility, button state and the progress-fill width.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LevelSelectionController : MonoBehaviour
    {
        public static LevelSelectionController Instance { get; private set; }

        private const string SelectedCountryKey = "CC_CountryMap_SelectedCountry";
        private const string FromCountryMapKey = "CC_LevelSelection_FromCountryMap";
        private const string SelectedCountryLevelStartKey = "CC_CountryMap_SelectedLevelStart";
        private const string DiamondsKey = "CC_Diamonds";
        private const string DiamondCurrencyMigrationKey = "CC_DiamondCurrencyMigrated";
        private const int LevelsPerCountry = WorldCatalog.LevelsPerCountry;      // 30
        private const int CardsPerPage = 3;      // the three cards in the scene show one page of levels
        private const int Pages = (LevelsPerCountry + CardsPerPage - 1) / CardsPerPage;   // 10 pages
        private const float PageSlideDuration = 0.22f;

        private const string CountrySubtitle = "FLAVORS, CULTURE, JOURNEY";
        private const string SelectedContinentKey = "CC_WorldMap_SelectedContinent";

        [Header("Navigation")]
        [SerializeField] private Button homeButton;
        [SerializeField] private Button backButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button coinPlusButton;
        [FormerlySerializedAs("chefPlusButton")]
        [SerializeField] private Button diamondPlusButton;
        [SerializeField] private Button leftArrowButton;
        [SerializeField] private Button rightArrowButton;

        [Header("Top HUD")]
        [SerializeField] private TextMeshProUGUI coinText;
        [FormerlySerializedAs("chefCurrencyText")]
        [SerializeField] private TextMeshProUGUI diamondText;
        [SerializeField] private TextMeshProUGUI starText;
        [SerializeField] private Image diamondCounterImage;

        [Header("Country Header")]
        [SerializeField] private TextMeshProUGUI countryTitleText;
        [SerializeField] private TextMeshProUGUI countrySubtitleText;
        [SerializeField] private Image countryFlagImage;

        [Header("Hero / Description")]
        [SerializeField] private Image heroImage;
        [SerializeField] private Sprite indiaHeroSprite;
        [SerializeField] private TextMeshProUGUI descriptionText;

        [Header("Level Cards")]
        [SerializeField] private LevelSelectionLevelCard[] levelCards = new LevelSelectionLevelCard[3];
        [SerializeField] private Sprite[] indiaThumbnails = new Sprite[3];

        [Header("Guide")]
        [SerializeField] private TextMeshProUGUI guideText;

        [Header("Country Progress")]
        [SerializeField] private Image progressEmblem;
        [SerializeField] private Image progressFill;
        [SerializeField] private TextMeshProUGUI progressTitleText;
        [SerializeField] private TextMeshProUGUI progressValueText;

        [Header("State")]
        [SerializeField] private TextMeshProUGUI statusText;

        [Header("Settings")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Button closeSettingsButton;
        [SerializeField] private Button soundButton;
        [SerializeField] private Button vibrationButton;
        [SerializeField] private TextMeshProUGUI soundText;
        [SerializeField] private TextMeshProUGUI vibrationText;

        private LevelSave levelSave;
        private int selectedCountry;
        private int countryLevelStart;
        private int selectedLevel;          // selected level in this country (0-29)
        private Coroutine pageSlide;
        private int CurrentPage => selectedLevel / CardsPerPage;
        private bool currencySubscribed;
        private int sceneCountry = -1;

        // Inside a country's Level Selection (LevelSelectionCountry): that country, with the
        // hero, background, icon and thumbnails exactly as set in the Hierarchy.
        private bool UsesSceneArt => sceneCountry >= 0;

        private void Awake()
        {
            LevelSelectionCountry country = GetComponentInParent<LevelSelectionCountry>(true);
            if (country != null)
            {
                if (!LevelSelectionCountry.Resolve(country))
                    return;   // another country is shown; this copy is being removed

                sceneCountry = LevelSelectionCountry.IndexOf(country.CountryId);
            }

            Instance = this;
            EnsureSaveControllerReady();
            UIEventSystemRuntime.UseCurrentSceneEventSystem();

            levelSave = SaveController.GetSaveObject<LevelSave>("level");
            ApplyApprovedDiamondArtwork();

            Wire(homeButton, HomePressed);
            Wire(backButton, BackPressed);
            Wire(settingsButton, OpenSettings);
            Wire(coinPlusButton, CoinPlusPressed);
            Wire(diamondPlusButton, DiamondPlusPressed);
            Wire(leftArrowButton, PreviousLevel);
            Wire(rightArrowButton, NextLevel);
            Wire(closeSettingsButton, CloseSettings);
            Wire(soundButton, ToggleSound);
            Wire(vibrationButton, ToggleVibration);

            if (levelCards != null)
            {
                for (int i = 0; i < levelCards.Length; i++)
                    levelCards[i]?.Bind(this);
            }

            if (settingsPanel != null)
                settingsPanel.SetActive(false);

            // swipe left / right over the cards turns the page
            if (levelCards != null && levelCards.Length > 0 && levelCards[0] != null &&
                levelCards[0].transform.parent != null)
            {
                LevelSelectionSwipe swipe = levelCards[0].transform.parent.GetComponent<LevelSelectionSwipe>();
                if (swipe == null)
                    swipe = levelCards[0].transform.parent.gameObject.AddComponent<LevelSelectionSwipe>();
                swipe.Bind(this);
            }
        }

        private void Start()
        {
            // Global country index (0-29) across all continents, see WorldCatalog.
            selectedCountry = Mathf.Clamp(
                PlayerPrefs.GetInt(SelectedCountryKey, 2),
                0,
                WorldCatalog.CountryCount - 1);

            if (sceneCountry >= 0)
                selectedCountry = sceneCountry;

            countryLevelStart = WorldCatalog.FirstLevelOfCountry(selectedCountry);

            PlayerPrefs.SetInt(SelectedCountryKey, selectedCountry);
            PlayerPrefs.SetInt(SelectedCountryLevelStartKey, countryLevelStart);
            PlayerPrefs.Save();

            selectedLevel = ResolveInitialSelectedLevel();

            ApplyCountryPresentation();
            RefreshAll();
            RefreshSettingsLabels();

            CurrenciesController.InvokeOrSubcrtibe(() =>
            {
                RefreshHUD();

                if (!currencySubscribed)
                {
                    CurrenciesController.SubscribeGlobalCallback(OnCurrencyChanged);
                    currencySubscribed = true;
                }
            });
        }

        private void OnDestroy()
        {
            if (currencySubscribed)
            {
                CurrenciesController.UnsubscribeGlobalCallback(OnCurrencyChanged);
                currencySubscribed = false;
            }

            if (Instance == this)
                Instance = null;
        }

        private void OnCurrencyChanged(Currency currency, int difference)
        {
            if (currency == null)
                return;

            if (currency.CurrencyType == CurrencyType.Coins ||
                currency.CurrencyType == CurrencyType.Diamonds)
            {
                RefreshHUD();
            }
        }

        // The next level to play (the last open level when the country is done).
        private int ResolveInitialSelectedLevel()
        {
            for (int i = 0; i < LevelsPerCountry; i++)
            {
                int levelIndex = countryLevelStart + i;
                if (IsLevelUnlocked(levelIndex) && !IsLevelCompleted(levelIndex))
                    return i;
            }

            for (int i = LevelsPerCountry - 1; i >= 0; i--)
            {
                if (IsLevelUnlocked(countryLevelStart + i))
                    return i;
            }

            return 0;
        }

        // Level shown by a card on the current page (-1 past the country's last level).
        private int CardLevel(int slot)
        {
            int inCountry = CurrentPage * CardsPerPage + slot;
            return inCountry < LevelsPerCountry ? inCountry : -1;
        }

        private void ApplyCountryPresentation()
        {
            WorldCatalog.Country country = WorldCatalog.GetCountry(selectedCountry);
            string countryName = country != null ? country.Name : "Country";

            if (countryTitleText != null)
                countryTitleText.text = countryName.ToUpperInvariant();

            if (countrySubtitleText != null)
                countrySubtitleText.text = CountrySubtitle;

            if (descriptionText != null && country != null)
                descriptionText.text = country.Description;

            if (guideText != null)
                guideText.text = "Complete all " + LevelsPerCountry + " levels\nto master " + countryName +
                                 (countryName.EndsWith("s") ? "’" : "’s") + " flavors!";

            if (progressTitleText != null)
                progressTitleText.text = "COUNTRY PROGRESS";

            // A country's own scene keeps the art set in its Hierarchy.
            if (UsesSceneArt)
                return;

            // Country art by name (Resources/World/<continent>/<country>/...), with the
            // existing Asian art standing in until a country has its own.
            Sprite hero = WorldArt.ForCountry(selectedCountry, WorldArt.LevelSelectHero);
            if (hero == null)
                hero = indiaHeroSprite;

            if (hero != null)
            {
                if (heroImage != null)
                    heroImage.sprite = hero;
                SetSceneSprite("Background Artwork", hero);
            }

            Sprite icon = WorldArt.ForCountry(selectedCountry, WorldArt.LevelSelectIcon);
            if (icon != null)
            {
                SetSceneSprite("India Icon", icon);
                SetSceneSprite("Country Icon", icon);
            }

            if (countryFlagImage != null)
            {
                Sprite flag = WorldArt.ForCountry(selectedCountry, WorldArt.FlagRound);
                if (flag != null)
                    countryFlagImage.sprite = flag;
            }
        }

        private void SetSceneSprite(string objectName, Sprite sprite)
        {
            if (sprite == null)
                return;

            Transform target = FindDeep(transform.root, objectName);
            if (target == null)
            {
                GameObject found = GameObject.Find(objectName);
                target = found != null ? found.transform : null;
            }

            Image image = target != null ? target.GetComponent<Image>() : null;
            if (image != null)
                image.sprite = sprite;
        }

        private static Transform FindDeep(Transform parent, string objectName)
        {
            if (parent == null)
                return null;
            if (parent.name == objectName)
                return parent;
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform found = FindDeep(parent.GetChild(i), objectName);
                if (found != null)
                    return found;
            }
            return null;
        }

        private void RefreshAll()
        {
            RefreshHUD();
            RefreshCards();
            RefreshProgress();

            if (leftArrowButton != null)
                leftArrowButton.interactable = CurrentPage > 0;

            if (rightArrowButton != null)
                rightArrowButton.interactable = CurrentPage < Pages - 1;
        }


        private void ApplyApprovedDiamondArtwork()
        {
            if (diamondCounterImage == null)
                return;

            // Use the exact approved Main Menu diamond counter artwork. That artwork
            // already contains the faceted blue gem and the green plus-button chrome.
            Sprite approvedDiamondBar = ProfessionalMainMenuEmbeddedAssets.GetSprite("diamond_bar");
            if (approvedDiamondBar != null)
            {
                diamondCounterImage.sprite = approvedDiamondBar;
                diamondCounterImage.color = Color.white;
                diamondCounterImage.preserveAspect = false;
            }

            // Older LevelSelection scenes had a separate placeholder diamond icon and
            // a visible Plus image. Hide those visuals so only the approved artwork is
            // rendered, while keeping the Plus Button component clickable.
            Transform legacyIcon = diamondCounterImage.transform.Find("Diamond Icon");
            if (legacyIcon != null)
                legacyIcon.gameObject.SetActive(false);

            Transform plusTransform = diamondCounterImage.transform.Find("Diamond Plus");
            Image plusImage = plusTransform != null ? plusTransform.GetComponent<Image>() : null;
            if (plusImage != null)
                plusImage.color = new Color(1f, 1f, 1f, 0f);
        }

        private void RefreshHUD()
        {
            if (coinText != null)
                CurrencyText.Set(coinText, GetCurrencyAmountSafe(CurrencyType.Coins));

            if (diamondText != null)
                CurrencyText.Set(diamondText, GetDiamondAmount());

            if (starText != null)
                starText.text = (levelSave != null ? levelSave.GetTotalStars() : 0).ToString("N0");
        }

        private static int GetDiamondAmount()
        {
            try
            {
                if (!PlayerPrefs.HasKey(DiamondCurrencyMigrationKey))
                {
                    int legacyAmount = PlayerPrefs.GetInt(DiamondsKey, 50);
                    PlayerPrefs.SetInt(DiamondCurrencyMigrationKey, 1);
                    PlayerPrefs.Save();
                    CurrenciesController.Set(CurrencyType.Diamonds, legacyAmount);
                }

                return CurrenciesController.Get(CurrencyType.Diamonds);
            }
            catch
            {
                return PlayerPrefs.GetInt(DiamondsKey, 50);
            }
        }

        private static int GetCurrencyAmountSafe(CurrencyType currencyType)
        {
            try
            {
                return CurrenciesController.Get(currencyType);
            }
            catch
            {
                return 0;
            }
        }

        // One card per level: the three scene cards show the current page (levels 1-3, 4-6 ...
        // 28-30). Each card cycles through the country's three place pictures and is titled
        // with that place, shows the level's number, its own stars and PLAY.
        private void RefreshCards()
        {
            if (levelCards == null)
                return;

            WorldCatalog.Country country = WorldCatalog.GetCountry(selectedCountry);
            int places = country != null && country.Missions.Length > 0 ? country.Missions.Length : 3;

            for (int slot = 0; slot < levelCards.Length && slot < CardsPerPage; slot++)
            {
                LevelSelectionLevelCard card = levelCards[slot];
                if (card == null)
                    continue;

                int inCountry = CardLevel(slot);
                card.gameObject.SetActive(inCountry >= 0);
                if (inCountry < 0)
                    continue;

                int levelIndex = countryLevelStart + inCountry;
                bool unlocked = IsLevelUnlocked(levelIndex);
                bool completed = IsLevelCompleted(levelIndex);
                int stars = completed ? Mathf.Max(1, GetLevelStars(levelIndex)) : 0;

                // Every level has its own picture (Resources/World/<continent>/<country>/level_N.png)
                // and title (card_titles.txt). Until a picture exists the card falls back to the
                // country's three place pictures (place == slot: pages are 3 long), and a country
                // scene to the picture the card had in its Hierarchy.
                int place = inCountry % places;
                Sprite thumbnail = WorldArt.ForCountry(selectedCountry, WorldArt.LevelThumbnail(inCountry), false);
                if (thumbnail == null)
                    thumbnail = UsesSceneArt ? card.AuthoredThumbnail : WorldArt.ForCountry(selectedCountry, WorldArt.LevelThumbnail(place));
                if (thumbnail == null && indiaThumbnails != null && place < indiaThumbnails.Length)
                    thumbnail = indiaThumbnails[place];

                string title = WorldArt.CardTitle(levelIndex) ?? WorldCatalog.GetMissionName(selectedCountry, place);

                card.Refresh(levelIndex, title, thumbnail, unlocked, inCountry == selectedLevel, completed, stars,
                             completed ? "REPLAY" : "PLAY", levelIndex + 1);   // game-wide number: China 1-30, Japan 31-60...
            }

            if (statusText != null)
            {
                // game-wide numbers, as on the cards (China 1-30, Japan 31-60...)
                int first = countryLevelStart + CurrentPage * CardsPerPage + 1;
                int last = Mathf.Min(first + CardsPerPage - 1, countryLevelStart + LevelsPerCountry);
                int shown = countryLevelStart + selectedLevel + 1;
                statusText.text = IsLevelUnlocked(countryLevelStart + selectedLevel)
                    ? "LEVELS " + first + "\u2013" + last
                    : "FINISH LEVEL " + (shown - 1) + " TO UNLOCK LEVEL " + shown;
            }
        }

        private static void FitText(TextMeshProUGUI text)
        {
            if (text == null || text.enableAutoSizing)
                return;

            text.fontSizeMax = text.fontSize;
            text.fontSizeMin = Mathf.Min(text.fontSizeMin > 0f ? text.fontSizeMin : 18f, text.fontSize);
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.enableAutoSizing = true;
        }

        private void RefreshProgress()
        {
            int completed = 0;
            for (int i = 0; i < LevelsPerCountry; i++)
            {
                if (IsLevelCompleted(countryLevelStart + i))
                    completed++;
            }

            if (progressValueText != null)
            {
                // "16/30" is wider than "3/30": it shrinks to fit its box instead of spilling out
                // of the bar's frame (the authored size stays the maximum), like the Country Map.
                FitText(progressValueText);
                progressValueText.text = completed + "/" + LevelsPerCountry;
            }

            if (progressFill == null)
                return;

            float normalized = completed / (float)LevelsPerCountry;

            // Never move or resize authored UI in Play Mode.
            progressFill.type = Image.Type.Filled;
            progressFill.fillMethod = Image.FillMethod.Horizontal;
            progressFill.fillOrigin = 0;
            progressFill.fillClockwise = true;
            // The fill picture has empty margins (its bar runs from 3.4% to 96.6% of the width).
            progressFill.fillAmount = normalized <= 0f ? 0f : Mathf.Lerp(0.0336f, 0.9664f, normalized);
            progressFill.preserveAspect = false;
            progressFill.raycastTarget = false;
        }

        public void HandleCardPressed(int slot)
        {
            int inCountry = CardLevel(Mathf.Clamp(slot, 0, CardsPerPage - 1));
            if (inCountry < 0)
                return;
            selectedLevel = inCountry;
            PlayClick();
            RefreshAll();
        }

        public void HandlePlayPressed(int slot)
        {
            int inCountry = CardLevel(Mathf.Clamp(slot, 0, CardsPerPage - 1));
            if (inCountry < 0)
                return;
            selectedLevel = inCountry;
            int levelIndex = countryLevelStart + inCountry;

            if (!IsLevelUnlocked(levelIndex))
            {
                HandleLockedPressed(slot);
                return;
            }

            LoadSelectedLevel(levelIndex);
        }

        public void HandleLockedPressed(int slot)
        {
            int inCountry = CardLevel(Mathf.Clamp(slot, 0, CardsPerPage - 1));
            if (inCountry >= 0)
                selectedLevel = inCountry;
            PlayClick();
            RefreshCards();     // the status line says which level to finish first
        }

        private void PreviousLevel() => TurnPage(-1);

        private void NextLevel() => TurnPage(1);

        /// <summary>Shows the previous / next three levels (arrows and swipes).</summary>
        public void TurnPage(int direction)
        {
            int page = Mathf.Clamp(CurrentPage + direction, 0, Pages - 1);
            if (page == CurrentPage)
                return;

            // keep the same card position on the new page
            int slot = selectedLevel % CardsPerPage;
            selectedLevel = Mathf.Min(page * CardsPerPage + slot, LevelsPerCountry - 1);
            PlayClick();
            RefreshAll();
            SlideCards(direction);
        }

        // The new page slides in from the side it came from (unscaled time).
        private void SlideCards(int direction)
        {
            RectTransform cardsRoot = levelCards != null && levelCards.Length > 0 && levelCards[0] != null
                ? levelCards[0].transform.parent as RectTransform
                : null;
            if (cardsRoot == null || !isActiveAndEnabled)
                return;
            if (pageSlide != null)
                StopCoroutine(pageSlide);
            pageSlide = StartCoroutine(SlideRoutine(cardsRoot, direction));
        }

        private Vector2 cardsHome;
        private bool cardsHomeKnown;

        private System.Collections.IEnumerator SlideRoutine(RectTransform cardsRoot, int direction)
        {
            if (!cardsHomeKnown)
            {
                cardsHome = cardsRoot.anchoredPosition;
                cardsHomeKnown = true;
            }

            CanvasGroup group = cardsRoot.GetComponent<CanvasGroup>();
            if (group == null)
                group = cardsRoot.gameObject.AddComponent<CanvasGroup>();

            float distance = cardsRoot.rect.width * 0.18f * direction;
            for (float t = 0f; t < PageSlideDuration; t += Time.unscaledDeltaTime)
            {
                float k = t / PageSlideDuration;
                float eased = 1f - (1f - k) * (1f - k) * (1f - k);
                cardsRoot.anchoredPosition = cardsHome + new Vector2(distance * (1f - eased), 0f);
                group.alpha = Mathf.Lerp(0.2f, 1f, eased);
                yield return null;
            }

            cardsRoot.anchoredPosition = cardsHome;
            group.alpha = 1f;
            pageSlide = null;
        }

        public void LoadSelectedLevel(int levelIndex)
        {
            if (levelSave == null)
            {
                Debug.LogError("[LevelSelection] LevelSave is unavailable.");
                return;
            }

            if (!IsLevelUnlocked(levelIndex))
                return;

            levelSave.selectedLevelIndex = levelIndex;
            levelSave.isPlayingFromLevelSelection = true;
            levelSave.ReplayingLevelAgain = IsLevelCompleted(levelIndex);

            SaveController.MarkAsSaveIsRequired();
            SaveController.Save(true);

            // Keep CountryMap context so gameplay -> LevelSelection returns to
            // this same country screen instead of the legacy generic selector.
            PlayerPrefs.SetInt(FromCountryMapKey, 1);
            PlayerPrefs.SetInt(SelectedCountryLevelStartKey, countryLevelStart);
            PlayerPrefs.Save();

            PlayClick();
            EnhancedLoadingScreen.LoadViaLoadingScreen("Game");
        }

        public void BackToMainMenu()
        {
            BackPressed();
        }

        public void ShowPage(int pageIndex)
        {
            int page = Mathf.Clamp(pageIndex, 0, Pages - 1);
            selectedLevel = Mathf.Min(page * CardsPerPage + selectedLevel % CardsPerPage, LevelsPerCountry - 1);
            RefreshAll();
        }

        public bool IsPageUnlocked(int pageIndex)
        {
            int page = Mathf.Clamp(pageIndex, 0, Pages - 1);
            return IsLevelUnlocked(countryLevelStart + page * CardsPerPage);
        }

        public int GetCurrentPageIndex()
        {
            return CurrentPage;
        }

        private void BackPressed()
        {
            PlayClick();
            PlayerPrefs.SetInt(FromCountryMapKey, 1);
            // Back to this country's continent on the Country Map.
            PlayerPrefs.SetInt(SelectedContinentKey, Mathf.Max(0, WorldCatalog.ContinentOfCountry(selectedCountry)));
            PlayerPrefs.Save();
            EnhancedLoadingScreen.LoadViaLoadingScreen("CountryMap");
        }

        private void HomePressed()
        {
            PlayClick();
            EnhancedLoadingScreen.LoadViaLoadingScreen("menu");
        }

        // The coin / diamond "+" opens the Shop, as on every screen.
        private void CoinPlusPressed() => MenuPanel.Show<ShopPanel>();

        private void DiamondPlusPressed() => MenuPanel.Show<ShopPanel>();

        // Every settings button in the game opens the shared Settings panel.
        private void OpenSettings()
        {
            ConveyorSettingsPanel.Show();
        }

        private void CloseSettings()
        {
            PlayClick();
            if (settingsPanel != null)
                settingsPanel.SetActive(false);
        }

        private void ToggleSound()
        {
            bool enabled = AudioController.GetVolume() > 0.001f;
            AudioController.SetVolume(enabled ? 0f : 1f);
            RefreshSettingsLabels();
            PlayClick();
        }

        private void ToggleVibration()
        {
            AudioController.SetVibrationState(!AudioController.IsVibrationEnabled());
            RefreshSettingsLabels();
            PlayClick();
        }

        private void RefreshSettingsLabels()
        {
            if (soundText != null)
                soundText.text = "SOUND: " + (AudioController.GetVolume() > 0.001f ? "ON" : "OFF");

            if (vibrationText != null)
                vibrationText.text = "VIBRATION: " + (AudioController.IsVibrationEnabled() ? "ON" : "OFF");
        }

        private bool IsLevelUnlocked(int levelIndex) => GameProgress.IsLevelUnlocked(levelIndex);

        private bool IsLevelCompleted(int levelIndex) => GameProgress.IsLevelCompleted(levelIndex);

        private int GetLevelStars(int levelIndex)
        {
            if (levelSave == null)
                return 0;

            LevelProgressData progress = levelSave.GetLevelProgress(levelIndex);
            return progress != null ? Mathf.Clamp(progress.starsEarned, 0, 3) : 0;
        }

        private static void Wire(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
                return;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        private static void EnsureSaveControllerReady()
        {
            if (SaveController.IsSaveLoaded)
                return;

            try
            {
                SaveController.Initialise(useAutoSave: false);
            }
            catch (Exception ex)
            {
                Debug.LogError("[LevelSelection] SaveController initialisation failed: " + ex.Message);
            }
        }

        private static void PlayClick()
        {
            try
            {
                AudioController.PlaySound(AudioController.Sounds.buttonSound);
            }
            catch
            {
                // UI remains functional when audio has not initialised yet.
            }
        }

#if UNITY_EDITOR
        public void EditorConfigure(
            Button home,
            Button back,
            Button settings,
            Button coinPlus,
            Button diamondPlus,
            Button leftArrow,
            Button rightArrow,
            TextMeshProUGUI coins,
            TextMeshProUGUI diamonds,
            TextMeshProUGUI stars,
            Image diamondCounter,
            TextMeshProUGUI countryTitle,
            TextMeshProUGUI countrySubtitle,
            Image flag,
            Image hero,
            Sprite indiaHero,
            TextMeshProUGUI description,
            LevelSelectionLevelCard[] cards,
            Sprite[] indiaMissionThumbnails,
            TextMeshProUGUI guide,
            Image emblem,
            Image fill,
            TextMeshProUGUI progressTitle,
            TextMeshProUGUI progressValue,
            TextMeshProUGUI state,
            GameObject settingsRoot,
            Button closeSettings,
            Button sound,
            Button vibration,
            TextMeshProUGUI soundLabel,
            TextMeshProUGUI vibrationLabel)
        {
            homeButton = home;
            backButton = back;
            settingsButton = settings;
            coinPlusButton = coinPlus;
            diamondPlusButton = diamondPlus;
            leftArrowButton = leftArrow;
            rightArrowButton = rightArrow;
            coinText = coins;
            diamondText = diamonds;
            starText = stars;
            diamondCounterImage = diamondCounter;
            countryTitleText = countryTitle;
            countrySubtitleText = countrySubtitle;
            countryFlagImage = flag;
            heroImage = hero;
            indiaHeroSprite = indiaHero;
            descriptionText = description;
            levelCards = cards;
            indiaThumbnails = indiaMissionThumbnails;
            guideText = guide;
            progressEmblem = emblem;
            progressFill = fill;
            progressTitleText = progressTitle;
            progressValueText = progressValue;
            statusText = state;
            settingsPanel = settingsRoot;
            closeSettingsButton = closeSettings;
            soundButton = sound;
            vibrationButton = vibration;
            soundText = soundLabel;
            vibrationText = vibrationLabel;
        }
#endif
    }
}
