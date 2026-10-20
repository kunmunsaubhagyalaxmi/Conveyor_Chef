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
                // where the chef rests, read before the menu's slide-in moves him
                if (chefRect == null)
                {
                    chefRect = FindDeep(transform, "Chef Character") as RectTransform;
                    if (chefRect != null)
                        chefRestX = chefRect.anchoredPosition.x;
                }
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

        // Gap kept between an outfit chef and the screen's left edge / the main buttons (canvas units).
        private const float OutfitMargin = 16f;

        private RectTransform chefRect;
        private float chefRestX;
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

        // The main menu animates "Chef Character" (slide-in, breathing) through its position, so
        // the outfit is shown by a child picture inside it, never by moving or resizing it.
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
            {
                chefRect = FindDeep(transform, "Chef Character") as RectTransform;
                if (chefRect != null)
                    chefRestX = chefRect.anchoredPosition.x;
            }
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
                var go = new GameObject("Outfit Chef", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
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

        // The outfit pictures are wider than the classic chef (0.5 vs 0.41 of their height) and hold
        // their prop (wok, pizza, knife) out to the right, so in the classic chef's place the prop
        // would be hidden behind the main buttons. The outfit chef stands on the classic chef's
        // floor, fitted between the screen's left edge and the buttons, no taller than the classic.
        private void LayoutOutfitChef()
        {
            Sprite classic = ProfessionalMainMenuEmbeddedAssets.GetSprite("chef");
            Sprite outfit = outfitChef != null ? outfitChef.sprite : null;
            if (chefRect == null || outfit == null)
                return;

            Rect r = chefRect.rect;
            fittedSize = r.size;
            RectTransform rt = outfitChef.rectTransform;
            RectTransform parent = chefRect.parent as RectTransform;
            if (r.width < 10f || r.height < 10f || classic == null || parent == null)
            {
                // not laid out yet (or no classic art): fill the chef box, re-fitted when it has a size
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = r.size;
                return;
            }

            float classicAspect = classic.rect.width / classic.rect.height;
            float outfitAspect = outfit.rect.width / outfit.rect.height;
            // the classic chef as preserveAspect shows it: as big as fits, centred
            float classicW = Mathf.Min(r.width, r.height * classicAspect);
            float classicH = classicW / classicAspect;
            float bottom = -classicH * 0.5f;

            // free space in the chef box's own units, measured from where the chef rests
            Rect p = parent.rect;
            float centre = Mathf.Lerp(p.xMin, p.xMax, (chefRect.anchorMin.x + chefRect.anchorMax.x) * 0.5f) + chefRestX;
            float left = p.xMin - centre + OutfitMargin;
            float right = r.width * 0.5f;
            RectTransform buttons = FindDeep(transform, "Main Buttons") as RectTransform;
            if (buttons != null)
            {
                Vector3[] corners = new Vector3[4];
                buttons.GetWorldCorners(corners);
                right = parent.InverseTransformPoint(corners[0]).x - centre - OutfitMargin;
            }

            float h = Mathf.Min(classicH, Mathf.Max(10f, right - left) / outfitAspect);
            float w = h * outfitAspect;
            rt.sizeDelta = new Vector2(w, h);
            rt.anchoredPosition = new Vector2((left + right) * 0.5f, bottom + h * 0.5f);
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
