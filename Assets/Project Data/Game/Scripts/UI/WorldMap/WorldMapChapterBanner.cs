using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.BusStop
{
    /// <summary>
    /// World Map header picture: the Conveyor Chef logo, "CHAPTER n • NAME" and
    /// "UNLOCKED • 15 LEVELS" painted as one image per continent
    /// (Resources/World/&lt;continent&gt;/chapter_banner.png, and chapter_banner_locked.png
    /// without the UNLOCKED line).
    ///
    /// It drives the header's WorldMapLogo image, so the banner placed in WorldMap.unity
    /// (Conveyor Chef > World Map > Use Chapter Banner Header) is exactly what Play shows:
    /// its width and top edge are kept, its height follows each picture. Changing continent
    /// cross-fades the old picture out while the new one fades and scales in.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WorldMapChapterBanner : MonoBehaviour
    {
        private const float FadeDuration = 0.28f;
        private const float StartScale = 0.94f;

        private Image front;
        private Image back;
        private float width;
        private float top;
        private Coroutine fadeRoutine;

        private Sprite authoredSprite;
        private Vector2 authoredSize;
        private Vector2 authoredPivot;
        private Vector2 authoredPosition;

        /// <summary>Bottom edge of the current picture, in the Header's local units.</summary>
        public float BottomY => top - front.rectTransform.sizeDelta.y;

        /// <summary>Turns the header logo image into the chapter banner (runtime only).</summary>
        public static WorldMapChapterBanner Attach(Image logo)
        {
            WorldMapChapterBanner banner = logo.GetComponent<WorldMapChapterBanner>();
            if (banner == null)
                banner = logo.gameObject.AddComponent<WorldMapChapterBanner>();
            banner.Initialise(logo);
            return banner;
        }

        private void Initialise(Image logo)
        {
            if (front != null)
                return;

            front = logo;
            RectTransform rect = front.rectTransform;

            authoredSprite = front.sprite;
            authoredSize = rect.sizeDelta;
            authoredPivot = rect.pivot;
            authoredPosition = rect.anchoredPosition;

            // Keep the authored width and top edge; grow downwards from the top.
            width = rect.rect.width;
            top = rect.anchoredPosition.y + (1f - rect.pivot.y) * rect.rect.height;
            rect.pivot = new Vector2(rect.pivot.x, 1f);
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, top);

            front.preserveAspect = true;
            front.raycastTarget = false;     // the header buttons keep their taps

            // The outgoing picture, drawn just behind the current one.
            var go = new GameObject("ChapterBanner (previous)", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(rect.parent, false);
            go.transform.SetSiblingIndex(rect.GetSiblingIndex());

            RectTransform backRect = (RectTransform)go.transform;
            backRect.anchorMin = rect.anchorMin;
            backRect.anchorMax = rect.anchorMax;
            backRect.pivot = rect.pivot;
            backRect.anchoredPosition = rect.anchoredPosition;

            back = go.GetComponent<Image>();
            back.raycastTarget = false;
            back.preserveAspect = true;
            back.enabled = false;
        }

        public void Show(Sprite sprite, bool instant)
        {
            if (sprite == null || front.sprite == sprite)
                return;

            StopFade();

            bool hadPicture = front.sprite != null && front.enabled && !instant && isActiveAndEnabled;

            // the picture on screen becomes the one fading out
            if (hadPicture)
                Apply(back, front.sprite, front.color.a);

            RectTransform rect = front.rectTransform;
            rect.pivot = new Vector2(authoredPivot.x, 1f);
            rect.anchoredPosition = new Vector2(authoredPosition.x, top);
            Apply(front, sprite, hadPicture ? 0f : 1f);
            back.enabled = hadPicture;

            if (hadPicture)
                fadeRoutine = StartCoroutine(CrossFade());
        }

        /// <summary>Puts the logo back exactly as authored (used when a continent has no banner).</summary>
        public void ShowAuthoredLogo()
        {
            StopFade();
            back.enabled = false;

            RectTransform rect = front.rectTransform;
            front.sprite = authoredSprite;
            rect.pivot = authoredPivot;
            rect.anchoredPosition = authoredPosition;
            rect.sizeDelta = authoredSize;
            rect.localScale = Vector3.one;
            SetAlpha(front, 1f);
        }

        private IEnumerator CrossFade()
        {
            RectTransform rect = front.rectTransform;

            for (float t = 0f; t < FadeDuration; t += Time.unscaledDeltaTime)
            {
                float k = t / FadeDuration;
                float eased = 1f - (1f - k) * (1f - k) * (1f - k);    // ease-out cubic

                SetAlpha(front, eased);
                SetAlpha(back, 1f - Mathf.Clamp01(k * 1.6f));          // old one clears a bit faster
                rect.localScale = Vector3.one * Mathf.LerpUnclamped(StartScale, 1f, eased);
                yield return null;
            }

            FinishFade();
        }

        private void StopFade()
        {
            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
                fadeRoutine = null;
            }

            FinishFade();
        }

        private void FinishFade()
        {
            fadeRoutine = null;
            if (front != null)
            {
                SetAlpha(front, 1f);
                front.rectTransform.localScale = Vector3.one;
            }
            if (back != null)
                back.enabled = false;
        }

        private void Apply(Image image, Sprite sprite, float alpha)
        {
            image.sprite = sprite;
            image.rectTransform.sizeDelta = new Vector2(width, Height(sprite));
            image.rectTransform.localScale = Vector3.one;
            SetAlpha(image, alpha);
        }

        private float Height(Sprite sprite)
        {
            return sprite != null ? width * sprite.rect.height / sprite.rect.width : authoredSize.y;
        }

        private static void SetAlpha(Image image, float alpha)
        {
            Color c = image.color;
            c.a = alpha;
            image.color = c;
        }

        // never leave a half-faded header behind
        private void OnDisable()
        {
            if (fadeRoutine != null)
                StopFade();
        }
    }
}
