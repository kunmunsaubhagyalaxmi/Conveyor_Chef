using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// A designed panel picture with its title ribbon and close button drawn in, in Resources as
    /// "MenuPanelFrames/&lt;key&gt;" (story, challenges, customize, shop, collection, achievements,
    /// leaderboard). The panel keeps the picture's proportions, fills its cream area with the
    /// content and puts the close hit area over the drawn X. Panels without a picture use the
    /// shared frame from MenuPanelArt.
    /// </summary>
    public sealed class MenuPanelFrame
    {
        public const string ResourceFolder = "MenuPanelFrames/";

        /// <summary>
        /// One frame picture with an EMPTY title ribbon (Resources/MenuPanelFrames/blank.png).
        /// When it is there every panel uses it and writes its own title on the ribbon, in the
        /// player's language; without it each panel keeps its own picture with the title drawn in.
        /// </summary>
        public const string BlankKey = "blank";

        // Measured in the picture's pixels, origin top-left.
        private struct Layout
        {
            public RectInt Cream;          // the cream content area
            public Vector3 Close;          // x, y = centre of the X button, z = radius
            public RectInt Title;          // where the title is written (blank frame only)

            public Layout(int left, int top, int right, int bottom, int closeX, int closeY, int closeRadius)
            {
                Cream = new RectInt(left, top, right - left, bottom - top);
                Close = new Vector3(closeX, closeY, closeRadius);
                Title = default;
            }

            public Layout WithTitle(int left, int top, int right, int bottom)
            {
                Layout copy = this;
                copy.Title = new RectInt(left, top, right - left, bottom - top);
                return copy;
            }
        }

        private static readonly Dictionary<string, Layout> Layouts = new Dictionary<string, Layout>
        {
            { "story",        new Layout(71, 236, 866, 1567, 877, 128, 46) },
            { "challenges",   new Layout(71, 222, 870, 1576, 874, 119, 56) },
            { "collection",   new Layout(71, 222, 870, 1576, 874, 119, 56) },   // the challenges picture, retitled
            { "customize",    new Layout(74, 241, 867, 1549, 874, 137, 58) },
            { "shop",         new Layout(72, 263, 868, 1555, 868, 146, 50) },
            { "achievements", new Layout(78, 250, 860, 1531, 860, 136, 52) },
            { "leaderboard",  new Layout(73, 254, 864, 1519, 876, 160, 54) },
            // measured once blank.png is in (same 941x1672 shape as the pictures above)
            { BlankKey,       new Layout(72, 263, 868, 1555, 868, 146, 50).WithTitle(270, 62, 670, 150) },
        };

        // Title area on a blank ribbon when not measured: centre of the ribbon, as fractions.
        private static readonly Rect DefaultTitle = new Rect(0.29f, 0.037f, 0.42f, 0.053f);

        // Unknown pictures: the shared layout of these frames, as fractions of the picture.
        private static readonly Rect DefaultCream = new Rect(0.076f, 0.145f, 0.845f, 0.785f);
        private static readonly Vector3 DefaultClose = new Vector3(0.928f, 0.08f, 0.058f);

        public Sprite Sprite { get; private set; }

        /// <summary>Width / height of the picture.</summary>
        public float Aspect => Sprite.rect.width / Sprite.rect.height;

        /// <summary>Cream area as anchors (0-1, origin bottom-left) of the frame.</summary>
        public Vector2 CreamMin { get; private set; }
        public Vector2 CreamMax { get; private set; }

        /// <summary>Close button box as anchors of the frame.</summary>
        public Vector2 CloseMin { get; private set; }
        public Vector2 CloseMax { get; private set; }

        /// <summary>True for the blank-ribbon frame: the panel writes its own title.</summary>
        public bool Blank { get; private set; }

        /// <summary>Title box as anchors of the frame (blank frame only).</summary>
        public Vector2 TitleMin { get; private set; }
        public Vector2 TitleMax { get; private set; }

        public static MenuPanelFrame Load(string key)
        {
            Sprite sprite = Resources.Load<Sprite>(ResourceFolder + key);
            if (sprite == null)
                return null;

            float width = sprite.rect.width;
            float height = sprite.rect.height;
            MenuPanelFrame frame = new MenuPanelFrame { Sprite = sprite, Blank = key == BlankKey };
            Rect title = DefaultTitle;

            if (Layouts.TryGetValue(key, out Layout layout))
            {
                RectInt cream = layout.Cream;
                frame.CreamMin = new Vector2(cream.xMin / width, 1f - cream.yMax / height);
                frame.CreamMax = new Vector2(cream.xMax / width, 1f - cream.yMin / height);

                Vector3 close = layout.Close;
                frame.CloseMin = new Vector2((close.x - close.z) / width, 1f - (close.y + close.z) / height);
                frame.CloseMax = new Vector2((close.x + close.z) / width, 1f - (close.y - close.z) / height);

                if (layout.Title.width > 0)
                    title = new Rect(layout.Title.xMin / width, layout.Title.yMin / height, layout.Title.width / width, layout.Title.height / height);
            }
            else
            {
                frame.CreamMin = new Vector2(DefaultCream.xMin, 1f - DefaultCream.yMax);
                frame.CreamMax = new Vector2(DefaultCream.xMax, 1f - DefaultCream.yMin);
                frame.CloseMin = new Vector2(DefaultClose.x - DefaultClose.z, 1f - DefaultClose.y - DefaultClose.z * width / height);
                frame.CloseMax = new Vector2(DefaultClose.x + DefaultClose.z, 1f - DefaultClose.y + DefaultClose.z * width / height);
            }

            frame.TitleMin = new Vector2(title.xMin, 1f - title.yMax);
            frame.TitleMax = new Vector2(title.xMax, 1f - title.yMin);
            return frame;
        }
    }
}
