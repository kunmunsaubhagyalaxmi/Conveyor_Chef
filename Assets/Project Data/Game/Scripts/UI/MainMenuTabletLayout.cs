using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// Main menu on tablets and other wide screens. The screen fitter (DesignFrame) keeps the
    /// design phone's frame (1440x3200) whole, so on a 3:4 tablet the menu sat in a phone-wide
    /// column in the middle with empty sides. Here the menu uses the extra width: the avatar and
    /// the counters go to the corners, the bottom icons spread out, the chef moves left and the
    /// buttons (with the logo above them) move right.
    ///
    /// Phones are left exactly as they are: nothing moves below <see cref="WideAspect"/>. Only
    /// positions are offset from what the scene and the fitter set (anchors are the fitter's).
    /// Added at runtime by MainMenuApprovedArtBinder.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(1000)]       // after the fitter and the menu scripts
    public sealed class MainMenuTabletLayout : MonoBehaviour
    {
        private const float DesignAspect = 1440f / 3200f;   // the design phone (DesignFrame)
        private const float WideAspect = 0.6f;              // narrower than this: a phone, untouched
        private const float SideShare = 0.5f;               // share of each side's extra width the chef / buttons use
        private const float NavEdgeMargin = 198f;           // bottom icon centre to the screen edge (as on the design phone)
        private const float NavMaxSpread = 1.6f;            // bottom icons never more than this much further apart

        private struct Item
        {
            public RectTransform rect;
            public Vector2 authored;
        }

        private RectTransform root;
        private Item chef, buttons, logo, avatar, namePlate, star, coin, diamond;
        private Item[] navIcons;
        private Vector2 laidOutFor = new Vector2(-1f, -1f);

        private void Awake()
        {
            root = transform as RectTransform;
            chef = Grab("Chef Character");
            buttons = Grab("Main Buttons");
            logo = Grab("Game Logo");
            avatar = Grab("Chef Avatar");
            namePlate = Grab("Player Name Plate");
            star = Grab("Star Counter");
            coin = Grab("Coin Counter");
            diamond = Grab("Diamond Counter");

            Transform nav = FindDeep(transform, "Bottom Navigation");
            navIcons = new Item[nav != null ? nav.childCount : 0];
            for (int i = 0; i < navIcons.Length; i++)
                navIcons[i] = Make(nav.GetChild(i) as RectTransform);
        }

        private void LateUpdate()
        {
            if (root == null)
                return;

            Vector2 size = root.rect.size;
            if (size == laidOutFor || size.x < 1f || size.y < 1f)
                return;
            laidOutFor = size;
            Layout(size);
        }

        private void OnDisable()
        {
            // back to the scene's positions (also lets the next enable lay out again)
            Layout(Vector2.zero);
            laidOutFor = new Vector2(-1f, -1f);
        }

        private void Layout(Vector2 size)
        {
            // Extra width beyond the design frame, in canvas units (the frame fills the height on
            // a screen wider than the design phone). Zero on phones and when restoring.
            float extra = 0f;
            if (size.y > 0f && size.x / size.y >= WideAspect)
                extra = Mathf.Max(0f, size.x - size.y * DesignAspect);
            float half = extra * 0.5f;

            // the avatar and its name plate to the left corner, the counters to the right corner
            Place(avatar, -half);
            Place(namePlate, -half);
            Place(star, half);
            Place(coin, half);
            Place(diamond, half);

            // the chef further left, the buttons and the logo above them further right
            Place(chef, -half * SideShare);
            Place(buttons, half * SideShare);
            Place(logo, half * SideShare);

            // the bottom icons spread across the wider screen, the outer ones as far from the edge
            // as on the design phone
            float designHalf = size.y * DesignAspect * 0.5f;
            float spread = extra > 0f
                ? Mathf.Clamp((size.x * 0.5f - NavEdgeMargin) / Mathf.Max(1f, designHalf - NavEdgeMargin), 1f, NavMaxSpread)
                : 1f;
            foreach (Item icon in navIcons)
            {
                if (icon.rect != null)
                    icon.rect.anchoredPosition = new Vector2(icon.authored.x * spread, icon.authored.y);
            }
        }

        private static void Place(Item item, float dx)
        {
            if (item.rect != null)
                item.rect.anchoredPosition = item.authored + new Vector2(dx, 0f);
        }

        private Item Grab(string objectName) => Make(FindDeep(transform, objectName) as RectTransform);

        private static Item Make(RectTransform rect) =>
            new Item { rect = rect, authored = rect != null ? rect.anchoredPosition : Vector2.zero };

        private static Transform FindDeep(Transform parent, string objectName)
        {
            if (parent == null)
                return null;
            if (parent.name == objectName)
                return parent;
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform found = FindDeep(parent.GetChild(i), objectName);
                if (found != null)
                    return found;
            }
            return null;
        }
    }
}
