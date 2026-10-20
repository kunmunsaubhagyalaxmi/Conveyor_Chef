using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.BusStop;

namespace Watermelon
{
    /// <summary>
    /// In-game pause menu, opened by the HUD pause button and the grid button on the Game
    /// toolbar. Gameplay is frozen (Time.timeScale = 0) while it is open.
    ///
    /// In the original Game UI the grid button opened the settings panel of UI Main Menu
    /// (sound / vibration) together with the replay button. That panel lives on a page
    /// that is hidden during gameplay, so this popup offers the same options on the Game
    /// Main Canvas: Resume, Sound, Vibration, Restart (the existing quit/replay
    /// confirmation) and Home (Level Selection).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameMenuPopup : MonoBehaviour
    {
        [SerializeField] Button openButton;
        [SerializeField] GameObject root;
        [SerializeField] Button backgroundButton;
        [SerializeField] Button resumeButton;
        [SerializeField] Button soundButton;
        [SerializeField] TMP_Text soundLabel;
        [SerializeField] Button vibrationButton;
        [SerializeField] TMP_Text vibrationLabel;
        [SerializeField] Button restartButton;
        [SerializeField] Button homeButton;
        [Tooltip("The HUD pause button (opens this menu through UIGame).")]
        [SerializeField] Button pauseButton;

        [Header("Toggle Art")]
        [SerializeField] Sprite soundOnSprite;
        [SerializeField] Sprite soundOffSprite;
        [SerializeField] Sprite vibrationOnSprite;
        [SerializeField] Sprite vibrationOffSprite;

        public bool IsOpen => root != null && root.activeSelf;

        private bool paused;
        private float timeScaleBeforePause = 1f;

        private void Awake()
        {
            // The HUD pause button was saved in Game.unity with its Button component switched
            // off, so it drew but ignored every tap. Both open buttons are forced usable here.
            MakeUsable(pauseButton);
            MakeUsable(openButton);

            Wire(openButton, Open);
            Wire(pauseButton, Open);      // the HUD pause button pauses with this menu
            Wire(backgroundButton, Close);
            Wire(resumeButton, Close);
            Wire(soundButton, ToggleSound);
            Wire(vibrationButton, ToggleVibration);
            Wire(restartButton, Restart);
            Wire(homeButton, Home);

            if (root != null)
                root.SetActive(false);
        }

        public void Open()
        {
            if (root == null || root.activeSelf)
                return;

            PlayClick();
            RefreshLabels();
            Layout();
            root.SetActive(true);
            root.transform.SetAsLastSibling();
            RaycastController.Disable();
            Pause();

            Debug.Log("[GameMenu] Opened");
        }

        public void Close()
        {
            if (root == null || !root.activeSelf)
                return;

            PlayClick();
            root.SetActive(false);
            Resume();

            if (GameController.IsGameActive)
                RaycastController.Enable();
        }

        private void Pause()
        {
            if (paused)
                return;
            paused = true;
            timeScaleBeforePause = Time.timeScale > 0f ? Time.timeScale : 1f;
            Time.timeScale = 0f;
        }

        private void Resume()
        {
            if (!paused)
                return;
            paused = false;
            Time.timeScale = timeScaleBeforePause;
        }

        private void OnDisable()
        {
            Resume();
        }

        private void OnDestroy()
        {
            Resume();
        }

        private void ToggleSound()
        {
            bool enabled = AudioController.GetVolume() > 0.001f;
            AudioController.SetVolume(enabled ? 0f : 1f);
            PlayClick();
            RefreshLabels();
        }

        private void ToggleVibration()
        {
            AudioController.SetVibrationState(!AudioController.IsVibrationEnabled());
            PlayClick();
            RefreshLabels();
        }

        /// <summary>
        /// Ends a pause that outlived the menu (the "Restart?" confirmation keeps the game
        /// frozen until it is answered).
        /// </summary>
        public void ReleasePause()
        {
            Resume();

            if (GameController.IsGameActive && (root == null || !root.activeSelf))
                RaycastController.Enable();
        }

        private void Restart()
        {
            // Hide the menu but stay paused: the game must not run behind the confirmation.
            PlayClick();
            if (root != null)
                root.SetActive(false);

            // "Restart?" confirmation, then the level is replayed.
            UIGame game = GetComponentInParent<UIGame>();
            if (game == null)
                game = UIController.GetPage<UIGame>();
            if (game != null)
                game.ShowReplayConfirmation();
            else
                ReleasePause();
        }

        private void Home()
        {
            root.SetActive(false);
            Resume();
            PlayClick();
            GameController.ReturnToLevelSelection();
        }

        // ------------------------------------------------------------ layout

        // Panel art (pause_popup_panel.png, 768x1024): the cream area is 18.9-89.5% of its
        // height and 11.8-87.9% of its width; MENU sits on the blue band at the top.
        private const float PanelWidth = 800f;
        private const float PanelAspect = 768f / 1024f;
        private const float CreamTop = 0.189f, CreamBottom = 0.895f;
        private const float TitleWidth = 620f, TitleFromTop = 0.085f;
        private const float ButtonWidth = 450f;                 // Continue, Sound, Vibration, Restart
        private const float HomeSize = 130f, HomeFromTop = 0.915f; // round Home over the bottom edge of the cream area

        /// <summary>
        /// Lays the menu out: Continue, Sound, Vibration and Restart at one width with equal gaps
        /// in the cream area, and the round Home button centred on the panel's bottom frame (as
        /// on the other popups). Sizes follow each picture's own shape, so nothing is stretched.
        /// Only positions and sizes change (anchors stay), so the DesignFrame fit is kept.
        /// </summary>
        private void Layout()
        {
            if (root == null)
                return;
            RectTransform panel = root.transform.Find("Panel") as RectTransform;
            if (panel == null)
                return;
            RectTransform title = root.transform.Find("Menu Title") as RectTransform;

            Vector2 c = panel.anchoredPosition;
            float panelH = PanelWidth / PanelAspect;
            panel.sizeDelta = new Vector2(PanelWidth, panelH);
            float top = c.y + panelH * 0.5f;

            float start = c.y + panelH * (0.5f - CreamTop);
            if (title != null)
            {
                float titleH = TitleWidth / Aspect(title);
                Place(title, c.x, top - panelH * TitleFromTop, TitleWidth, titleH);
                start = Mathf.Min(start, title.anchoredPosition.y - titleH * 0.5f);
            }
            float bottom = c.y + panelH * (0.5f - CreamBottom);
            RectTransform home = Rect(homeButton);
            if (home != null)
            {
                float homeY = c.y + panelH * (0.5f - HomeFromTop);
                Place(home, c.x, homeY, HomeSize, HomeSize);
                bottom = Mathf.Max(bottom, homeY + HomeSize * 0.5f);
            }

            RectTransform[] column = { Rect(resumeButton), Rect(soundButton), Rect(vibrationButton), Rect(restartButton) };
            float content = 0f;
            int items = 0;
            foreach (RectTransform r in column)
            {
                if (r == null)
                    continue;
                content += ButtonWidth / Aspect(r);
                items++;
            }
            float gap = Mathf.Max(8f, (start - bottom - content) / (items + 1));

            float y = start - gap;
            foreach (RectTransform r in column)
            {
                if (r == null)
                    continue;
                float h = ButtonWidth / Aspect(r);
                Place(r, c.x, y - h * 0.5f, ButtonWidth, h);
                y -= h + gap;
            }

            // Home sits on the frame in front of the panel
            if (home != null)
                home.SetAsLastSibling();
        }

        private static RectTransform Rect(Button button) => button != null ? button.transform as RectTransform : null;

        private static float Aspect(RectTransform rect)
        {
            Image image = rect.GetComponent<Image>();
            if (image == null || image.sprite == null)
                return 3f;
            Rect r = image.sprite.rect;
            return r.width / Mathf.Max(1f, r.height);
        }

        private static void Place(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
        }

        // The toggle art shows the current state (SOUND ON / SOUND OFF, ...); plain
        // labels are only used when no art is assigned.
        private void RefreshLabels()
        {
            bool soundOn = AudioController.GetVolume() > 0.001f;
            bool vibrationOn = AudioController.IsVibrationEnabled();

            SetState(soundButton, soundLabel, soundOn, soundOnSprite, soundOffSprite, "SOUND");
            SetState(vibrationButton, vibrationLabel, vibrationOn, vibrationOnSprite, vibrationOffSprite, "VIBRATION");
        }

        private static void SetState(Button button, TMP_Text label, bool on, Sprite onSprite, Sprite offSprite, string name)
        {
            Sprite sprite = on ? onSprite : offSprite;
            Image image = button != null ? button.targetGraphic as Image : null;
            if (image != null && sprite != null)
                image.sprite = sprite;

            if (label != null)
            {
                label.gameObject.SetActive(sprite == null);
                label.text = name + (on ? ": ON" : ": OFF");
            }
        }

        private static void MakeUsable(Button button)
        {
            if (button == null)
                return;

            if (!button.enabled)
            {
                Debug.Log("[GameMenu] '" + button.name + "' had its Button switched off in the scene; switched on.");
                button.enabled = true;
            }

            button.interactable = true;

            if (button.targetGraphic != null)
                button.targetGraphic.raycastTarget = true;
        }

        private static void Wire(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
                return;

            button.onClick.RemoveListener(action);
            button.onClick.AddListener(action);
        }

        private static void PlayClick()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);
        }

#if UNITY_EDITOR
        public void EditorConfigure(
            Button open, GameObject menuRoot, Button background, Button resume,
            Button sound, TMP_Text soundText, Button vibration, TMP_Text vibrationText,
            Button restart, Button home, Button pause,
            Sprite soundOn, Sprite soundOff, Sprite vibrationOn, Sprite vibrationOff)
        {
            soundOnSprite = soundOn;
            soundOffSprite = soundOff;
            vibrationOnSprite = vibrationOn;
            vibrationOffSprite = vibrationOff;
            openButton = open;
            root = menuRoot;
            backgroundButton = background;
            resumeButton = resume;
            soundButton = sound;
            soundLabel = soundText;
            vibrationButton = vibration;
            vibrationLabel = vibrationText;
            restartButton = restart;
            homeButton = home;
            pauseButton = pause;
        }
#endif
    }
}
