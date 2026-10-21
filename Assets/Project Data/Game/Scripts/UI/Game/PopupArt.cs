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

        /// <summary>Where the words go on the ribbon: insets from left, bottom, right, top as shares of its size.</summary>
        public static readonly Vector4 RibbonTextInsets = new Vector4(0.2f, 0.36f, 0.2f, 0.2f);

        public static readonly Color RibbonOutline = new Color32(16, 52, 128, 255);

        private static Sprite Get(ref Sprite cached, string path, Sprite fallback)
        {
            if (cached == null)
                cached = Resources.Load<Sprite>(path);
            return cached != null ? cached : fallback;
        }
    }
}
