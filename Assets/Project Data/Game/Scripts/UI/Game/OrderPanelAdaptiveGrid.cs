using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    /// <summary>
    /// Lays out the ORDERS panel for any number of orders (levels use 1-7), always inside
    /// the same content width so nothing reaches the frame's border:
    ///   1-4 orders:  one column of cards, a big dish on the left and the count on the right
    ///   5-6 orders:  two columns of cards, a big dish with the count underneath
    ///   7-10 orders: three columns in the same panel width, no card behind the dish: the
    ///                dish fills its cell and the count sits on a small dark badge on its
    ///                corner, so the food is as big as the panel allows
    /// The dish pictures have empty space around the food, so the icon is drawn a little larger
    /// than its slot (IconOverscale) to make the food itself fill the card. Cards never shrink
    /// with more orders; the frame grows instead, so the dishes stay easy to see.
    /// UIOrderPanel keeps spawning and updating the items; this only changes their layout.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(GridLayoutGroup))]
    public sealed class OrderPanelAdaptiveGrid : MonoBehaviour
    {
        // Width of the panel inside the frame's padding. Every layout fits in it.
        [SerializeField] float contentWidth = 232f;

        private const int MaxSingleColumnItems = 4;
        private const int MaxTwoColumnItems = 6;
        private const float CompactCardHeight = 92f;
        private const float CompactSpacing = 2f;
        private const float CompactRowSpacing = 10f;       // air between the rows of dishes
        private const float CompactIconOverscale = 1.14f;  // the dish picture's empty edge may overlap neighbours
        private const string CountBadgeName = "Count Badge";
        private const float CardPixelsPerUnit = 4.5f;      // the card's normal border scale
        private const float ColumnSpacing = 10f;
        private const float RowSpacing = 10f;
        private const float LargeCardHeight = 112f;
        private const float MediumCardHeight = 106f;
        private const float StackedCardHeight = 148f;

        // The card picture (order_item_slot.png) has a transparent border: the visible yellow
        // card starts this far inside the item's box. Dish and count are laid out inside the
        // visible card, so nothing pokes out of it.
        private const float CardInsetX = 15f;
        private const float CardInsetY = 16f;
        private const float CardRim = 7f;                  // gold edge inside that, around the cream
        private const float IconOverscale = 1.1f;          // dish pictures are ~80% food, 20% empty edge
        private const float StackedIconOverscale = 1.0f;     // two columns: the dish must leave room for the count

        private enum Style { Large, Medium, Stacked, Compact }

        private GridLayoutGroup grid;
        private LayoutElement layoutElement;
        private int lastCount = -1;

        private void Awake()
        {
            grid = GetComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.padding = new RectOffset(0, 0, 0, 0);

            // A fixed width keeps the frame the same size whatever the column count.
            layoutElement = GetComponent<LayoutElement>();
            if (layoutElement == null)
                layoutElement = gameObject.AddComponent<LayoutElement>();
            layoutElement.minWidth = contentWidth;
            layoutElement.preferredWidth = contentWidth;
            layoutElement.flexibleWidth = 0f;
            layoutElement.flexibleHeight = 0f;
        }

        private void LateUpdate()
        {
            int count = CountActiveItems();
            if (count != lastCount)
                Relayout(count);
        }

        /// <summary>Lays the items out now (called by UIOrderPanel after it spawns them).</summary>
        public void Refresh()
        {
            if (grid == null)
                Awake();
            Relayout(CountActiveItems());
        }

        private int CountActiveItems()
        {
            int count = 0;
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).gameObject.activeSelf)
                    count++;
            }
            return count;
        }

        private void Relayout(int count)
        {
            lastCount = count;

            Style style = count > MaxTwoColumnItems ? Style.Compact
                : count > MaxSingleColumnItems ? Style.Stacked
                : count == MaxSingleColumnItems ? Style.Medium
                : Style.Large;

            // same width for every layout: the panel never spreads over the level badge
            float width = contentWidth;
            layoutElement.minWidth = width;
            layoutElement.preferredWidth = width;

            if (style == Style.Compact)
            {
                grid.constraintCount = 3;
                grid.spacing = new Vector2(CompactSpacing, CompactRowSpacing);
                grid.cellSize = new Vector2((width - CompactSpacing * 2f) / 3f, CompactCardHeight);
            }
            else if (style == Style.Stacked)
            {
                grid.constraintCount = 2;
                grid.spacing = new Vector2(ColumnSpacing, RowSpacing);
                grid.cellSize = new Vector2((contentWidth - ColumnSpacing) * 0.5f, StackedCardHeight);
            }
            else
            {
                grid.constraintCount = 1;
                grid.spacing = new Vector2(0f, RowSpacing);
                grid.cellSize = new Vector2(contentWidth, style == Style.Medium ? MediumCardHeight : LargeCardHeight);
            }

            for (int i = 0; i < transform.childCount; i++)
                ApplyItemLayout(transform.GetChild(i), style, grid.cellSize);

            LayoutRebuilder.MarkLayoutForRebuild((RectTransform)transform);
        }

        /// <summary>Large card layout, used by the editor builder for the prefab.</summary>
        public static void ApplyItemLayout(Transform item, bool compact)
        {
            ApplyItemLayout(item, compact ? Style.Stacked : Style.Large,
                compact ? new Vector2(111f, StackedCardHeight) : new Vector2(232f, LargeCardHeight));
            // (cell sizes match Relayout: two 111-wide columns, or one 232-wide column)
        }

        private static void ApplyItemLayout(Transform item, Style style, Vector2 cell)
        {
            RectTransform icon = item.Find("Icon") as RectTransform;
            RectTransform text = item.Find("Order Text") as RectTransform;
            Image card = item.GetComponent<Image>();
            RectTransform badge = item.Find(CountBadgeName) as RectTransform;

            if (style == Style.Compact)
            {
                LayoutCompact(item, icon, text, card, ref badge, cell);
                return;
            }

            if (card != null)
                card.enabled = true;
            if (badge != null)
                badge.gameObject.SetActive(false);

            // The card picture's normal border scale (compact cells hide the card instead).
            float ppu = CardPixelsPerUnit;
            if (card != null && card.type == Image.Type.Sliced)
                card.pixelsPerUnitMultiplier = ppu;
            float insetX = CardInsetX * CardPixelsPerUnit / ppu;
            float insetY = CardInsetY * CardPixelsPerUnit / ppu;
            float rim = CardRim * CardPixelsPerUnit / ppu;     // gold edge of the card
            float innerW = cell.x - insetX * 2f;
            float innerH = cell.y - insetY * 2f;

            if (style == Style.Stacked || style == Style.Compact)
            {
                // Dish in the upper part of the visible card, count on its own line below.
                const float textHeight = 32f;
                const float overlap = 0f;
                const float gap = 2f;
                // the count sits on the cream, just above the gold bottom edge
                float slot = Mathf.Min(innerW - gap * 2f, innerH - textHeight - rim - gap * 3f + overlap);
                float iconSize = slot * StackedIconOverscale;

                if (icon != null)
                {
                    icon.anchorMin = new Vector2(0.5f, 1f);
                    icon.anchorMax = new Vector2(0.5f, 1f);
                    icon.pivot = new Vector2(0.5f, 0.5f);
                    icon.anchoredPosition = new Vector2(0f, -insetY - gap - slot * 0.5f);
                    icon.sizeDelta = new Vector2(iconSize, iconSize);
                }

                if (text != null)
                {
                    text.anchorMin = new Vector2(0f, 0f);
                    text.anchorMax = new Vector2(1f, 0f);
                    text.pivot = new Vector2(0.5f, 0f);
                    text.anchoredPosition = new Vector2(0f, insetY + rim);
                    text.sizeDelta = new Vector2(-insetX * 2f - 4f, textHeight);
                }
            }
            else
            {
                // Dish on the left of the visible card, count in the rest.
                const float gap = 3f;
                float slot = innerH - gap * 2f;
                float iconSize = slot * IconOverscale;

                if (icon != null)
                {
                    icon.anchorMin = new Vector2(0f, 0.5f);
                    icon.anchorMax = new Vector2(0f, 0.5f);
                    icon.pivot = new Vector2(0.5f, 0.5f);
                    icon.anchoredPosition = new Vector2(insetX + gap + slot * 0.5f + 4f, 0f);
                    icon.sizeDelta = new Vector2(iconSize, iconSize);
                }

                if (text != null)
                {
                    text.anchorMin = Vector2.zero;
                    text.anchorMax = Vector2.one;
                    text.pivot = new Vector2(0.5f, 0.5f);
                    text.offsetMin = new Vector2(insetX + gap + slot + 10f, insetY + 4f);
                    text.offsetMax = new Vector2(-insetX - 6f, -insetY - 4f);
                }
            }

            // The count is drawn above the dish so the dish never covers it.
            if (text != null)
                text.SetAsLastSibling();
            Transform check = item.Find("Completed Check");
            if (check != null)
                check.SetAsLastSibling();

            // Autosize may shrink far enough that the count never spills past the card, but
            // never below a size that is easy to read.
            TextMeshProUGUI label = text != null ? text.GetComponent<TextMeshProUGUI>() : null;
            if (label != null)
            {
                label.enableAutoSizing = true;
                label.fontSizeMin = style == Style.Stacked ? 22f : 16f;
                label.fontSizeMax = style == Style.Stacked ? 34f : 40f;
                label.fontStyle = FontStyles.Bold;
                label.color = new Color32(92, 52, 24, 255);
                label.alignment = TextAlignmentOptions.Center;
                label.overflowMode = TextOverflowModes.Overflow;
            }
        }
    
        // 7-10 orders: the dish fills the whole cell (no card), the count on a dark badge in
        // the lower-right corner, the tick (when done) in the upper-right corner.
        private static void LayoutCompact(Transform item, RectTransform icon, RectTransform text, Image card, ref RectTransform badge, Vector2 cell)
        {
            if (card != null)
                card.enabled = false;

            float size = Mathf.Min(cell.x, cell.y) * CompactIconOverscale;
            if (icon != null)
            {
                icon.anchorMin = icon.anchorMax = new Vector2(0.5f, 0.5f);
                icon.pivot = new Vector2(0.5f, 0.5f);
                icon.anchoredPosition = new Vector2(0f, 3f);
                icon.sizeDelta = new Vector2(size, size);
                icon.SetSiblingIndex(0);
            }

            if (badge == null)
            {
                var go = new GameObject(CountBadgeName, typeof(RectTransform));
                go.transform.SetParent(item, false);
                Image back = go.AddComponent<Image>();
                back.sprite = MenuUI.Rounded(14, 2, new Color32(60, 34, 14, 235), new Color32(255, 214, 120, 255));
                back.type = Image.Type.Sliced;
                back.raycastTarget = false;
                badge = (RectTransform)go.transform;
            }
            badge.gameObject.SetActive(true);
            badge.anchorMin = badge.anchorMax = new Vector2(1f, 0f);
            badge.pivot = new Vector2(1f, 0f);
            badge.anchoredPosition = new Vector2(-1f, 1f);
            badge.sizeDelta = new Vector2(Mathf.Min(48f, cell.x * 0.62f), 24f);
            badge.SetAsLastSibling();

            if (text != null)
            {
                text.anchorMin = text.anchorMax = new Vector2(1f, 0f);
                text.pivot = new Vector2(1f, 0f);
                text.anchoredPosition = badge.anchoredPosition;
                text.sizeDelta = badge.sizeDelta;
                text.SetAsLastSibling();
                TextMeshProUGUI label = text.GetComponent<TextMeshProUGUI>();
                if (label != null)
                {
                    label.enableAutoSizing = true;
                    label.fontSizeMin = 14f;
                    label.fontSizeMax = 20f;
                    label.fontStyle = FontStyles.Bold;
                    label.color = Color.white;
                    label.alignment = TextAlignmentOptions.Center;
                    label.overflowMode = TextOverflowModes.Overflow;
                }
            }

            Transform check = item.Find("Completed Check");
            if (check != null)
                check.SetAsLastSibling();
        }
}
}
