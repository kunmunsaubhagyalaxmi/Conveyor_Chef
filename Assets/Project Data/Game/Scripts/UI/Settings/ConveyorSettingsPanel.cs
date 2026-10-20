using System.Collections.Generic;
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
    ///
    /// Panel and close button from Resources/SettingsPanelArt; the title ribbon, buttons and
    /// icons are the text-free art (UIArt): SOUND / MUSIC / VIBRATION are orange pills when on,
    /// gray when off, LANGUAGE opens a list of the languages (small pills, the chosen one green).
    /// Every word is written by the game in the player's language.
    /// </summary>
    public sealed class ConveyorSettingsPanel : MonoBehaviour
    {
        private const int SortingOrder = 5000;

        // Layout at 1080x1920 on the popup panel art (768x1024: blue band at the top, cream area
        // 19-88% of its height).
        private const float PanelWidth = 700f;
        private const float PanelHeight = PanelWidth * 1024f / 768f;
        private const float CreamTop = 0.19f, CreamBottom = 0.88f;
        private const float TitleWidth = 560f, TitleFromTop = 0.095f;
        private const float ButtonWidth = 470f;
        private const float OptionWidth = 262f;

        private static ConveyorSettingsPanel instance;

        private GameObject root;
        private Image titleImage;
        private Image soundImage, musicImage, vibrationImage, languageImage;
        private TMP_Text versionLabel;
        private GameObject picker;
        private Image pickerTitle;
        private readonly List<(GameLanguage language, Image image)> languageButtons = new List<(GameLanguage, Image)>();
        private SettingsPanelArt art;

        public static bool IsOpen => instance != null && instance.root != null && instance.root.activeSelf;

        public static void Show()
        {
            EnsureInstance();
            instance.Open();
        }

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
            Loc.Changed += Refresh;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Loc.Changed -= Refresh;
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
            PlayClick();
            Refresh();
            root.SetActive(true);
        }

        private void Close(bool playSound)
        {
            if (root == null || !root.activeSelf)
                return;

            if (playSound)
                PlayClick();

            if (picker != null)
                picker.SetActive(false);
            root.SetActive(false);
        }

        // ------------------------------------------------------------ toggles

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
            if (root == null)
                return;
            Toggle(soundImage, IsSoundOn(), "sound", "SOUND ON", "SOUND OFF");
            Toggle(musicImage, MusicSetting.On, "music", "MUSIC ON", "MUSIC OFF");
            Toggle(vibrationImage, IsVibrationOn(), "vibration", "VIBRATION ON", "VIBRATION OFF");

            TMP_Text language = UIArt.Button(languageImage, "orange", "language", "LANGUAGE");
            if (language != null)
                language.text = Loc.NativeName(Loc.Current).ToUpperInvariant() == "ENGLISH"
                    ? Loc.Upper(Loc.T("LANGUAGE"))
                    : Loc.Upper(Loc.T("LANGUAGE")) + ": " + Loc.Upper(Loc.NativeName(Loc.Current));

            UIArt.Title(titleImage, UIArt.Panel("title_ribbon_blue"), "SETTINGS", UIArt.BlueRibbonArea, UIArt.BlueTitleOutline);
            UIArt.Title(pickerTitle, UIArt.Panel("title_ribbon_blue"), "LANGUAGE", UIArt.BlueRibbonArea, UIArt.BlueTitleOutline);

            if (versionLabel != null)
                versionLabel.text = Loc.T("VERSION") + " " + Application.version;

            foreach (var entry in languageButtons)
            {
                TMP_Text label = UIArt.Button(entry.image, entry.language == Loc.Current ? "green" : "orange", null, "", small: true);
                if (label != null)
                    label.text = Loc.NativeName(entry.language);
            }
        }

        private static void Toggle(Image image, bool on, string icon, string onKey, string offKey)
        {
            UIArt.Button(image, on ? "orange" : "gray", icon + (on ? "_on" : "_off"), on ? onKey : offKey);
        }

        #region Build

        private void Build(SettingsPanelArt panelArt)
        {
            art = panelArt;
            if (art == null)
                Debug.LogWarning("[Settings] Resources/" + SettingsPanelArt.ResourcePath + " not found; using plain shapes.");
            MenuPanelArt menuArt = Resources.Load<MenuPanelArt>(MenuPanelArt.ResourcePath);
            if (MenuUI.Font == null && menuArt != null)
                MenuUI.Font = menuArt.font;

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

            float top = BuildPanel(rootRect, out titleImage, () => Close(playSound: true));

            // SOUND, MUSIC, VIBRATION, LANGUAGE: one width, equal gaps, the version line under them
            soundImage = CreateButton("Sound Button", rootRect, ToggleSound);
            musicImage = CreateButton("Music Button", rootRect, ToggleMusic);
            vibrationImage = CreateButton("Vibration Button", rootRect, ToggleVibration);
            languageImage = CreateButton("Language Button", rootRect, OpenPicker);
            versionLabel = CreateText("Version", rootRect, "VERSION " + Application.version, new Color32(35, 48, 70, 255), 34f);

            Image[] column = { soundImage, musicImage, vibrationImage, languageImage };
            Sprite pill = UIArt.Pill("orange");
            float h = pill != null ? ButtonWidth * pill.rect.height / pill.rect.width : 124f;
            const float versionH = 50f;
            float start = top, bottom = PanelHeight * 0.5f - PanelHeight * CreamBottom;
            float gap = Mathf.Max(8f, (start - bottom - column.Length * h - versionH) / (column.Length + 2));
            float y = start - gap;
            foreach (Image image in column)
            {
                Place(image.rectTransform, new Vector2(0f, y - h * 0.5f), new Vector2(ButtonWidth, h));
                y -= h + gap;
            }
            Place(versionLabel.rectTransform, new Vector2(0f, y - versionH * 0.5f), new Vector2(500f, versionH));

            BuildPicker(rootRect);
            root.SetActive(false);
        }

        /// <summary>Dim-free panel art + title ribbon + close X; returns the y where content can start.</summary>
        private float BuildPanel(RectTransform parent, out Image title, UnityEngine.Events.UnityAction close)
        {
            float top = PanelHeight * 0.5f;
            Image panel = CreateImage("Panel", parent, art != null ? art.panel : null, new Color(0.98f, 0.93f, 0.8f));
            Place(panel.rectTransform, Vector2.zero, new Vector2(PanelWidth, PanelHeight));
            panel.preserveAspect = true;
            panel.raycastTarget = true; // taps on the panel itself do not close it

            Sprite ribbon = UIArt.Panel("title_ribbon_blue");
            float titleH = ribbon != null ? TitleWidth * ribbon.rect.height / ribbon.rect.width : 180f;
            title = CreateImage("Title", parent, ribbon, Color.white);
            title.preserveAspect = true;
            float titleY = top - PanelHeight * TitleFromTop;
            Place(title.rectTransform, new Vector2(0f, titleY), new Vector2(TitleWidth, titleH));

            Image x = CreateImage("Close Button", parent, art != null ? art.closeButton : null, new Color(0.85f, 0.2f, 0.2f));
            Place(x.rectTransform, new Vector2(PanelWidth * 0.5f - 44f, top - 50f), new Vector2(100f, 100f));
            x.preserveAspect = true;
            AddButton(x, close);

            float creamTop = top - PanelHeight * CreamTop;
            return Mathf.Min(creamTop, titleY - titleH * 0.5f);
        }

        // ------------------------------------------------------------ language picker

        // The languages in two columns; the chosen one is green.
        private void BuildPicker(RectTransform parent)
        {
            RectTransform rect = CreateRect("Language Picker", parent);
            Stretch(rect);
            picker = rect.gameObject;

            Image dim = CreateImage("Dim", rect, null, new Color(0.03f, 0.05f, 0.1f, 0.6f));
            Stretch(dim.rectTransform);
            AddButton(dim, ClosePicker).transition = Selectable.Transition.None;

            float start = BuildPanel(rect, out pickerTitle, ClosePicker);

            GameLanguage[] languages = Loc.All;
            Sprite small = UIArt.Small("orange");
            float h = small != null ? OptionWidth * small.rect.height / small.rect.width : 88f;
            int rows = (languages.Length + 1) / 2;
            float bottom = PanelHeight * 0.5f - PanelHeight * CreamBottom;
            float gap = Mathf.Max(8f, (start - bottom - rows * h) / (rows + 1));
            for (int i = 0; i < languages.Length; i++)
            {
                GameLanguage language = languages[i];
                float x = (i % 2 == 0 ? -1f : 1f) * (OptionWidth * 0.5f + 12f);
                float y = start - gap - (i / 2) * (h + gap) - h * 0.5f;
                Image image = CreateButton(language.ToString(), rect, () => ChooseLanguage(language));
                Place(image.rectTransform, new Vector2(x, y), new Vector2(OptionWidth, h));
                languageButtons.Add((language, image));
            }

            picker.SetActive(false);
        }

        private void OpenPicker()
        {
            Refresh();
            if (picker != null)
            {
                picker.SetActive(true);
                picker.transform.SetAsLastSibling();
            }
        }

        private void ClosePicker()
        {
            if (picker != null)
                picker.SetActive(false);
        }

        private void ChooseLanguage(GameLanguage language)
        {
            Loc.Set(language);
            Refresh();
            ClosePicker();
        }

        // ------------------------------------------------------------ helpers

        private Image CreateButton(string name, Transform parent, UnityEngine.Events.UnityAction action)
        {
            Image image = CreateImage(name, parent, null, Color.white);
            image.preserveAspect = true;
            AddButton(image, action);
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
