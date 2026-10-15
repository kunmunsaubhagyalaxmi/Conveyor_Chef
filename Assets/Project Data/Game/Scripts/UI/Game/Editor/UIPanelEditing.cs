using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// Conveyor Chef > UI Panels: makes the win / lose popups visible in the Game scene so
    /// they can be edited in the Hierarchy and Scene view.
    ///
    /// Both live in Game.unity under "UI Main Canvas" (UI Complete, UI Game Over). That object
    /// is saved switched off (GameMainCanvas switches it on in Play mode) and each page's own
    /// Canvas component is off until the game opens the page, so normally nothing shows.
    /// "Edit ..." switches them on; "Hide ..." puts them back. Either state is fine to save:
    /// the game hides every page when it starts.
    ///
    /// The win panel also gets its "Chef XP" bar as a real object (where GET X3 was), so it
    /// can be moved and restyled; the game uses it as it is.
    /// </summary>
    public static class UIPanelEditing
    {
        private const string GameScenePath = "Assets/Project Data/Game/Scenes/Game.unity";

        [MenuItem("Conveyor Chef/UI Panels/Edit Win Panel", false, 10)]
        private static void EditWinPanel()
        {
            UIComplete page = OpenPage<UIComplete>();
            if (page == null)
                return;

            // GET X3 is hidden in the game; the XP bar takes its place.
            Vector2 barPosition = new Vector2(0f, -294f);
            Transform x3 = FindDeep(page.transform, "Multiply Reward Button");
            if (x3 is RectTransform x3Rect)
                barPosition = x3Rect.anchoredPosition;
            if (x3 != null && x3.gameObject.activeSelf)
            {
                Undo.RecordObject(x3.gameObject, "Hide GET X3");
                x3.gameObject.SetActive(false);
            }

            Transform existing = page.transform.Find(ChefXPWidget.ObjectName);
            ChefXPWidget bar = existing != null ? existing.GetComponent<ChefXPWidget>() : null;
            if (bar == null)
            {
                if (existing != null)
                    Undo.DestroyObjectImmediate(existing.gameObject);
                bar = ChefXPWidget.Create(page.transform, barPosition);
                Undo.RegisterCreatedObjectUndo(bar.gameObject, "Add Chef XP bar");
            }
            bar.ShowPreview();

            ShowOnly(page, FindPage<UIGameOver>());
        }

        [MenuItem("Conveyor Chef/UI Panels/Edit Lose Panel", false, 11)]
        private static void EditLosePanel()
        {
            UIGameOver page = OpenPage<UIGameOver>();
            if (page != null)
                ShowOnly(page, FindPage<UIComplete>());
        }

        [MenuItem("Conveyor Chef/UI Panels/Hide Win and Lose Panels", false, 30)]
        private static void HidePanels()
        {
            if (Application.isPlaying)
                return;

            SetCanvas(FindPage<UIComplete>(), false);
            SetCanvas(FindPage<UIGameOver>(), false);

            // Back to how the scene is normally saved (HUD preview without the popups' canvas).
            UIController controller = Object.FindFirstObjectByType<UIController>(FindObjectsInactive.Include);
            if (controller != null && controller.gameObject.activeSelf)
            {
                Undo.RecordObject(controller.gameObject, "Hide UI Main Canvas");
                controller.gameObject.SetActive(false);
                EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
            }
        }

        private static T OpenPage<T>() where T : Component
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("UI Panels", "Stop Play mode first, then edit the panel.", "OK");
                return null;
            }

            if (FindPage<T>() == null)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    return null;
                EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
            }

            T page = FindPage<T>();
            if (page == null)
            {
                Debug.LogWarning("[UI Panels] " + typeof(T).Name + " was not found in " + GameScenePath);
                return null;
            }

            // Its parent, UI Main Canvas, is saved switched off.
            Transform parent = page.transform.parent;
            if (parent != null && !parent.gameObject.activeSelf)
            {
                Undo.RecordObject(parent.gameObject, "Show UI Main Canvas");
                parent.gameObject.SetActive(true);
            }
            if (!page.gameObject.activeSelf)
            {
                Undo.RecordObject(page.gameObject, "Show page");
                page.gameObject.SetActive(true);
            }
            return page;
        }

        private static void ShowOnly(Component page, Component other)
        {
            SetCanvas(other, false);
            SetCanvas(page, true);

            Selection.activeGameObject = page.gameObject;
            EditorGUIUtility.PingObject(page.gameObject);
            if (SceneView.lastActiveSceneView != null)
                SceneView.lastActiveSceneView.FrameSelected();
            EditorSceneManager.MarkSceneDirty(page.gameObject.scene);
        }

        private static void SetCanvas(Component page, bool enabled)
        {
            Canvas canvas = page != null ? page.GetComponent<Canvas>() : null;
            if (canvas == null || canvas.enabled == enabled)
                return;
            Undo.RecordObject(canvas, enabled ? "Show panel" : "Hide panel");
            canvas.enabled = enabled;
            EditorSceneManager.MarkSceneDirty(page.gameObject.scene);
        }

        private static T FindPage<T>() where T : Component
        {
            T[] pages = Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            return pages.Length > 0 ? pages[0] : null;
        }

        private static Transform FindDeep(Transform root, string name)
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == name)
                    return t;
            }
            return null;
        }
    }
}
