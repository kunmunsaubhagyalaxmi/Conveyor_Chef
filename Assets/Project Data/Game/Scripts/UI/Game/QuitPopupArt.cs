using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    /// <summary>
    /// RESTART LEVEL? popup in the shared popup layout (as the in-game MENU, see PopupArt): the
    /// MENU's frame 800 wide, the painted title on its blue arch, the red X on the top-right corner
    /// and the heart, its line and RESTART spread over the cream area. Its own frame was a
    /// different size, the title sat lower on a ribbon and the X floated outside, so it did not
    /// match the MENU it opens from. Added by GameSceneHUDController.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class QuitPopupArt : MonoBehaviour
    {
        private const float CardWidth = 560f, CardHeight = 430f;   // the heart and its line
        private const float HeartSize = 300f;
        private const float ButtonWidth = 380f;

        private bool done;

        private void OnEnable()
        {
            if (!done)
                done = Apply();
        }

        private bool Apply()
        {
            RectTransform panel = transform.Find("Panel Back") as RectTransform;
            Image frame = panel != null ? panel.GetComponent<Image>() : null;
            if (frame == null || frame.sprite == null)
                return false;

            MenuPanelArt art = Resources.Load<MenuPanelArt>(MenuPanelArt.ResourcePath);

            // the MENU's frame, at the MENU's size
            Sprite panelSprite = PopupArt.Panel(null);
            if (panelSprite != null)
            {
                frame.sprite = panelSprite;
                frame.type = Image.Type.Simple;
            }
            frame.preserveAspect = true;
            float w = PopupArt.PanelWidth, h = PopupArt.PanelHeight;
            panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(w, h);
            float top = h * 0.5f, right = w * 0.5f;      // children below are placed from the panel's centre
            float titleY = top - h * PopupArt.TitleFromTop;

            // title on the blue arch
            RectTransform title = panel.Find("Quit Text") as RectTransform;
            Sprite baked = PopupArt.BakedTitle("restart_level");
            if (baked != null && title != null)
            {
                Image header = NewImage("Title", panel, baked, title.GetSiblingIndex());
                Place((RectTransform)header.transform, 0f, titleY, PopupArt.TitleWidth, PopupArt.TitleHeight(baked));
                title.gameObject.SetActive(false);
                title = null;
            }

            Sprite ribbonSprite = PopupArt.TitleRibbon(art);
            if (ribbonSprite != null && title != null)
            {
                Vector2 size = new Vector2(PopupArt.TitleWidth, PopupArt.TitleHeight(ribbonSprite));
                RectTransform ribbon = (RectTransform)NewImage("Title Ribbon", panel, ribbonSprite, title.GetSiblingIndex()).transform;
                Place(ribbon, 0f, titleY, size.x, size.y);

                // the words on the ribbon's blue band
                Vector4 inset = PopupArt.RibbonTextInsets;
                title.SetParent(ribbon, false);
                title.anchorMin = Vector2.zero;
                title.anchorMax = Vector2.one;
                title.pivot = new Vector2(0.5f, 0.5f);
                title.offsetMin = new Vector2(inset.x * size.x, inset.y * size.y);
                title.offsetMax = new Vector2(-inset.z * size.x, -inset.w * size.y);
                TMPro.TextMeshProUGUI text = title.GetComponent<TMPro.TextMeshProUGUI>();
                if (text != null)
                {
                    text.enableAutoSizing = true;
                    text.fontSizeMax = Mathf.Max(text.fontSize, 40f);
                    text.fontSizeMin = 18f;
                    text.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
                    text.alignment = TMPro.TextAlignmentOptions.Center;
                }
            }

            // the red X on the top-right corner
            RectTransform close = panel.Find("Close Button") as RectTransform;
            Image closeImage = close != null ? close.GetComponent<Image>() : null;
            if (closeImage != null)
            {
                if (art != null && art.closeButton != null)
                    closeImage.sprite = art.closeButton;
                closeImage.preserveAspect = true;
                Place(close, right - PopupArt.CloseFromCorner.x, top - PopupArt.CloseFromCorner.y, PopupArt.CloseSize, PopupArt.CloseSize);
                close.SetAsLastSibling();
            }

            // the heart card and RESTART with equal gaps in the cream area
            float creamTop = top - h * PopupArt.CreamTop;
            float creamBottom = top - h * PopupArt.CreamBottom;
            RectTransform card = panel.Find("Life Lose Back Image") as RectTransform;
            RectTransform button = panel.Find("Quit Button") as RectTransform;
            float buttonH = ButtonWidth / Aspect(button, 2.93f);
            float gap = Mathf.Max(12f, (creamTop - creamBottom - CardHeight - buttonH) / 3f);
            if (card != null)
            {
                Place(card, 0f, creamTop - gap - CardHeight * 0.5f, CardWidth, CardHeight);
                RectTransform heart = card.Find("Life Image") as RectTransform;
                if (heart != null)
                    Place(heart, 0f, CardHeight * 0.5f - 24f - HeartSize * 0.5f, HeartSize, HeartSize);
                RectTransform line = card.Find("Lose Life Text") as RectTransform;
                if (line != null)
                    Place(line, 0f, -CardHeight * 0.5f + 20f + 50f, CardWidth - 40f, 100f);
            }
            if (button != null)
                Place(button, 0f, creamBottom + gap + buttonH * 0.5f, ButtonWidth, buttonH);

            return true;
        }

        private static Image NewImage(string name, Transform parent, Sprite sprite, int sibling)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            go.transform.SetSiblingIndex(sibling);
            Image image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            return image;
        }

        // centre-anchored, so positions are from the parent's centre
        private static void Place(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static float Aspect(RectTransform rect, float fallback)
        {
            Image image = rect != null ? rect.GetComponent<Image>() : null;
            if (image == null || image.sprite == null)
                return fallback;
            return image.sprite.rect.width / image.sprite.rect.height;
        }
    }
}
