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
    /// * Runs once by itself (per project) when LevelSelection.unity is open: puts both arrows of
    ///   all countries beside the cards, at the spot set on China (Galaxy Note20 Ultra preview),
    ///   as screen-relative anchors so they stay in place on every phone.
    /// * "Copy Arrows From Selected Country To All": move the arrows of one country in the
    ///   Scene view (edit mode, not Play mode), select that country, run it - every other
    ///   country gets the same arrow layout.
    /// </summary>
    [InitializeOnLoad]
    public static class LevelSelectionArrowLayout
    {
        private const string ScenePath = "Assets/Project Data/Game/Scenes/LevelSelection.unity";
        private const string DoneKey = "ConveyorChef.LevelSelection.ArrowLayout.v1";

        // fractions of the screen (anchors), offsets zero
        private static readonly Vector2 LeftMin = new Vector2(-0.0098f, 0.4084f);
        private static readonly Vector2 LeftMax = new Vector2(0.0629f, 0.4782f);
        private static readonly Vector2 RightMin = new Vector2(0.9212f, 0.4084f);
        private static readonly Vector2 RightMax = new Vector2(0.9939f, 0.4782f);

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
                count += Set(country.transform.Find("Left Arrow") as RectTransform, LeftMin, LeftMax);
                count += Set(country.transform.Find("Right Arrow") as RectTransform, RightMin, RightMax);
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

        private static int Set(RectTransform arrow, Vector2 min, Vector2 max)
        {
            if (arrow == null)
                return 0;
            Undo.RecordObject(arrow, "Level Selection Arrows");
            arrow.anchorMin = min;
            arrow.anchorMax = max;
            arrow.offsetMin = Vector2.zero;
            arrow.offsetMax = Vector2.zero;
            return 1;
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
