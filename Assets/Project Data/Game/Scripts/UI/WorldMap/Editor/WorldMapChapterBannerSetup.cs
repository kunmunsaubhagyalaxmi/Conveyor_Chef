#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Watermelon.BusStop;

namespace Watermelon.EditorTools
{
    /// <summary>
    /// Puts the chapter banner into WorldMap.unity itself, so the Scene view shows what Play
    /// shows: the header's WorldMapLogo image becomes Asia's banner (logo + "CHAPTER 1 • ASIA"
    /// + "UNLOCKED • 15 LEVELS" in one picture) and the two old header texts are switched off.
    /// At runtime WorldMapSceneController swaps the picture per continent.
    ///
    /// Runs by itself once (per project) when WorldMap.unity is open; "Use Chapter Banner
    /// Header" applies it again. Move or resize the WorldMapLogo object freely afterwards:
    /// Play keeps its width and top edge.
    /// </summary>
    [InitializeOnLoad]
    public static class WorldMapChapterBannerSetup
    {
        private const string ScenePath = "Assets/Project Data/Game/Scenes/WorldMap.unity";
        private const string BannerPath = "Assets/Project Data/Game/Resources/World/asia/chapter_banner.png";
        private const string DoneKey = "ConveyorChef.WorldMap.ChapterBannerApplied.v1";

        // Fits between the Back and Settings buttons; top edge where the old logo started.
        private const float Width = 700f;
        private const float Top = 146f;

        private static bool queued;

        static WorldMapChapterBannerSetup()
        {
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            Queue();
        }

        private static string ProjectDoneKey => DoneKey + "." + Application.dataPath.GetHashCode();

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode) => Queue();

        private static void Queue()
        {
            if (queued || EditorPrefs.GetBool(ProjectDoneKey, false))
                return;
            queued = true;
            EditorApplication.delayCall += TryAutoApply;
        }

        private static void TryAutoApply()
        {
            queued = false;

            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                Queue();
                return;
            }

            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != ScenePath)
                return;

            Apply(scene);
        }

        [MenuItem("Conveyor Chef/World Map/Use Chapter Banner Header", priority = 42)]
        public static void ApplyMenu()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.path != ScenePath)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    return;
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            Image logo = Apply(scene);
            if (logo != null)
            {
                Selection.activeGameObject = logo.gameObject;
                EditorGUIUtility.PingObject(logo.gameObject);
            }
        }

        private static Image Apply(Scene scene)
        {
            WorldMapSceneController controller = Object.FindFirstObjectByType<WorldMapSceneController>(FindObjectsInactive.Include);
            Sprite banner = AssetDatabase.LoadAssetAtPath<Sprite>(BannerPath);
            if (controller == null || banner == null)
            {
                Debug.LogWarning("[World Map] Chapter banner not applied: " +
                                 (controller == null ? "no WorldMapSceneController in the scene." : "missing " + BannerPath));
                return null;
            }

            var so = new SerializedObject(controller);
            var chapterText = so.FindProperty("selectedChapterText").objectReferenceValue as TextMeshProUGUI;
            var statusText = so.FindProperty("statusText").objectReferenceValue as TextMeshProUGUI;

            Image logo = WorldMapSceneController.FindHeaderLogo(chapterText);
            if (logo == null)
            {
                Debug.LogWarning("[World Map] Chapter banner not applied: Header/WorldMapLogo image not found.");
                return null;
            }

            bool wasDirty = scene.isDirty;
            RectTransform rect = logo.rectTransform;

            Undo.RecordObjects(new Object[] { logo, rect }, "Use Chapter Banner Header");
            logo.sprite = banner;
            logo.preserveAspect = true;
            logo.raycastTarget = false;
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, Top);
            rect.sizeDelta = new Vector2(Width, Width * banner.rect.height / banner.rect.width);

            // The banner already shows the chapter and UNLOCKED line; LOCKED is shown at runtime.
            if (chapterText != null)
            {
                Undo.RecordObject(chapterText.gameObject, "Use Chapter Banner Header");
                chapterText.gameObject.SetActive(false);
            }
            if (statusText != null)
            {
                Undo.RecordObject(statusText.gameObject, "Use Chapter Banner Header");
                statusText.gameObject.SetActive(false);
            }

            EditorSceneManager.MarkSceneDirty(scene);

            // Save only when nothing else was unsaved, so other edits are never saved by surprise.
            if (!wasDirty)
                EditorSceneManager.SaveScene(scene);

            EditorPrefs.SetBool(ProjectDoneKey, true);
            Debug.Log("[World Map] Header now shows the chapter banner (Header > WorldMapLogo)." +
                      (wasDirty ? " The scene had other unsaved changes: save it (Ctrl+S) to keep this." : " Scene saved."));
            return logo;
        }
    }
}
#endif
