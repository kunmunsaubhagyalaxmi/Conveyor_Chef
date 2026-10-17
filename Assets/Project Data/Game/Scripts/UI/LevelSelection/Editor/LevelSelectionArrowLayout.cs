#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Watermelon.BusStop;

namespace Watermelon.EditorTools
{
    /// <summary>
    /// The page arrows on the Level Selection screen of every country.
    ///
    /// * Runs once by itself (per project) when LevelSelection.unity is open: gives both arrows
    ///   of all countries the same blue arrow art (the left one mirrored), 120 x 120, pinned 56
    ///   from the screen's side and level with the cards' pictures, on every phone.
    /// * "Copy Arrows From Selected Country To All": move the arrows of one country in the
    ///   Scene view (edit mode, not Play mode), select that country, run it - every other
    ///   country gets the same arrow layout.
    /// </summary>
    [InitializeOnLoad]
    public static class LevelSelectionArrowLayout
    {
        private const string ScenePath = "Assets/Project Data/Game/Scenes/LevelSelection.unity";
        private const string DoneKey = "ConveyorChef.LevelSelection.ArrowLayout.v2";

        // centre: 56 canvas units in from the screen side, 46.46% up the screen (card pictures)
        private const float Height = 0.4646f;
        private const float Inset = 56f;
        private static readonly Vector2 Size = new Vector2(120f, 120f);

        private static bool queued;

        static LevelSelectionArrowLayout()
        {
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            Queue();
        }

        private static string ProjectKey => DoneKey + "." + Application.dataPath.GetHashCode();

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode) => Queue();

        private static void Queue()
        {
            if (queued || EditorPrefs.GetBool(ProjectKey, false))
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

            bool wasDirty = scene.isDirty;
            int count = 0;
            foreach (LevelSelectionCountry country in Countries())
            {
                var left = country.transform.Find("Left Arrow") as RectTransform;
                var right = country.transform.Find("Right Arrow") as RectTransform;
                count += Set(left, 0f, Inset, -1f);
                count += Set(right, 1f, -Inset, 1f);
                CopySprite(right, left);
            }

            Finish(scene, wasDirty, count, "arrows placed beside the cards in every country");
            EditorPrefs.SetBool(ProjectKey, true);
        }

        [MenuItem("Conveyor Chef/Level Selection/Copy Arrows From Selected Country To All", priority = 60)]
        private static void CopyFromSelected()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Level Selection arrows",
                    "Leave Play mode first: changes made in Play mode are not saved.", "OK");
                return;
            }

            LevelSelectionCountry source = Selection.activeGameObject != null
                ? Selection.activeGameObject.GetComponentInParent<LevelSelectionCountry>(true)
                : null;
            if (source == null)
            {
                EditorUtility.DisplayDialog("Level Selection arrows",
                    "Select a country in the Hierarchy (for example 'Level Selection - China' or its Left Arrow) first.", "OK");
                return;
            }

            var left = source.transform.Find("Left Arrow") as RectTransform;
            var right = source.transform.Find("Right Arrow") as RectTransform;
            if (left == null || right == null)
            {
                EditorUtility.DisplayDialog("Level Selection arrows", source.name + " has no Left Arrow / Right Arrow.", "OK");
                return;
            }

            Scene scene = source.gameObject.scene;
            bool wasDirty = scene.isDirty;
            int count = 0;
            foreach (LevelSelectionCountry country in Countries())
            {
                if (country == source)
                    continue;
                count += Copy(left, country.transform.Find("Left Arrow") as RectTransform);
                count += Copy(right, country.transform.Find("Right Arrow") as RectTransform);
            }

            Finish(scene, wasDirty, count, "arrows of " + source.name + " copied to every country");
        }

        private static LevelSelectionCountry[] Countries() =>
            Object.FindObjectsByType<LevelSelectionCountry>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        private static int Set(RectTransform arrow, float side, float x, float scaleX)
        {
            if (arrow == null)
                return 0;
            Undo.RecordObject(arrow, "Level Selection Arrows");
            arrow.anchorMin = arrow.anchorMax = new Vector2(side, Height);
            arrow.pivot = new Vector2(0.5f, 0.5f);
            arrow.anchoredPosition = new Vector2(x, 0f);
            arrow.sizeDelta = Size;
            arrow.localScale = new Vector3(scaleX, 1f, 1f);
            return 1;
        }

        private static void CopySprite(RectTransform from, RectTransform to)
        {
            var source = from != null ? from.GetComponent<UnityEngine.UI.Image>() : null;
            var target = to != null ? to.GetComponent<UnityEngine.UI.Image>() : null;
            if (source == null || target == null || target.sprite == source.sprite)
                return;
            Undo.RecordObject(target, "Level Selection Arrows");
            target.sprite = source.sprite;
            target.preserveAspect = true;
        }

        private static int Copy(RectTransform from, RectTransform to)
        {
            if (from == null || to == null)
                return 0;
            Undo.RecordObject(to, "Level Selection Arrows");
            to.anchorMin = from.anchorMin;
            to.anchorMax = from.anchorMax;
            to.pivot = from.pivot;
            to.offsetMin = from.offsetMin;
            to.offsetMax = from.offsetMax;
            to.localScale = from.localScale;
            to.localRotation = from.localRotation;
            return 1;
        }

        private static void Finish(Scene scene, bool wasDirty, int count, string what)
        {
            if (count == 0)
                return;
            EditorSceneManager.MarkSceneDirty(scene);
            // save only when nothing else was unsaved, so other edits are never saved by surprise
            if (!wasDirty)
                EditorSceneManager.SaveScene(scene);
            Debug.Log("[Level Selection] " + count + " " + what + "." +
                      (wasDirty ? " The scene had other unsaved changes: save it (Ctrl+S) to keep this." : " Scene saved."));
        }
    }
}
#endif
