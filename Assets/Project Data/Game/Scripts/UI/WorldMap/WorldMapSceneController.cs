using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Watermelon.BusStop
{
    /// <summary>
    /// Runtime behaviour for the serialized World Map scene.
    /// It does not rebuild or reposition designer-authored UI at startup.
    /// Only the ScrollRect content moves when the player navigates the map.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WorldMapSceneController : MonoBehaviour
    {
        private const int LevelsPerContinent = WorldCatalog.LevelsPerContinent;
        private const string SelectedContinentKey = "CC_WorldMap_SelectedContinent";
        private const string ContinentOrderVersionKey = "CC_WorldMap_ContinentOrderVersion";
        private const int CurrentContinentOrderVersion = 2;
        private const string DragHintSeenKey = "CC_WorldMap_DragHintSeen";

        private static readonly string[] CanonicalContinentNames =
        {
            "Asia",
            "North America",
            "South America",
            "Europe",
            "Africa",
            "Australia / Oceania",
            "Antarctica"
        };

        // Converts the old progression order
        // North America, South America, Europe, Africa, Asia, Oceania
        // to the new Asia-first order.
        private static readonly int[] LegacyToCanonicalContinentIndex =
        {
            1, 2, 3, 4, 0, 5
        };

        [Header("Map")]
        [SerializeField] private ScrollRect mapScrollRect;
        [SerializeField] private RectTransform mapContent;
        [SerializeField] private WorldMapContinentNode[] continents;

        [Header("Navigation")]
        [SerializeField] private Button leftButton;
        [SerializeField] private Button rightButton;
        [SerializeField] private Button backButton;
        [SerializeField] private Button settingsButton;

        [Header("Header")]
        [SerializeField] private TextMeshProUGUI selectedChapterText;
        [SerializeField] private TextMeshProUGUI statusText;

        [Header("Drag Hint")]
        [SerializeField] private CanvasGroup dragHintCanvasGroup;

        [Header("Settings")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Button closeSettingsButton;
        [SerializeField] private Button soundButton;
        [SerializeField] private Button vibrationButton;
        [SerializeField] private TextMeshProUGUI soundText;
        [SerializeField] private TextMeshProUGUI vibrationText;

        [Header("Motion")]
        [SerializeField, Min(0.05f)] private float focusDuration = 0.35f;

        [Header("Chapter bar")]
        [Tooltip("The illustrated chapter bar in the scene (Conveyor Chef > World Map > Rebuild Chapter Bar).")]
        [SerializeField] private WorldMapChapterStrip chapterStrip;

        [Header("Scene-authored startup")]
        [Tooltip("When enabled, Play mode starts with exactly the sprites, colors, active states, sizes and positions saved in WorldMap.unity.")]
        [SerializeField] private bool preserveAuthoredStartupVisuals = true;
        [SerializeField, Range(0, 5)] private int authoredSelectedContinent = 0;
        [Tooltip("Normally OFF. Enabling this intentionally moves MapContent when Play begins.")]
        [SerializeField] private bool focusSelectedContinentOnStart = false;
        [Tooltip("When the World Map opens, the selected continent (Asia unless another is chosen) is put in the middle of the screen.")]
        [SerializeField] private bool centerContinentOnOpen = true;

        private int selectedContinent;
        private WorldMapChapterBanner chapterBanner;
        private Vector2 statusTextAuthoredPosition;
        private bool bannerShown;
        private Coroutine focusRoutine;
        private Coroutine hintRoutine;
        private readonly Dictionary<int, int> runtimeIndexBySerializedIndex =
            new Dictionary<int, int>();

        // Captured from the serialized WorldMap.unity before any runtime navigation.
        // This is the position designers see and edit in the Scene/Simulator before Play.
        private Vector2 authoredMapContentPosition;

        public int SelectedContinent => selectedContinent;

        private void Awake()
        {
            if (mapContent != null)
                authoredMapContentPosition = mapContent.anchoredPosition;

            EnsureEventSystem();
            EnsureSaveControllerReady();
            MigrateLegacySelectedContinentPreference();
            EnsureMapInteractionReady();

            Wire(leftButton, PreviousContinent);
            Wire(rightButton, NextContinent);
            Wire(backButton, BackToMenu);
            Wire(settingsButton, OpenSettings);
            Wire(closeSettingsButton, CloseSettings);
            Wire(soundButton, ToggleSound);
            Wire(vibrationButton, ToggleVibration);

            // Preserve the serialized active/inactive state of SettingsPanel.
            // The initial runtime frame must match the Scene/Simulator preview.
            RepairContinentReferencesIfNeeded();

            if (continents != null)
            {
                foreach (WorldMapContinentNode node in continents)
                {
                    // Do not use ?. with UnityEngine.Object references.
                    // A destroyed Unity object is not managed-null, so ?. can still
                    // call into its destroyed native object and throw MissingReferenceException.
                    if (node != null)
                        node.Bind(this);
                }
            }

            if (mapScrollRect != null)
                mapScrollRect.onValueChanged.AddListener(OnMapScrollValueChanged);

            CreateChapterBanner();
        }

        // The chapter banner pictures replace the logo + text header: the header's
        // WorldMapLogo image shows the selected continent's banner. Without a logo image the
        // authored header stays exactly as it is.
        private void CreateChapterBanner()
        {
            Image logo = FindHeaderLogo(selectedChapterText);
            if (logo == null)
                return;

            if (statusText != null)
                statusTextAuthoredPosition = statusText.rectTransform.anchoredPosition;

            chapterBanner = WorldMapChapterBanner.Attach(logo);
        }

        /// <summary>The header logo image (sibling of the chapter text, named WorldMapLogo).</summary>
        public static Image FindHeaderLogo(TextMeshProUGUI chapterText)
        {
            Transform header = chapterText != null ? chapterText.transform.parent : null;
            Transform logo = header != null ? header.Find("WorldMapLogo") : null;
            return logo != null ? logo.GetComponent<Image>() : null;
        }

        // Returns false when this continent has no banner picture (texts are used instead).
        private bool ShowChapterBanner(bool unlocked)
        {
            if (chapterBanner == null)
                return false;

            Sprite sprite = WorldArt.ForContinent(selectedContinent,
                unlocked ? WorldArt.ChapterBanner : WorldArt.ChapterBannerLocked, false);
            if (sprite == null && !unlocked)
                sprite = WorldArt.ForContinent(selectedContinent, WorldArt.ChapterBanner, false);

            bool useBanner = sprite != null;
            if (selectedChapterText != null)
                selectedChapterText.gameObject.SetActive(!useBanner);

            if (!useBanner)
            {
                chapterBanner.ShowAuthoredLogo();
                SetLockedBadge(false, 0f);
                if (statusText != null)
                {
                    statusText.gameObject.SetActive(true);
                    statusText.rectTransform.anchoredPosition = statusTextAuthoredPosition;
                }
                return false;
            }

            chapterBanner.Show(sprite, instant: !bannerShown);
            bannerShown = true;

            // The unlocked picture already says "UNLOCKED • 15 LEVELS"; a locked continent
            // shows the LOCKED picture (or, without it, the LOCKED text) under its banner.
            bool badge = !unlocked && SetLockedBadge(true, chapterBanner.BottomY);
            if (unlocked)
                SetLockedBadge(false, 0f);

            if (statusText != null)
            {
                bool showText = !unlocked && !badge;
                statusText.gameObject.SetActive(showText);
                if (showText)
                {
                    float halfHeight = statusText.rectTransform.rect.height * 0.5f;
                    statusText.rectTransform.anchoredPosition =
                        new Vector2(statusTextAuthoredPosition.x, chapterBanner.BottomY - halfHeight + 6f);
                }
            }

            return true;
        }

        // Same letter height (~39 units) as the "UNLOCKED • 15 LEVELS" line it stands in for.
        private const float LockedBadgeWidth = 155f;
        private const float LockedBadgePopDuration = 0.32f;

        private Image lockedBadge;
        private Coroutine lockedBadgeRoutine;

        // Shows / hides the LOCKED picture with its top edge at `top` (Header units).
        // Returns false when there is no LOCKED picture to show.
        private bool SetLockedBadge(bool visible, float top)
        {
            if (!visible)
            {
                if (lockedBadge != null)
                {
                    StopLockedBadgePop();
                    lockedBadge.gameObject.SetActive(false);
                }
                return false;
            }

            if (lockedBadge == null)
            {
                Sprite sprite = WorldArt.Shared(WorldArt.LockedStatus);
                RectTransform header = chapterBanner != null ? chapterBanner.transform.parent as RectTransform : null;
                if (sprite == null || header == null)
                    return false;

                var go = new GameObject("LockedStatus", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                go.transform.SetParent(header, false);
                go.transform.SetSiblingIndex(chapterBanner.transform.GetSiblingIndex() + 1);

                lockedBadge = go.GetComponent<Image>();
                lockedBadge.sprite = sprite;
                lockedBadge.preserveAspect = true;
                lockedBadge.raycastTarget = false;

                RectTransform r = lockedBadge.rectTransform;
                r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
                r.pivot = new Vector2(0.5f, 1f);
                r.sizeDelta = new Vector2(LockedBadgeWidth, LockedBadgeWidth * sprite.rect.height / sprite.rect.width);
                go.SetActive(false);
            }

            // Where the UNLOCKED line starts in the unlocked picture (~6 units above the
            // locked picture's bottom edge).
            lockedBadge.rectTransform.anchoredPosition = new Vector2(0f, top + 7f);

            if (!lockedBadge.gameObject.activeSelf)
            {
                lockedBadge.gameObject.SetActive(true);
                StopLockedBadgePop();
                if (isActiveAndEnabled && bannerShown)
                    lockedBadgeRoutine = StartCoroutine(PopLockedBadge());
            }

            return true;
        }

        private IEnumerator PopLockedBadge()
        {
            RectTransform r = lockedBadge.rectTransform;
            for (float t = 0f; t < LockedBadgePopDuration; t += Time.unscaledDeltaTime)
            {
                float k = t / LockedBadgePopDuration;
                // ease-out-back: a small overshoot, then settle
                const float s = 1.70158f;
                float eased = 1f + (s + 1f) * Mathf.Pow(k - 1f, 3f) + s * Mathf.Pow(k - 1f, 2f);
                r.localScale = Vector3.one * Mathf.LerpUnclamped(0.6f, 1f, eased);
                SetImageAlpha(lockedBadge, Mathf.Clamp01(k * 2.5f));
                yield return null;
            }

            lockedBadgeRoutine = null;
            r.localScale = Vector3.one;
            SetImageAlpha(lockedBadge, 1f);
        }

        private void StopLockedBadgePop()
        {
            if (lockedBadgeRoutine != null)
            {
                StopCoroutine(lockedBadgeRoutine);
                lockedBadgeRoutine = null;
            }

            if (lockedBadge != null)
            {
                lockedBadge.rectTransform.localScale = Vector3.one;
                SetImageAlpha(lockedBadge, 1f);
            }
        }

        private static void SetImageAlpha(Image image, float alpha)
        {
            Color c = image.color;
            c.a = alpha;
            image.color = c;
        }

        private void Start()
        {
            if (preserveAuthoredStartupVisuals)
            {
                // The map opens on the continent the player is on now (the furthest one
                // unlocked), not always on the authored one (Asia): after Asia it shows the next.
                selectedContinent = Mathf.Clamp(
                    ProgressContinent(),
                    0,
                    continents != null && continents.Length > 0 ? continents.Length - 1 : 0);

                if (mapContent != null)
                    mapContent.anchoredPosition = authoredMapContentPosition;

                // Apply gameplay progression immediately so locked continents are
                // correct on the first visible frame. RefreshAll is now visual-safe:
                // it updates lock sprites/glow/button states only and never rewrites
                // designer-authored labels, text colors or continent artwork colors.
                RefreshAll();
            }
            else
            {
                selectedContinent = RestoreSelectedContinent();
                RefreshAll();
            }

            if (chapterStrip == null)
                chapterStrip = FindFirstObjectByType<WorldMapChapterStrip>(FindObjectsInactive.Include);

            if (chapterStrip != null)
            {
                chapterStrip.Bind(this, continents);
                chapterStrip.Refresh(selectedContinent);
            }

            Canvas.ForceUpdateCanvases();

            if (focusSelectedContinentOnStart || centerContinentOnOpen)
                FocusContinent(selectedContinent, false);

            // DragHint's serialized state remains scene-authored.
        }

        private void OnDestroy()
        {
            if (mapScrollRect != null)
                mapScrollRect.onValueChanged.RemoveListener(OnMapScrollValueChanged);
        }

        private void RepairContinentReferencesIfNeeded()
        {
            WorldMapContinentNode[] liveNodes = null;

            if (mapContent != null)
                liveNodes = mapContent.GetComponentsInChildren<WorldMapContinentNode>(true);

            if (liveNodes == null || liveNodes.Length == 0)
                liveNodes = continents;

            if (liveNodes == null || liveNodes.Length == 0)
                return;

            // Never trust the serialized array/index order here. Older baked scenes
            // stored Asia at index 4. Sorting by the continent's identity makes those
            // scenes behave correctly immediately, even before an editor rebake/repair.
            Array.Sort(
                liveNodes,
                (a, b) =>
                {
                    if (a == null && b == null) return 0;
                    if (a == null) return 1;
                    if (b == null) return -1;
                    return GetCanonicalContinentIndex(a).CompareTo(GetCanonicalContinentIndex(b));
                });

            continents = liveNodes;
            runtimeIndexBySerializedIndex.Clear();

            for (int i = 0; i < continents.Length; i++)
            {
                WorldMapContinentNode node = continents[i];
                if (node == null)
                    continue;

                node.Bind(this);

                // Legacy nodes still send their old serialized ContinentIndex when clicked.
                // Translate that index into the canonical runtime index.
                runtimeIndexBySerializedIndex[node.ContinentIndex] = i;
            }
        }

        public static int GetCanonicalContinentIndex(WorldMapContinentNode node)
        {
            if (node == null)
                return int.MaxValue;

            for (int i = 0; i < CanonicalContinentNames.Length; i++)
            {
                if (string.Equals(
                    node.ContinentName,
                    CanonicalContinentNames[i],
                    StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return Mathf.Clamp(node.ContinentIndex, 0, CanonicalContinentNames.Length - 1);
        }

        private int ResolveRuntimeContinentIndex(int serializedIndex)
        {
            if (runtimeIndexBySerializedIndex.TryGetValue(serializedIndex, out int runtimeIndex))
                return runtimeIndex;

            return serializedIndex;
        }

        private static void MigrateLegacySelectedContinentPreference()
        {
            int version = PlayerPrefs.GetInt(ContinentOrderVersionKey, 0);
            if (version >= CurrentContinentOrderVersion)
                return;

            if (PlayerPrefs.HasKey(SelectedContinentKey))
            {
                int legacyIndex = PlayerPrefs.GetInt(SelectedContinentKey, 0);
                if (legacyIndex >= 0 && legacyIndex < LegacyToCanonicalContinentIndex.Length)
                {
                    PlayerPrefs.SetInt(
                        SelectedContinentKey,
                        LegacyToCanonicalContinentIndex[legacyIndex]);
                }
            }

            PlayerPrefs.SetInt(ContinentOrderVersionKey, CurrentContinentOrderVersion);
            PlayerPrefs.Save();
        }


        private void EnsureMapInteractionReady()
        {
            // Interaction-only repair. This never changes positions, sizes, anchors,
            // pivots, scale or any other authored visual layout.
            if (mapScrollRect == null)
                return;

            mapScrollRect.enabled = true;
            mapScrollRect.horizontal = true;
            mapScrollRect.vertical = true;

            if (mapContent != null && mapScrollRect.content != mapContent)
                mapScrollRect.content = mapContent;

            RectTransform viewport = mapScrollRect.viewport;
            if (viewport == null)
                viewport = mapScrollRect.transform as RectTransform;

            if (viewport != null)
            {
                mapScrollRect.viewport = viewport;

                Graphic viewportGraphic = viewport.GetComponent<Graphic>();
                if (viewportGraphic != null)
                    viewportGraphic.raycastTarget = true;

                Canvas canvas = viewport.GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
                    if (raycaster == null)
                        raycaster = canvas.gameObject.AddComponent<GraphicRaycaster>();

                    raycaster.enabled = true;
                }
            }

            mapScrollRect.StopMovement();

            Debug.Log(
                "[WorldMap Input] Ready. EventSystem=" +
                (EventSystem.current != null ? EventSystem.current.name : "NULL") +
                ", ScrollRect=" + mapScrollRect.enabled +
                ", Horizontal=" + mapScrollRect.horizontal +
                ", Vertical=" + mapScrollRect.vertical +
                ", Content=" + (mapScrollRect.content != null ? mapScrollRect.content.name : "NULL") +
                ", Viewport=" + (mapScrollRect.viewport != null ? mapScrollRect.viewport.name : "NULL"));
        }

        private static void EnsureEventSystem()
        {
            UIEventSystemRuntime.UseCurrentSceneEventSystem();
        }
        private static void EnsureSaveControllerReady()
        {
            if (SaveController.IsSaveLoaded)
                return;

            try
            {
                SaveController.Initialise(useAutoSave: false);
            }
            catch (Exception ex)
            {
                Debug.LogError("[WorldMap] Failed to initialise SaveController: " + ex.Message);
            }
        }

        private static void Wire(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
                return;

            // Do not change interactable/visual state on startup. The Button's
            // serialized state in WorldMap.unity is authoritative.
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        public int LevelsPerContinentCount => LevelsPerContinent;

        public int GetContinentLevelsCompleted(int continentIndex)
        {
            int start = continentIndex * LevelsPerContinent;
            int done = 0;
            for (int level = start; level < start + LevelsPerContinent; level++)
            {
                if (LevelController.IsLevelCompleted(level))
                    done++;
            }
            return done;
        }

        /// <summary>The continent the player is on: the furthest one unlocked.</summary>
        private int ProgressContinent()
        {
            int highestUnlocked = 0;
            if (continents == null)
                return 0;
            for (int i = 0; i < continents.Length; i++)
            {
                if (IsContinentUnlocked(i))
                    highestUnlocked = i;
                else
                    break;
            }
            return highestUnlocked;
        }

        public bool IsContinentUnlocked(int continentIndex)
        {
            if (continentIndex <= 0)
                return true;

            int previousStart = (continentIndex - 1) * LevelsPerContinent;
            int previousEndExclusive = previousStart + LevelsPerContinent;

            for (int level = previousStart; level < previousEndExclusive; level++)
            {
                if (!LevelController.IsLevelCompleted(level))
                    return false;
            }

            return true;
        }

        private int RestoreSelectedContinent()
        {
            if (continents == null || continents.Length == 0)
                return 0;

            int highestUnlocked = ProgressContinent();

            int stored = Mathf.Clamp(
                PlayerPrefs.GetInt(SelectedContinentKey, highestUnlocked),
                0,
                continents.Length - 1);

            return IsContinentUnlocked(stored) ? stored : highestUnlocked;
        }

        public void HandleContinentPressed(int continentIndex)
        {
            continentIndex = ResolveRuntimeContinentIndex(continentIndex);

            if (continents == null || continentIndex < 0 || continentIndex >= continents.Length)
                return;

            SelectContinent(continentIndex, true);

            if (!IsContinentUnlocked(continentIndex))
            {
                if (statusText != null)
                    statusText.text = Loc.T("LOCKED • COMPLETE THE PREVIOUS CONTINENT");

                return;
            }

            PlayerPrefs.SetInt(SelectedContinentKey, continentIndex);
            PlayerPrefs.SetInt("CC_CountryMap_LaunchedFromWorldMap", 1);
            PlayerPrefs.Save();

            PlayClick();
            Watermelon.EnhancedLoadingScreen.LoadViaLoadingScreen("CountryMap");
        }

        private void SelectContinent(int continentIndex, bool animated)
        {
            if (continents == null || continents.Length == 0)
                return;

            selectedContinent = Mathf.Clamp(continentIndex, 0, continents.Length - 1);
            PlayerPrefs.SetInt(SelectedContinentKey, selectedContinent);
            PlayerPrefs.Save();

            RefreshAll();
            FocusContinent(selectedContinent, animated);
        }

        private void PreviousContinent()
        {
            if (continents == null || continents.Length == 0)
                return;

            PlayClick();
            SelectContinent(Mathf.Max(0, selectedContinent - 1), true);
        }

        private void NextContinent()
        {
            if (continents == null || continents.Length == 0)
                return;

            PlayClick();
            SelectContinent(Mathf.Min(continents.Length - 1, selectedContinent + 1), true);
        }

        private void RefreshAll()
        {
            RepairContinentReferencesIfNeeded();

            if (continents == null || continents.Length == 0)
                return;

            selectedContinent = Mathf.Clamp(selectedContinent, 0, continents.Length - 1);

            for (int i = 0; i < continents.Length; i++)
            {
                bool unlocked = IsContinentUnlocked(i);
                WorldMapContinentNode node = continents[i];

                if (node != null)
                    node.Refresh(unlocked, i == selectedContinent);
            }

            WorldMapContinentNode selectedNode = continents[selectedContinent];
            string selectedName =
                selectedNode != null && !string.IsNullOrWhiteSpace(selectedNode.ContinentName)
                    ? selectedNode.ContinentName
                    : CanonicalContinentNames[Mathf.Clamp(
                        selectedContinent,
                        0,
                        CanonicalContinentNames.Length - 1)];

            if (selectedChapterText != null)
                selectedChapterText.text =
                    "CHAPTER " + (selectedContinent + 1) + "  •  " +
                    selectedName.ToUpperInvariant();

            bool selectedUnlocked = IsContinentUnlocked(selectedContinent);

            if (statusText != null)
            {
                statusText.text = selectedUnlocked
                    ? "UNLOCKED  •  " + LevelsPerContinent + " LEVELS"
                    : "LOCKED  •  COMPLETE THE PREVIOUS CONTINENT";
            }

            ShowChapterBanner(selectedUnlocked);

            if (chapterStrip != null)
                chapterStrip.Refresh(selectedContinent);

            if (leftButton != null)
                leftButton.interactable = selectedContinent > 0;

            if (rightButton != null)
                rightButton.interactable = selectedContinent < continents.Length - 1;
        }

        public void FocusContinent(int index, bool animated)
        {
            RepairContinentReferencesIfNeeded();

            if (mapScrollRect == null || mapContent == null || continents == null ||
                index < 0 || index >= continents.Length || continents[index] == null)
            {
                return;
            }

            RectTransform viewport = mapScrollRect.viewport;
            RectTransform target = continents[index].MapTarget;

            if (viewport == null || target == null)
                return;

            Canvas.ForceUpdateCanvases();

            Vector2 desired = -target.anchoredPosition;

            float maxX = Mathf.Max(0f, (mapContent.rect.width - viewport.rect.width) * 0.5f);
            float maxY = Mathf.Max(0f, (mapContent.rect.height - viewport.rect.height) * 0.5f);

            desired.x = Mathf.Clamp(desired.x, -maxX, maxX);
            desired.y = Mathf.Clamp(desired.y, -maxY, maxY);

            if (focusRoutine != null)
                StopCoroutine(focusRoutine);

            mapScrollRect.StopMovement();

            if (animated && isActiveAndEnabled)
                focusRoutine = StartCoroutine(FocusRoutine(desired));
            else
                mapContent.anchoredPosition = desired;
        }

        private IEnumerator FocusRoutine(Vector2 targetPosition)
        {
            Vector2 start = mapContent.anchoredPosition;
            float elapsed = 0f;
            float duration = Mathf.Max(0.05f, focusDuration);

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                mapContent.anchoredPosition = Vector2.LerpUnclamped(start, targetPosition, eased);
                yield return null;
            }

            mapContent.anchoredPosition = targetPosition;
            focusRoutine = null;
        }

        public void SelectNearestToViewport()
        {
            // Intentionally no-op for free-drag scrolling.
            //
            // Dragging is viewport navigation only; it must not change selectedContinent
            // or refresh authored UI. Selection changes only through explicit continent
            // clicks or Previous/Next buttons.
        }

        public void NotifyMapDragged()
        {
            if (PlayerPrefs.GetInt(DragHintSeenKey, 0) == 0)
            {
                PlayerPrefs.SetInt(DragHintSeenKey, 1);
                PlayerPrefs.Save();
            }

            SetDragHintVisible(false, immediate: false);
        }

        private void OnMapScrollValueChanged(Vector2 _)
        {
            if (mapScrollRect != null && mapScrollRect.velocity.sqrMagnitude > 16f)
                NotifyMapDragged();
        }

        private void SetDragHintVisible(bool visible, bool immediate)
        {
            if (dragHintCanvasGroup == null)
                return;

            if (hintRoutine != null)
                StopCoroutine(hintRoutine);

            float target = visible ? 1f : 0f;

            if (immediate || !isActiveAndEnabled)
            {
                dragHintCanvasGroup.alpha = target;
                dragHintCanvasGroup.blocksRaycasts = false;
                dragHintCanvasGroup.interactable = false;
                return;
            }

            hintRoutine = StartCoroutine(FadeHint(target));
        }

        private IEnumerator FadeHint(float target)
        {
            float start = dragHintCanvasGroup.alpha;
            float elapsed = 0f;
            const float duration = 0.2f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                dragHintCanvasGroup.alpha = Mathf.Lerp(start, target, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            dragHintCanvasGroup.alpha = target;
            hintRoutine = null;
        }

        private void BackToMenu()
        {
            PlayClick();
            Watermelon.EnhancedLoadingScreen.LoadViaLoadingScreen("menu");
        }

        // Every settings button in the game opens the shared Settings panel.
        private void OpenSettings()
        {
            ConveyorSettingsPanel.Show();
        }

        private void CloseSettings()
        {
            PlayClick();

            if (settingsPanel != null)
                settingsPanel.SetActive(false);
        }

        private void ToggleSound()
        {
            bool enabled = AudioController.GetVolume() > 0.001f;
            AudioController.SetVolume(enabled ? 0f : 1f);
            RefreshSettingsLabels();
            PlayClick();
        }

        private void ToggleVibration()
        {
            AudioController.SetVibrationState(!AudioController.IsVibrationEnabled());
            RefreshSettingsLabels();
            PlayClick();
        }

        private void RefreshSettingsLabels()
        {
            if (soundText != null)
                soundText.text = "SOUND: " + (AudioController.GetVolume() > 0.001f ? "ON" : "OFF");

            if (vibrationText != null)
                vibrationText.text = "VIBRATION: " + (AudioController.IsVibrationEnabled() ? "ON" : "OFF");
        }

        private static void PlayClick()
        {
            try
            {
                AudioController.PlaySound(AudioController.Sounds.buttonSound);
            }
            catch
            {
                // UI stays functional even if audio is not ready.
            }
        }

#if UNITY_EDITOR
        public void EditorConfigure(
            ScrollRect scrollRect,
            RectTransform content,
            WorldMapContinentNode[] nodes,
            Button previous,
            Button next,
            Button back,
            Button settings,
            TextMeshProUGUI chapterText,
            TextMeshProUGUI stateText,
            CanvasGroup hint,
            GameObject settingsRoot,
            Button settingsClose,
            Button sound,
            Button vibration,
            TextMeshProUGUI soundLabel,
            TextMeshProUGUI vibrationLabel)
        {
            mapScrollRect = scrollRect;
            mapContent = content;
            continents = nodes;
            leftButton = previous;
            rightButton = next;
            backButton = back;
            settingsButton = settings;
            selectedChapterText = chapterText;
            statusText = stateText;
            dragHintCanvasGroup = hint;
            settingsPanel = settingsRoot;
            closeSettingsButton = settingsClose;
            soundButton = sound;
            vibrationButton = vibration;
            soundText = soundLabel;
            vibrationText = vibrationLabel;
        }
#endif
    }
}
