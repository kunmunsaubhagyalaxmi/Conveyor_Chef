using System;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    /// <summary>
    /// Applies the approved Conveyor Chef menu artwork to the real serialized
    /// Canvas -> NEW Main Menu hierarchy.
    ///
    /// This component changes VISUAL SPRITES ONLY.
    /// It never changes RectTransform positions, sizes, anchors, pivots or scale
    /// (except in Play mode, where an outfit chef is fitted into the classic chef's place).
    /// The layout saved in menu.unity therefore stays authoritative.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-1000)]
    public sealed class MainMenuApprovedArtBinder : MonoBehaviour
    {
        private void OnEnable()
        {
            ApplyApprovedArtwork();
            if (Application.isPlaying)
            {
                if (chefRect == null)
                    chefRect = FindDeep(transform, "Chef Character") as RectTransform;
                ChefProgress.OutfitChanged += ApplyOutfit;
                if (started)            // re-enabled later; the first time Start does it
                    ApplyOutfit();
            }
        }

        private bool started;

        private void OnDisable()
        {
            ChefProgress.OutfitChanged -= ApplyOutfit;
        }

        private RectTransform chefRect;
        private Image outfitChef;
        private Vector2 fittedSize;

        private System.Collections.IEnumerator Start()
        {
            if (!Application.isPlaying)
                yield break;
            yield return null;          // the canvas has its final size now
            started = true;
            ApplyOutfit();
        }

        // Re-fitted only when the chef box itself changes size (another screen); the chef never
        // moves or resizes otherwise.
        private void LateUpdate()
        {
            if (outfitChef != null && outfitChef.gameObject.activeSelf && chefRect != null && chefRect.rect.size != fittedSize)
                LayoutOutfitChef();
        }

        // In the game the big menu chef and the top-left avatar wear the outfit picked in
        // Customize > CHEF; the classic chef keeps the approved menu art.
        private void ApplyOutfit()
        {
            ChefProgress.Outfit outfit = ChefProgress.Selected;
            Sprite full = outfit.Id == "classic" ? null : ChefProgress.LoadFull(outfit);
            ShowOutfitChef(full);
            if (full == null)
            {
                SetSprite("Chef Avatar", "avatar", true);
                return;
            }
            SetOutfitSprite("Chef Avatar", ChefProgress.LoadHud(outfit) ?? ChefProgress.LoadAvatar(outfit));
        }

        // Classic: the approved chef picture. Outfit: that picture hidden and the outfit drawn
        // by a child, so the menu's own chef animation keeps moving it.
        private void ShowOutfitChef(Sprite outfit)
        {
            if (chefRect == null)
                chefRect = FindDeep(transform, "Chef Character") as RectTransform;
            if (chefRect == null)
                return;
            Image classic = chefRect.GetComponent<Image>();

            // the logo is drawn over the chef: a wide outfit (a wok, a pizza peel) must not cover it
            Transform logo = FindDeep(transform, "Game Logo");
            if (logo != null && logo.parent == chefRect.parent && logo.GetSiblingIndex() < chefRect.GetSiblingIndex())
                logo.SetSiblingIndex(chefRect.GetSiblingIndex());

            if (outfit == null)
            {
                if (outfitChef != null)
                    outfitChef.gameObject.SetActive(false);
                SetSprite("Chef Character", "chef", true);
                return;
            }

            if (outfitChef == null)
            {
                // DesignFrameIgnore: the screen fitter (DesignFrame) re-anchors every child into the
                // design phone's frame. For a child of the full-screen chef box that moved its centre
                // from the chef box's centre to the screen's centre (+211, +80 here), so every outfit
                // chef stood far right and too high, over the logo and the buttons, on every screen.
                var go = new GameObject("Outfit Chef", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(DesignFrameIgnore));
                go.layer = chefRect.gameObject.layer;
                go.transform.SetParent(chefRect, false);
                outfitChef = go.GetComponent<Image>();
                outfitChef.raycastTarget = false;
                RectTransform rt = outfitChef.rectTransform;
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            }
            outfitChef.sprite = outfit;
            outfitChef.preserveAspect = true;
            outfitChef.gameObject.SetActive(true);
            if (classic != null)
                classic.enabled = false;
            LayoutOutfitChef();
        }

        // Every outfit chef stands exactly like the classic chef: the same size (the same boy, so
        // the same head and body size, not the same height: hats differ), feet on the same floor and
        // in the same spot. Measured on the pictures: all outfit pictures (512x1024) are drawn at one
        // scale, 1.435x the classic picture's pixels (the royal chef, in the classic's pose, then
        // overlaps the classic chef almost exactly), then nudged by outfitOffset (set in the Inspector).
        private const float OutfitPixelScale = 1.435f;      // outfit picture pixel -> classic picture pixel
        private const float OutfitSourceHeight = 1024f;     // outfit pictures as painted (imported smaller)

        [Tooltip("Moves every outfit chef together, in pixels of the classic chef picture (x: right, y: up). " +
                 "Changes apply at once in Play mode.")]
        [SerializeField] private Vector2 outfitOffset = new Vector2(-20f, 6f);

        // Where each character stands in its picture: the bottom of the shoes (pixel row from the
        // top) and the middle between the feet (pixel column), measured on the 512x1024 pictures.
        private static readonly System.Collections.Generic.Dictionary<string, Vector2> OutfitFeet =
            new System.Collections.Generic.Dictionary<string, Vector2>
            {
                { "street_food", new Vector2(252.5f, 976f) },
                { "pizza",       new Vector2(252.0f, 980f) },
                { "bbq",         new Vector2(248.0f, 990f) },
                { "sushi",       new Vector2(272.0f, 988f) },
                { "royal",       new Vector2(289.0f, 1004f) },
                { "golden",      new Vector2(273.5f, 982f) },
                { "pastry",      new Vector2(275.0f, 1005f) },
            };

        // The classic picture (593x1439): shoes end on row 1430, the feet's middle is column 296.5.
        private const float ClassicFeetRow = 1430f;
        private const float ClassicFeetColumn = 296.5f;

        private void LayoutOutfitChef()
        {
            Sprite classic = ProfessionalMainMenuEmbeddedAssets.GetSprite("chef");
            Sprite outfit = outfitChef != null ? outfitChef.sprite : null;
            if (chefRect == null || outfit == null)
                return;

            Rect r = chefRect.rect;
            fittedSize = r.size;
            RectTransform rt = outfitChef.rectTransform;
            if (r.width < 10f || r.height < 10f || classic == null)
            {
                // not laid out yet (or no classic art): fill the chef box, re-fitted when it has a size
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = r.size;
                return;
            }

            // the classic chef as preserveAspect shows it: as big as fits, centred in the box
            float classicPxW = classic.rect.width, classicPxH = classic.rect.height;
            float classicW = Mathf.Min(r.width, r.height * classicPxW / classicPxH);
            float classicH = classicW * classicPxH / classicPxW;
            float unit = classicH / classicPxH;              // box units per classic picture pixel
            // painted size of the classic picture (it may be imported smaller than painted)
            float classicScale = classicPxH / 1439f;

            // where the classic chef's feet are, in the box (0 = its centre)
            float feetX = -classicW * 0.5f + ClassicFeetColumn * classicScale * unit;
            float feetY = classicH * 0.5f - ClassicFeetRow * classicScale * unit;

            float aspect = outfit.rect.width / outfit.rect.height;
            float h = OutfitSourceHeight * OutfitPixelScale * classicScale * unit;
            float w = h * aspect;
            float k = h / OutfitSourceHeight;                // box units per outfit picture pixel

            Vector2 feet;
            if (!OutfitFeet.TryGetValue(ChefProgress.Selected.Id, out feet))
            {
                // an outfit not measured yet: feet at the bottom of its drawn outline, in the middle
                Figure o = Measure(outfit);
                feet = new Vector2(OutfitSourceHeight * aspect * 0.5f, OutfitSourceHeight * (1f - o.feet));
            }

            float left = feetX + outfitOffset.x * classicScale * unit - feet.x * k;
            float top = feetY + outfitOffset.y * classicScale * unit + feet.y * k;
            rt.sizeDelta = new Vector2(w, h);
            rt.anchoredPosition = new Vector2(left + w * 0.5f, top - h * 0.5f);
        }

        // Where the character is inside its picture, as fractions of the picture (0 = bottom).
        private struct Figure
        {
            public float feet, head;
        }

        private static readonly System.Collections.Generic.Dictionary<Sprite, Figure> figures =
            new System.Collections.Generic.Dictionary<Sprite, Figure>();

        private static Figure Measure(Sprite sprite)
        {
            if (figures.TryGetValue(sprite, out Figure figure))
                return figure;

            figure = new Figure { feet = 0f, head = 1f };
            Vector2[] vertices = sprite.vertices;      // the tight outline of the visible pixels
            Rect rect = sprite.rect;
            if (vertices != null && vertices.Length > 2 && rect.width > 0f && rect.height > 0f)
            {
                float ppu = sprite.pixelsPerUnit;
                float bottom = float.MaxValue, top = float.MinValue;
                foreach (Vector2 v in vertices)
                {
                    float y = (v.y * ppu + sprite.pivot.y) / rect.height;
                    bottom = Mathf.Min(bottom, y);
                    top = Mathf.Max(top, y);
                }

                if (top - bottom > 0.05f)
                {
                    figure.feet = Mathf.Clamp01(bottom);
                    figure.head = Mathf.Clamp01(top);
                }
            }

            figures[sprite] = figure;
            return figure;
        }

        private void SetOutfitSprite(string objectName, Sprite sprite)
        {
            Transform target = FindDeep(transform, objectName);
            Image image = target != null ? target.GetComponent<Image>() : null;
            if (image == null || sprite == null)
                return;
            image.sprite = sprite;
            image.preserveAspect = true;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!isActiveAndEnabled)
                return;

            ApplyApprovedArtwork();
            // the outfit chef is laid out, which Unity does not allow inside OnValidate
            if (Application.isPlaying)
                UnityEditor.EditorApplication.delayCall += () => { if (this != null && isActiveAndEnabled) ApplyOutfit(); };
        }
#endif

        [ContextMenu("Apply Approved Menu Artwork")]
        public void ApplyApprovedArtwork()
        {
            // Main hero artwork.
            SetSprite("Background Artwork", "background", false);
            SetSprite("Game Logo", "logo", true);
            SetSprite("Chef Character", "chef", true);
            SetSprite("Chef Avatar", "avatar", true);

            // Main action buttons (the approved art, words painted in).
            SetSprite("PLAY", "play", false);
            SetSprite("STORY", "story", false);
            SetSprite("CHALLENGES", "challenges", false);
            SetSprite("CUSTOMIZE", "customize", false);
            SetSprite("SETTINGS", "settings", false);

            // HUD artwork.
            SetSprite("Coin Counter", "coin_bar", false);
            SetSprite("Diamond Counter", "diamond_bar", false);
            SetSprite("Star Icon", "star", true);

            // Bottom navigation (the approved art, labels painted in).
            SetSprite("SHOP", "shop", true);
            SetSprite("COLLECTION", "collection", true);
            SetSprite("ACHIEVEMENTS", "achievements", true);
            SetSprite("LEADERBOARD", "leaderboard", true);

            // The approved button/navigation sprites already contain their labels.
            HideDirectChild("PLAY", "Label");
            HideDirectChild("STORY", "Label");
            HideDirectChild("CHALLENGES", "Label");
            HideDirectChild("CUSTOMIZE", "Label");
            HideDirectChild("SETTINGS", "Label");

            HideDirectChild("SHOP", "Label");
            HideDirectChild("COLLECTION", "Label");
            HideDirectChild("ACHIEVEMENTS", "Label");
            HideDirectChild("LEADERBOARD", "Label");

            // These icons are already painted into the approved currency bars.
            SetActive("Coin Icon", false);
            SetActive("Diamond Icon", false);

            // Make sure important approved elements are visible.
            SetActive("Background Artwork", true);
            SetActive("Game Logo", true);
            SetActive("Chef Character", true);
            SetActive("Chef Avatar", true);
            SetActive("Top HUD", true);
            SetActive("Main Buttons", true);
            SetActive("Bottom Navigation", true);

        }

        private void SetSprite(string objectName, string assetName, bool preserveAspect)
        {
            Transform target = FindDeep(transform, objectName);
            if (target == null)
                return;

            Image image = target.GetComponent<Image>();
            if (image == null)
                return;

            // Uses the high-quality art in Resources/MainMenu when present (see ProfessionalMainMenuEmbeddedAssets).
            Sprite sprite = ProfessionalMainMenuEmbeddedAssets.GetSprite(assetName);
            if (sprite == null)
                return;

            image.sprite = sprite;
            image.color = Color.white;
            image.enabled = true;
            image.preserveAspect = preserveAspect;

            // Buttons need to remain raycastable; decorative art should not block clicks.
            Button button = target.GetComponent<Button>();
            image.raycastTarget = button != null;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(image);
                UnityEditor.EditorUtility.SetDirty(target.gameObject);
            }
#endif
        }

        private void HideDirectChild(string parentName, string childName)
        {
            Transform parent = FindDeep(transform, parentName);
            if (parent == null)
                return;

            Transform child = parent.Find(childName);
            if (child != null && child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(false);

#if UNITY_EDITOR
                if (!Application.isPlaying)
                    UnityEditor.EditorUtility.SetDirty(child.gameObject);
#endif
            }
        }

        private void SetActive(string objectName, bool state)
        {
            Transform target = FindDeep(transform, objectName);
            if (target == null || target.gameObject.activeSelf == state)
                return;

            target.gameObject.SetActive(state);

#if UNITY_EDITOR
            if (!Application.isPlaying)
                UnityEditor.EditorUtility.SetDirty(target.gameObject);
#endif
        }

        private static Transform FindDeep(Transform parent, string objectName)
        {
            if (parent == null)
                return null;

            if (string.Equals(parent.name, objectName, StringComparison.Ordinal))
                return parent;

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform result = FindDeep(parent.GetChild(i), objectName);
                if (result != null)
                    return result;
            }

            return null;
        }
    }
}
