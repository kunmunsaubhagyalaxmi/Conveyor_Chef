using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    /// <summary>
    /// RESTART LEVEL? popup, in the game's panel style. Its frame picture keeps its proportions, so
    /// it is drawn shorter than its 800x1000 box: the title sat on the frame's top border, and the
    /// close button (which had the CONTINUE picture squeezed into it) floated outside the frame.
    /// The title now sits in the menu panels' title ribbon on the frame's drawn top edge, and the
    /// close button is the panels' red X in the drawn top-right corner. Added by GameSceneHUDController.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class QuitPopupArt : MonoBehaviour
    {
        private const float RibbonWidth = 600f;
        private const float RibbonHeight = 200f;      // the blue ribbon's proportions (1953x649)
        private const float CloseSize = 110f;
        private const float BakedWidth = 620f;

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

            // where the frame picture is really drawn inside its box (centred, as preserveAspect draws it)
            Vector2 box = panel.rect.size;
            float aspect = frame.sprite.rect.width / frame.sprite.rect.height;
            Vector2 drawn = box;
            if (frame.preserveAspect)
                drawn = box.x / box.y > aspect ? new Vector2(box.y * aspect, box.y) : new Vector2(box.x, box.x / aspect);
            Vector2 centre = Vector2.zero;      // children below are anchored to the box's centre
            float top = centre.y + drawn.y * 0.5f;
            float right = centre.x + drawn.x * 0.5f;

            // title in a ribbon across the drawn top edge
            RectTransform title = panel.Find("Quit Text") as RectTransform;
            // the title with its words painted in, when it has been made
            Sprite baked = PopupArt.BakedTitle("restart_level");
            if (baked != null && title != null)
            {
                RectTransform header = new GameObject("Title", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<RectTransform>();
                header.SetParent(panel, false);
                header.SetSiblingIndex(title.GetSiblingIndex());
                Image headerImage = header.GetComponent<Image>();
                headerImage.sprite = baked;
                headerImage.preserveAspect = true;
                headerImage.raycastTarget = false;
                header.anchorMin = header.anchorMax = header.pivot = new Vector2(0.5f, 0.5f);
                header.sizeDelta = new Vector2(BakedWidth, BakedWidth * baked.rect.height / baked.rect.width);
                header.anchoredPosition = new Vector2(centre.x, top - 14f);
                title.gameObject.SetActive(false);
                title = null;
            }

            Sprite ribbonSprite = PopupArt.TitleRibbon(art);
            if (ribbonSprite != null && title != null)
            {
                RectTransform ribbon = new GameObject("Title Ribbon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<RectTransform>();
                ribbon.SetParent(panel, false);
                ribbon.SetSiblingIndex(title.GetSiblingIndex());
                Image ribbonImage = ribbon.GetComponent<Image>();
                ribbonImage.sprite = ribbonSprite;
                ribbonImage.preserveAspect = true;
                ribbonImage.raycastTarget = false;
                ribbon.anchorMin = ribbon.anchorMax = ribbon.pivot = new Vector2(0.5f, 0.5f);
                ribbon.sizeDelta = new Vector2(RibbonWidth, RibbonHeight);
                ribbon.anchoredPosition = new Vector2(centre.x, top - 14f);

                // the words on the ribbon's blue band
                Vector4 inset = PopupArt.RibbonTextInsets;
                title.SetParent(ribbon, false);
                title.anchorMin = Vector2.zero;
                title.anchorMax = Vector2.one;
                title.pivot = new Vector2(0.5f, 0.5f);
                title.offsetMin = new Vector2(inset.x * RibbonWidth, inset.y * RibbonHeight);
                title.offsetMax = new Vector2(-inset.z * RibbonWidth, -inset.w * RibbonHeight);
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

            // the red X in the drawn top-right corner
            RectTransform close = panel.Find("Close Button") as RectTransform;
            Image closeImage = close != null ? close.GetComponent<Image>() : null;
            if (art != null && art.closeButton != null && closeImage != null)
            {
                closeImage.sprite = art.closeButton;
                closeImage.preserveAspect = true;
                close.anchorMin = close.anchorMax = close.pivot = new Vector2(0.5f, 0.5f);
                close.sizeDelta = new Vector2(CloseSize, CloseSize);
                close.anchoredPosition = new Vector2(right - 30f, top - 30f);
            }

            return true;
        }
    }
}
