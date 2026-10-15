using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// The standard top bar, taken from the Country Map (canvas units, 1080 wide):
    ///   BACK      250 x 84, centre 145 from the left, 138 from the top
    ///   SETTINGS   82 x 82, centre 55 from the right, 129 from the top
    ///   COINS     245 x 82 pill (icon 68, plus 62, value 110 x 54), centre 218.7 from the
    ///             right, 131.9 from the top; with a gem pill too, gems take that place and
    ///             coins sit one pill further left.
    /// The editor menu Conveyor Chef > UI > Line Up Top Bars applies it to the Country Map
    /// and Level Selection scenes and saves them, so it can still be edited by hand.
    /// Positions are measured from the screen's corners, so panel anchors don't matter.
    /// </summary>
    public static class BackButtonLayout
    {
        public static readonly Vector2 BackFromTopLeft = new Vector2(145f, -138f);
        public static readonly Vector2 BackSize = new Vector2(250f, 84f);

        public static readonly Vector2 SettingsFromTopRight = new Vector2(-55f, -129f);
        public static readonly Vector2 SettingsSize = new Vector2(82f, 82f);

        public static readonly Vector2 PillFromTopRight = new Vector2(-218.7f, -131.9f);
        public static readonly Vector2 PillSize = new Vector2(245f, 82f);
        public const float PillGap = 22f;

        /// <summary>"BackButton" / "Back Button", but not the hidden legacy copies.</summary>
        public static bool IsBackButton(Transform t) => Matches(t, "backbutton");

        public static bool Matches(Transform t, string compactName)
        {
            if (t.name.Replace(" ", "").ToLowerInvariant() != compactName)
                return false;
            for (Transform p = t; p != null; p = p.parent)
            {
                if (p.name.Contains("Legacy") || p.name.Contains("OLD "))
                    return false;
            }
            return true;
        }

        public static bool Apply(RectTransform rect) => Place(rect, false, BackFromTopLeft, BackSize);

        /// <summary>
        /// Centres <paramref name="rect"/> at <paramref name="offset"/> (canvas units) from the
        /// screen's top-left or top-right corner with the given size. False if the canvas has
        /// no size yet.
        /// </summary>
        public static bool Place(RectTransform rect, bool fromRight, Vector2 offset, Vector2 size)
        {
            Canvas canvas = rect.GetComponentInParent<Canvas>(true);
            if (canvas == null)
                return false;
            var canvasRect = (RectTransform)canvas.rootCanvas.transform;
            if (canvasRect.rect.width < 1f)
                return false;

            var corners = new Vector3[4];
            canvasRect.GetWorldCorners(corners);
            Vector3 corner = fromRight ? corners[2] : corners[1];
            Vector3 centre = corner + canvasRect.TransformVector(new Vector3(offset.x, offset.y, 0f));

            rect.anchorMin = rect.anchorMax = fromRight ? new Vector2(1f, 1f) : new Vector2(0f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            Vector3 parentScale = rect.parent != null ? rect.parent.lossyScale : Vector3.one;
            Vector3 canvasScale = canvasRect.lossyScale;
            rect.sizeDelta = new Vector2(size.x * canvasScale.x / Mathf.Max(1e-5f, parentScale.x),
                                         size.y * canvasScale.y / Mathf.Max(1e-5f, parentScale.y));
            rect.localScale = Vector3.one;
            rect.position = centre;
            rect.anchoredPosition = new Vector2(Mathf.Round(rect.anchoredPosition.x * 10f) / 10f, Mathf.Round(rect.anchoredPosition.y * 10f) / 10f);
            return true;
        }

        /// <summary>
        /// Inside a currency pill: icon on the left, plus button on the right, value in the
        /// middle, at the Country Map coin pill's sizes. Children are found by the end of
        /// their name ("... Icon", "... Plus" / "... Plus Button", "... Value").
        /// </summary>
        public static void LayoutPill(RectTransform pill)
        {
            foreach (RectTransform child in pill)
            {
                string n = child.name.ToLowerInvariant();
                if (n.EndsWith("icon"))
                    Set(child, new Vector2(0f, 0.5f), new Vector2(40f, 0f), new Vector2(68f, 68f));
                else if (n.EndsWith("plus") || n.EndsWith("plus button"))
                    Set(child, new Vector2(1f, 0.5f), new Vector2(-34f, 0f), new Vector2(62f, 62f));
                else if (n.EndsWith("value"))
                    Set(child, new Vector2(0.5f, 0.5f), new Vector2(-9.4f, 4f), new Vector2(110f, 54f));
            }
        }

        private static void Set(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            rect.localScale = Vector3.one;
        }
    }
}
