using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    /// <summary>
    /// The text-free UI art (Resources/UI, see Docs/UIArt/ui_art_master_prompts.md) and the words
    /// the game writes on it. Every button is a glossy pill (green / blue / orange / red / pink /
    /// gray, or the small orange / green pills) with an optional icon on the left and a label in
    /// the player's language; titles are written centred on their ribbon. Calling a method again
    /// only updates the existing icon / label, so it is safe on every refresh or language change.
    /// </summary>
    public static class UIArt
    {
        private const string Root = "UI/";
        private static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

        // Outline of the white letters, per button colour (a darker shade of the button).
        private static readonly Dictionary<string, Color32> Outlines = new Dictionary<string, Color32>
        {
            { "green", new Color32(22, 92, 14, 255) },
            { "blue", new Color32(14, 46, 120, 255) },
            { "orange", new Color32(124, 46, 6, 255) },
            { "red", new Color32(112, 14, 14, 255) },
            { "pink", new Color32(70, 12, 60, 255) },
            { "gray", new Color32(70, 70, 76, 255) },
        };

        public static readonly Color32 TitleFace = new Color32(255, 250, 236, 255);
        public static readonly Color32 BlueTitleOutline = new Color32(16, 42, 110, 255);
        public static readonly Color32 RedTitleOutline = new Color32(110, 20, 20, 255);
        public static readonly Color32 GoldTitleOutline = new Color32(120, 60, 8, 255);

        public static Sprite Get(string path)
        {
            if (cache.TryGetValue(path, out Sprite s) && s != null)
                return s;
            s = Resources.Load<Sprite>(Root + path);
            cache[path] = s;
            return s;
        }

        public static Sprite Pill(string colour) => Get("Buttons/pill_" + colour);
        public static Sprite Small(string colour) => Get("Buttons/small_" + colour);
        public static Sprite Icon(string name) => string.IsNullOrEmpty(name) ? null : Get("Icons/icon_" + name);
        public static Sprite Panel(string name) => Get("Panels/" + name);

        public static Color32 OutlineFor(string colour) =>
            Outlines.TryGetValue(colour, out Color32 c) ? c : new Color32(40, 40, 40, 255);

        /// <summary>
        /// Makes <paramref name="image"/> a <paramref name="colour"/> pill with <paramref name="icon"/>
        /// on the left and the translated <paramref name="textKey"/>. Fixed-size buttons are given the
        /// pill's own shape (same width), so the art is never squashed.
        /// </summary>
        public static TextMeshProUGUI Button(Image image, string colour, string icon, string textKey, bool small = false)
        {
            if (image == null)
                return null;
            Sprite pill = small ? Small(colour) : Pill(colour);
            if (pill != null)
            {
                image.sprite = pill;
                image.type = Image.Type.Simple;
                image.preserveAspect = true;
                image.color = Color.white;
                FitHeight(image.rectTransform, pill);
            }
            HideOldLabels(image.transform);

            float aspect = pill != null ? pill.rect.width / pill.rect.height : 3.8f;
            Sprite iconSprite = Icon(icon);
            Image iconImage = Child<Image>(image.transform, "Art Icon");
            float labelLeft = 0.08f;
            if (iconSprite != null)
            {
                iconImage.gameObject.SetActive(true);
                iconImage.sprite = iconSprite;
                iconImage.preserveAspect = true;
                iconImage.raycastTarget = false;
                // a square as tall as 70% of the pill, a little in from the left end
                float h = 0.70f, w = h / aspect;
                RectTransform r = iconImage.rectTransform;
                r.anchorMin = new Vector2(0.075f, 0.5f - h * 0.5f);
                r.anchorMax = new Vector2(0.075f + w, 0.5f + h * 0.5f);
                r.offsetMin = r.offsetMax = Vector2.zero;
                labelLeft = 0.075f + w + 0.02f;
            }
            else if (iconImage != null)
            {
                iconImage.gameObject.SetActive(false);
            }

            TextMeshProUGUI label = Label(image.transform, "Art Label", textKey, OutlineFor(colour), Color.white);
            RectTransform lr = label.rectTransform;
            lr.anchorMin = new Vector2(labelLeft, 0.14f);
            lr.anchorMax = new Vector2(iconSprite != null ? 0.93f : 0.92f, 0.86f);
            lr.offsetMin = lr.offsetMax = Vector2.zero;
            return label;
        }

        /// <summary>A title written on a ribbon picture; <paramref name="area"/> is the word band (0-1 of the picture).</summary>
        // Title pictures with the word painted in (Resources/UI/Panels): used as they are when present.
        private static readonly Dictionary<string, string> PaintedTitles = new Dictionary<string, string>
        {
            { "MENU", "title_menu" }, { "SETTINGS", "title_settings" }, { "LANGUAGE", "title_language" },
            { "LEVEL COMPLETED", "level_completed" }, { "LEVEL FAILED", "level_failed" },
            { "COUNTRY COMPLETED!", "title_country_completed" }, { "NEXT COUNTRY", "title_next_country" },
        };

        public static TextMeshProUGUI Title(Image ribbon, Sprite art, string textKey, Rect area, Color32 outline)
        {
            if (ribbon == null)
                return null;
            Sprite painted = PaintedTitles.TryGetValue(textKey, out string file) ? Panel(file) : null;
            if (painted != null)
            {
                ribbon.sprite = painted;
                ribbon.preserveAspect = true;
                ribbon.color = Color.white;
                FitHeight(ribbon.rectTransform, painted);
                Transform written = ribbon.transform.Find("Art Title");
                if (written != null)
                    written.gameObject.SetActive(false);
                return null;
            }
            if (art != null)
            {
                ribbon.sprite = art;
                ribbon.preserveAspect = true;
                ribbon.color = Color.white;
                FitHeight(ribbon.rectTransform, art);
            }
            HideOldLabels(ribbon.transform);
            TextMeshProUGUI label = Label(ribbon.transform, "Art Title", textKey, outline, TitleFace);
            RectTransform r = label.rectTransform;
            r.anchorMin = new Vector2(area.xMin, 1f - area.yMax);
            r.anchorMax = new Vector2(area.xMax, 1f - area.yMin);
            r.offsetMin = r.offsetMax = Vector2.zero;
            return label;
        }

        // Word bands of the ribbons (fractions of the picture, origin top-left), measured on the art.
        public static readonly Rect BlueRibbonArea = new Rect(0.22f, 0.20f, 0.56f, 0.42f);       // title_ribbon_blue
        public static readonly Rect CountryTitleArea = new Rect(0.15f, 0.12f, 0.70f, 0.40f);     // country_title_blank
        public static readonly Rect NextCountryArea = new Rect(0.18f, 0.27f, 0.64f, 0.44f);      // next_country_blank
        public static readonly Rect LevelCompletedArea = new Rect(0.15f, 0.268f, 0.70f, 0.078f); // level_completed_blank
        public static readonly Rect LevelFailedArea = new Rect(0.13f, 0.255f, 0.74f, 0.070f);    // level_failed_blank

        private static TextMeshProUGUI Label(Transform parent, string name, string textKey, Color32 outline, Color32 face)
        {
            TextMeshProUGUI label = Child<TextMeshProUGUI>(parent, name);
            if (label == null)
            {
                label = MenuUI.OutlinedText(name, parent, textKey, 120f, outline, TextAlignmentOptions.Center, face);
                label.fontSizeMin = 14f;
                label.fontSizeMax = 120f;
                label.enableAutoSizing = true;
                label.textWrappingMode = TextWrappingModes.NoWrap;
                label.overflowMode = TextOverflowModes.Overflow;
            }
            label.gameObject.SetActive(true);
            label.text = Loc.Upper(Loc.T(textKey));
            return label;
        }

        private static T Child<T>(Transform parent, string name) where T : Component
        {
            Transform t = parent.Find(name);
            if (t != null)
                return t.GetComponent<T>();
            if (typeof(T) != typeof(Image))
                return null;
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.layer = parent.gameObject.layer;
            go.transform.SetParent(parent, false);
            return go.GetComponent<T>();
        }

        // Old text children (labels drawn when the picture had no words) stay hidden.
        private static void HideOldLabels(Transform parent)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform c = parent.GetChild(i);
                if (c.name.StartsWith("Art "))
                    continue;
                if (c.GetComponent<TMP_Text>() != null || c.GetComponent<Text>() != null)
                    c.gameObject.SetActive(false);
            }
        }

        // A fixed-size (not stretched) rect keeps its width and takes the picture's height.
        private static void FitHeight(RectTransform rect, Sprite sprite)
        {
            if (rect.anchorMin != rect.anchorMax || sprite == null)
                return;
            float aspect = sprite.rect.width / Mathf.Max(1f, sprite.rect.height);
            Vector2 size = rect.sizeDelta;
            if (size.x > 1f)
                rect.sizeDelta = new Vector2(size.x, size.x / aspect);
        }
    }
}
