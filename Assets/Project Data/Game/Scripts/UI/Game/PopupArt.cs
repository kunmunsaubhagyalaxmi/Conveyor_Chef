using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// The popups' header and buttons, in the game's style: a blue title ribbon with gold scrolls
    /// (matches the blue and gold popup frames) and glossy gold-rimmed buttons like the main menu's,
    /// all without words (the game writes them). From Resources/UI; the menu panels' art is the
    /// fallback.
    /// </summary>
    public static class PopupArt
    {
        private static Sprite titleRibbon, buttonOrange, buttonGreen;

        public static Sprite TitleRibbon(MenuPanelArt fallback) => Get(ref titleRibbon, "UI/Panels/title_ribbon_blue", fallback != null ? fallback.titleRibbon : null);

        public static Sprite ButtonOrange(MenuPanelArt fallback) => Get(ref buttonOrange, "UI/Buttons/small_orange", fallback != null ? fallback.buttonOrange : null);

        public static Sprite ButtonGreen(MenuPanelArt fallback) => Get(ref buttonGreen, "UI/Buttons/small_green", fallback != null ? fallback.buttonGreen : null);

        /// <summary>
        /// A popup title with its words painted in (Resources/UI/Panels/title_&lt;key&gt;.png, in the
        /// style of the SETTINGS title), or null while it has not been made: then the blue ribbon
        /// with the words written by the game is used.
        /// </summary>
        public static Sprite BakedTitle(string key) => Resources.Load<Sprite>("UI/Panels/title_" + key);

        /// <summary>Where the words go on the ribbon: insets from left, bottom, right, top as shares of its size.</summary>
        public static readonly Vector4 RibbonTextInsets = new Vector4(0.2f, 0.36f, 0.2f, 0.2f);

        public static readonly Color RibbonOutline = new Color32(16, 52, 128, 255);

        // ------------------------------------------------------------ one layout for every popup
        // The in-game MENU's: the pause_popup_panel frame (768x1024, blue arch on top, cream area
        // inside) 800 wide, the painted title 620 wide on the arch, the red X on the top-right
        // corner and the content in the cream area. MENU, SETTINGS / LANGUAGE, RESTART LEVEL?,
        // LEVEL UP!, NEW POWER-UP! and OUT OF POWER-UPS all use it, so they look like one set.
        public const float PanelWidth = 800f;
        public const float PanelHeight = PanelWidth * 1024f / 768f;
        public const float TitleWidth = 620f;
        public const float TitleFromTop = 0.085f;                       // the title's centre, share of the height
        public const float CreamTop = 0.189f, CreamBottom = 0.895f;     // shares of the height, from the top
        public const float CreamWidth = 0.76f;                          // 11.8-87.9% of the width
        public const float CloseSize = 110f;
        public static readonly Vector2 CloseFromCorner = new Vector2(55f, 58f);   // the X's centre, in from the top-right corner

        /// <summary>Height of a title drawn <see cref="TitleWidth"/> wide.</summary>
        public static float TitleHeight(Sprite title) =>
            title != null ? TitleWidth * title.rect.height / title.rect.width : TitleWidth / 3f;

        private static Sprite panel;

        /// <summary>The popup frame (the MENU's), or the menu panels' frame when it is missing.</summary>
        public static Sprite Panel(MenuPanelArt fallback)
        {
            if (panel == null)
            {
                SettingsPanelArt settings = Resources.Load<SettingsPanelArt>(SettingsPanelArt.ResourcePath);
                panel = settings != null ? settings.panel : null;
            }
            return panel != null ? panel : (fallback != null ? fallback.frame : null);
        }

        private static Sprite Get(ref Sprite cached, string path, Sprite fallback)
        {
            if (cached == null)
                cached = Resources.Load<Sprite>(path);
            return cached != null ? cached : fallback;
        }
    }
}
