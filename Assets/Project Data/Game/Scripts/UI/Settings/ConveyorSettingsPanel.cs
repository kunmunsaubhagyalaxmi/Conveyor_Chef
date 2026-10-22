using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Watermelon
{
    /// <summary>
    /// The game's single Settings panel, opened by every settings button (main menu,
    /// World Map, Country Map, Continent Map, Level Selection, Game HUD) through
    /// <see cref="Show"/>. It builds itself on first use on its own overlay canvas above
    /// all scene UI, survives scene loads, and closes when a new scene loads.
    /// SOUND, MUSIC, VIBRATION and LANGUAGE; LANGUAGE opens the language list.
    /// Art (words painted in) comes from Resources/SettingsPanelArt.
    /// </summary>
    public sealed class ConveyorSettingsPanel : MonoBehaviour
    {
        private const int SortingOrder = 5000;

        private static ConveyorSettingsPanel instance;

        private GameObject root;
        private GameObject settingsPage;
        private GameObject languagePage;
        private Image soundImage;
        private Image musicImage;
        private Image vibrationImage;
        private TMP_Text soundLabel;
        private TMP_Text musicLabel;
        private TMP_Text vibrationLabel;
        private TMP_Text versionText;
        private Image[] languageTicks;
        private SettingsPanelArt art;

        public static bool IsOpen => instance != null && instance.root != null && instance.root.activeSelf;

        public static void Show()
        {
            EnsureInstance();
            instance.Open();
        }

        /// <summary>
        /// Opens on the language list alone (the in-game menu's LANGUAGE button): its X, or a tap
        /// outside, closes the panel (no Settings page) and then calls <paramref name="onClosed"/>.
        /// </summary>
        public static void ShowLanguages(System.Action onClosed = null)
        {
            EnsureInstance();
            instance.languagesOnly = true;
            instance.languagesClosed = onClosed;
            instance.Refresh();
            instance.ShowPage(language: true);
            instance.root.SetActive(true);
        }

        private bool languagesOnly;
        private System.Action languagesClosed;

        public static void Hide()
        {
            if (instance != null)
                instance.Close(playSound: false);
        }

        private static void EnsureInstance()
        {
            if (instance != null)
                return;

            GameObject go = new GameObject("[SETTINGS PANEL]");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<ConveyorSettingsPanel>();
            instance.Build(Resources.Load<SettingsPanelArt>(SettingsPanelArt.ResourcePath));
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Close(playSound: false);
        }

        private void Open()
        {
            languagesOnly = false;
            languagesClosed = null;
            PlayClick();
            ShowPage(language: false);
            Refresh();
            root.SetActive(true);
        }

        private void Close(bool playSound)
        {
            if (root == null || !root.activeSelf)
                return;

            if (playSound)
                PlayClick();

            root.SetActive(false);

            // opened from the in-game menu: hand back to it
            System.Action closed = languagesClosed;
            languagesOnly = false;
            languagesClosed = null;
            closed?.Invoke();
        }

        private void ShowPage(bool language)
        {
            settingsPage.SetActive(!language);
            languagePage.SetActive(language);
        }

        private void OpenLanguages()
        {
            PlayClick();
            Refresh();
            ShowPage(language: true);
        }

        private void BackToSettings()
        {
            if (languagesOnly)
            {
                Close(playSound: true);       // opened on its own: close, never show Settings
                return;
            }

            PlayClick();
            ShowPage(language: false);
        }

        private void PickLanguage(GameLanguage language)
        {
            Loc.Set(language);
            PlayClick();
            Refresh();
        }

        private void ToggleSound()
        {
            bool enabled = IsSoundOn();
            try { AudioController.SetVolume(enabled ? 0f : 1f); } catch { }
            PlayClick();
            Refresh();
        }

        private void ToggleMusic()
        {
            MusicSetting.Set(!MusicSetting.On);
            PlayClick();
            Refresh();
        }

        private void ToggleVibration()
        {
            bool enabled = IsVibrationOn();
            try { AudioController.SetVibrationState(!enabled); } catch { }
            PlayClick();
            Refresh();
        }

        // Audio may not be initialised when a scene is opened directly in the editor;
        // the panel keeps working and assumes the defaults.
        private static bool IsSoundOn()
        {
            try { return AudioController.GetVolume() > 0.001f; } catch { return true; }
        }

        private static bool IsVibrationOn()
        {
            try { return AudioController.IsVibrationEnabled(); } catch { return true; }
        }

        private void Refresh()
        {
            ApplyState(soundImage, soundLabel, IsSoundOn(),
                art != null ? art.soundOn : null, art != null ? art.soundOff : null, "SOUND");
            ApplyState(musicImage, musicLabel, MusicSetting.On,
                art != null ? art.musicOn : null, art != null ? art.musicOff : null, "MUSIC");
            ApplyState(vibrationImage, vibrationLabel, IsVibrationOn(),
                art != null ? art.vibrationOn : null, art != null ? art.vibrationOff : null, "VIBRATION");

            versionText.text = Loc.T("VERSION") + " " + Application.version;
            for (int i = 0; i < languageTicks.Length; i++)
                languageTicks[i].gameObject.SetActive(i == (int)Loc.Current);
        }

        private static void ApplyState(Image image, TMP_Text label, bool on, Sprite onSprite, Sprite offSprite, string name)
        {
            Sprite sprite = on ? onSprite : offSprite;
            if (sprite != null)
            {
                image.sprite = sprite;
                image.color = Color.white;
            }
            else
            {
                image.sprite = null;
                image.color = on ? new Color(0.2f, 0.65f, 0.25f) : new Color(0.75f, 0.25f, 0.2f);
            }

            label.gameObject.SetActive(sprite == null);
            label.text = Loc.T(name + (on ? " ON" : " OFF"));
        }

        #region Build

        // Layout at 1080x1920. Panel: pause_popup_panel.png (768x1024, blue title band at
        // 11%, cream area 19-88% of its height).
        private const float PanelW = PopupArt.PanelWidth;       // the shared popup layout (as MENU)
        private const float PanelH = PopupArt.PanelHeight;
        private const float CreamTop = PopupArt.CreamTop;
        private const float CreamBottom = PopupArt.CreamBottom;

        private void Build(SettingsPanelArt panelArt)
        {
            art = panelArt;
            if (art == null)
                Debug.LogWarning("[Settings] Resources/" + SettingsPanelArt.ResourcePath + " not found; using plain buttons.");

            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = SortingOrder;

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0f;

            gameObject.AddComponent<GraphicRaycaster>();

            RectTransform rootRect = CreateRect("Settings", transform);
            Stretch(rootRect);
            root = rootRect.gameObject;

            // Dim background: tapping outside the panel closes it.
            Image dim = CreateImage("Dim", rootRect, null, new Color(0.03f, 0.05f, 0.1f, 0.72f));
            Stretch(dim.rectTransform);
            AddButton(dim, () => Close(playSound: true)).transition = Selectable.Transition.None;

            // SETTINGS: four buttons spread over the cream area, the version under them.
            RectTransform page = BuildPage("Settings Page", rootRect, art != null ? art.title : null, "SETTINGS",
                () => Close(playSound: true));
            settingsPage = page.gameObject;

            Vector2 toggleSize = new Vector2(430f, 146f);
            soundImage = CreateToggle("Sound Button", page, new Vector2(0f, Row(0, 4)), toggleSize, out soundLabel);
            AddButton(soundImage, ToggleSound);
            musicImage = CreateToggle("Music Button", page, new Vector2(0f, Row(1, 4)), toggleSize, out musicLabel);
            AddButton(musicImage, ToggleMusic);
            vibrationImage = CreateToggle("Vibration Button", page, new Vector2(0f, Row(2, 4)), toggleSize, out vibrationLabel);
            AddButton(vibrationImage, ToggleVibration);

            Image language = CreateToggle("Language Button", page, new Vector2(0f, Row(3, 4)), toggleSize, out TMP_Text languageLabel);
            SetArt(language, languageLabel, art != null ? art.languageButton : null, "LANGUAGE", new Color(0.2f, 0.45f, 0.8f));
            AddButton(language, OpenLanguages);

            versionText = CreateText("Version", page, "VERSION " + Application.version, new Color32(35, 48, 70, 255), 32f);
            Place(versionText.rectTransform, new Vector2(0f, PanelH * 0.5f - PanelH * CreamBottom + 36f), new Vector2(500f, 48f));

            // LANGUAGE: the eight languages in two columns, a tick on the chosen one.
            page = BuildPage("Language Page", rootRect, art != null ? art.languageTitle : null, "LANGUAGE", BackToSettings);
            languagePage = page.gameObject;

            GameLanguage[] all = Loc.All;
            languageTicks = new Image[all.Length];
            int rows = (all.Length + 1) / 2;
            Vector2 cell = new Vector2(285f, 97f);
            for (int i = 0; i < all.Length; i++)
            {
                GameLanguage lang = all[i];
                Vector2 position = new Vector2(i % 2 == 0 ? -148f : 148f, Row(i / 2, rows));
                Image button = CreateToggle("Language " + lang, page, position, cell, out TMP_Text label);
                Sprite sprite = art != null && art.languages != null && i < art.languages.Length ? art.languages[i] : null;
                SetArt(button, label, sprite, Loc.NativeName(lang).ToUpperInvariant(), new Color(0.9f, 0.45f, 0.1f));
                AddButton(button, () => PickLanguage(lang));

                Image tick = CreateImage("Tick", button.transform, art != null ? art.languageTick : null, new Color(0.2f, 0.7f, 0.2f));
                Place(tick.rectTransform, new Vector2(cell.x * 0.5f - 22f, cell.y * 0.5f - 16f), new Vector2(66f, 66f));
                tick.preserveAspect = true;
                languageTicks[i] = tick;
            }

            root.SetActive(false);
        }

        // One page: the panel picture, its painted title and the close button.
        private RectTransform BuildPage(string name, Transform parent, Sprite titleSprite, string titleWord,
            UnityEngine.Events.UnityAction onClose)
        {
            RectTransform page = CreateRect(name, parent);
            Stretch(page);
            float top = PanelH * 0.5f;

            Image panel = CreateImage("Panel", page, art != null ? art.panel : null, new Color(0.98f, 0.93f, 0.8f));
            Place(panel.rectTransform, Vector2.zero, new Vector2(PanelW, PanelH));
            panel.preserveAspect = true;
            panel.raycastTarget = true; // taps on the panel itself do not close it

            if (titleSprite != null)
            {
                Image title = CreateImage("Title", page, titleSprite, Color.white);
                Place(title.rectTransform, new Vector2(0f, top - PanelH * PopupArt.TitleFromTop),
                    new Vector2(PopupArt.TitleWidth, PopupArt.TitleHeight(titleSprite)));
                title.preserveAspect = true;
            }
            else
            {
                TMP_Text title = CreateText("Title", page, Loc.T(titleWord), Color.white, 64f);
                Place(title.rectTransform, new Vector2(0f, top - PanelH * PopupArt.TitleFromTop), new Vector2(560f, 110f));
            }

            Image close = CreateImage("Close Button", page, art != null ? art.closeButton : null, new Color(0.85f, 0.2f, 0.2f));
            Place(close.rectTransform, new Vector2(PanelW * 0.5f - PopupArt.CloseFromCorner.x, top - PopupArt.CloseFromCorner.y),
                new Vector2(PopupArt.CloseSize, PopupArt.CloseSize));
            close.preserveAspect = true;
            AddButton(close, onClose);
            return page;
        }

        // Centre of row <index> of <count> rows spread over the cream area, above the version line.
        private static float Row(int index, int count)
        {
            float top = PanelH * 0.5f - PanelH * CreamTop - 14f;
            float bottom = PanelH * 0.5f - PanelH * CreamBottom + 70f;
            float step = (top - bottom) / count;
            return top - step * (index + 0.5f);
        }

        // A button with its word painted in; a plain coloured button with a label when the art is missing.
        private static void SetArt(Image image, TMP_Text label, Sprite sprite, string word, Color fallback)
        {
            image.sprite = sprite;
            image.color = sprite != null ? Color.white : fallback;
            label.text = Loc.T(word);
            label.gameObject.SetActive(sprite == null);
        }

        private Image CreateToggle(string name, Transform parent, Vector2 position, Vector2 size, out TMP_Text label)
        {
            Image image = CreateImage(name, parent, null, Color.white);
            Place(image.rectTransform, position, size);
            image.preserveAspect = true;

            label = CreateText("Label", image.transform, name, Color.white, size.y * 0.3f);
            Stretch(label.rectTransform);
            return image;
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            RectTransform rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            return rect;
        }

        private static Image CreateImage(string name, Transform parent, Sprite sprite, Color color)
        {
            RectTransform rect = CreateRect(name, parent);
            rect.gameObject.AddComponent<CanvasRenderer>();
            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = sprite != null ? Color.white : color;
            image.raycastTarget = false;
            return image;
        }

        private TMP_Text CreateText(string name, Transform parent, string value, Color color, float size)
        {
            RectTransform rect = CreateRect(name, parent);
            rect.gameObject.AddComponent<CanvasRenderer>();
            TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            if (art != null && art.font != null)
                text.font = art.font;
            text.text = value;
            text.color = color;
            text.fontSize = size;
            text.enableAutoSizing = true;
            text.fontSizeMin = size * 0.5f;
            text.fontSizeMax = size;
            text.alignment = TextAlignmentOptions.Center;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.raycastTarget = false;
            return text;
        }

        private static Button AddButton(Image image, UnityEngine.Events.UnityAction action)
        {
            image.raycastTarget = true;
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(action);
            return button;
        }

        private static void Place(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void PlayClick()
        {
            try { AudioController.PlaySound(AudioController.Sounds.buttonSound); } catch { }
        }

        #endregion
    }
}
