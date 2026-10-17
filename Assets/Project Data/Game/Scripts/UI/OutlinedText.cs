using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace Watermelon
{
    /// <summary>
    /// Outlined game text. The game's Fredoka font assets are bitmap fonts, which cannot draw
    /// an outline, so outlines set on them were silently ignored. This builds (once) an SDF
    /// copy of the same Fredoka One font from its .ttf (MenuPanelArt.outlineFontSource) and
    /// gives each outlined label its own material: an outline and a soft shadow in any colour.
    /// </summary>
    public static class OutlinedText
    {
        private static TMP_FontAsset sdfFont;

        public static TMP_FontAsset Font
        {
            get
            {
                if (sdfFont != null)
                    return sdfFont;
                MenuPanelArt art = Resources.Load<MenuPanelArt>(MenuPanelArt.ResourcePath);
                if (art == null || art.outlineFontSource == null)
                    return null;
                try
                {
                    sdfFont = TMP_FontAsset.CreateFontAsset(art.outlineFontSource, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024,
                        AtlasPopulationMode.Dynamic);
                    if (sdfFont != null)
                        sdfFont.name = "Fredoka One SDF (outlined text)";
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning("[OutlinedText] Could not build the SDF font: " + ex.Message);
                }
                return sdfFont;
            }
        }

        /// <summary>
        /// Gives <paramref name="text"/> an outline (width ~0.15-0.35) and, optionally, a soft
        /// drop shadow. The face colour stays the text's colour.
        /// </summary>
        public static void Apply(TMP_Text text, Color outline, float width, bool shadow = true)
        {
            if (text == null)
                return;
            TMP_FontAsset font = Font;
            if (font == null || font.material == null)
                return;

            text.font = font;
            var material = new Material(font.material) { name = text.name + " Outline" };
            material.EnableKeyword(ShaderUtilities.Keyword_Outline);
            material.SetColor(ShaderUtilities.ID_OutlineColor, outline);
            material.SetFloat(ShaderUtilities.ID_OutlineWidth, width);
            material.SetFloat(ShaderUtilities.ID_FaceDilate, width * 0.45f);
            if (shadow)
            {
                material.EnableKeyword(ShaderUtilities.Keyword_Underlay);
                material.SetColor(ShaderUtilities.ID_UnderlayColor, new Color(0f, 0f, 0f, 0.4f));
                material.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -0.55f);
                material.SetFloat(ShaderUtilities.ID_UnderlayDilate, width * 0.5f);
                material.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 0.25f);
            }
            ShaderUtilities.UpdateShaderRatios(material);
            text.fontSharedMaterial = material;
        }
    }
}
