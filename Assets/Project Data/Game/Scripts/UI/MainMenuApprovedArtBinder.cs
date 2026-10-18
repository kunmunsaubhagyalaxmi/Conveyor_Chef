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

        // How far an outfit chef may reach further left than the classic chef (canvas units).
        private const float OutfitExtraWidth = 80f;

        private RectTransform chefRect;
        private Vector2 chefOffsetMin, chefOffsetMax;

        private System.Collections.IEnumerator Start()
        {
            if (!Application.isPlaying)
                yield break;
            yield return null;          // the canvas has its final size now
            started = true;
            ApplyOutfit();
        }

        // In the game the big menu chef and the top-left avatar wear the outfit picked in
        // Customize > CHEF; the classic chef keeps the approved menu art.
        private void ApplyOutfit()
        {
            RestoreChefRect();
            ChefProgress.Outfit outfit = ChefProgress.Selected;
            if (outfit.Id == "classic")
            {
                SetSprite("Chef Character", "chef", true);
                SetSprite("Chef Avatar", "avatar", true);
                return;
            }
            Sprite full = ChefProgress.LoadFull(outfit);
            FitOutfitChef(full);
            SetOutfitSprite("Chef Character", full);
            SetOutfitSprite("Chef Avatar", ChefProgress.LoadHud(outfit) ?? ChefProgress.LoadAvatar(outfit));
        }

        // The outfit pictures are wider than the classic chef (0.5 vs 0.41 of their height), so at
        // the same height they would reach into the buttons. The outfit chef stands where the
        // classic one stands: same floor, same right edge, at most a little wider to the left,
        // and only as tall as that width allows. Play mode only; the scene layout is untouched.
        private void FitOutfitChef(Sprite outfit)
        {
            Sprite classic = ProfessionalMainMenuEmbeddedAssets.GetSprite("chef");
            if (chefRect == null || outfit == null || classic == null)
                return;

            Rect r = chefRect.rect;
            float classicAspect = classic.rect.width / classic.rect.height;
            float outfitAspect = outfit.rect.width / outfit.rect.height;
            // the classic chef as preserveAspect shows it: full height, centred
            float classicW = Mathf.Min(r.width, r.height * classicAspect);
            float classicH = classicW / classicAspect;
            float right = r.center.x + classicW * 0.5f;
            float bottom = r.center.y - classicH * 0.5f;

            float h = Mathf.Min(classicH, (classicW + OutfitExtraWidth) / outfitAspect);
            float w = h * outfitAspect;
            chefRect.offsetMin = chefOffsetMin + new Vector2((right - w) - r.xMin, bottom - r.yMin);
            chefRect.offsetMax = chefOffsetMax + new Vector2(right - r.xMax, (bottom + h) - r.yMax);
        }

        private void RestoreChefRect()
        {
            if (chefRect == null)
            {
                Transform t = FindDeep(transform, "Chef Character");
                chefRect = t as RectTransform;
                if (chefRect == null)
                    return;
                chefOffsetMin = chefRect.offsetMin;
                chefOffsetMax = chefRect.offsetMax;
            }
            chefRect.offsetMin = chefOffsetMin;
            chefRect.offsetMax = chefOffsetMax;
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
            // the outfit chef is resized, which Unity does not allow inside OnValidate
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

            // Main action buttons.
            SetSprite("PLAY", "play", false);
            SetSprite("STORY", "story", false);
            SetSprite("CHALLENGES", "challenges", false);
            SetSprite("CUSTOMIZE", "customize", false);
            SetSprite("SETTINGS", "settings", false);

            // HUD artwork.
            SetSprite("Coin Counter", "coin_bar", false);
            SetSprite("Diamond Counter", "diamond_bar", false);
            SetSprite("Star Icon", "star", true);

            // Bottom navigation.
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
