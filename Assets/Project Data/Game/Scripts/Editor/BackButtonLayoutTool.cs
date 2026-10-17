using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Watermelon
{
    /// <summary>
    /// Conveyor Chef > UI > Line Up Top Bars: gives the Country Map (7 continents) the
    /// standard top bar (<see cref="BackButtonLayout"/>): BACK
    /// button, settings button and coin / gem pills at the Country Map's places and sizes,
    /// then saves the scenes. The World Map keeps its own. The result is ordinary scene data,
    /// visible and editable in the Hierarchy and Inspector.
    /// It also offers to run once by itself after this script is first compiled.
    /// </summary>
    [InitializeOnLoad]
    public static class BackButtonLayoutTool
    {
        private const string AutoRunKey = "CC_TopBarLayout_AutoRun_v1";

        private static readonly string[] ScenePaths =
        {
            "Assets/Project Data/Game/Scenes/CountryMap.unity",
            // Level Selection is left out: its 35 countries share China's hand-placed top bar
        };

        static BackButtonLayoutTool()
        {
            if (EditorPrefs.GetBool(AutoRunKey, false))
                return;
            EditorApplication.delayCall += () =>
            {
                if (Application.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
                    return;
                EditorPrefs.SetBool(AutoRunKey, true);
                if (EditorUtility.DisplayDialog("Top bars",
                        "Line up the BACK button, coins, gems and settings in the Country Map and Level Selection scenes " +
                        "to match the Country Map? Both scenes are saved afterwards.\n\n(Later: Conveyor Chef > UI > Line Up Top Bars)",
                        "Line up now", "Not now"))
                    LineUp();
            };
        }

        [MenuItem("Conveyor Chef/UI/Line Up Top Bars", false, 20)]
        private static void LineUp()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Top bars", "Stop Play mode first.", "OK");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            string returnTo = SceneManager.GetActiveScene().path;
            var report = new System.Text.StringBuilder();
            foreach (string path in ScenePaths)
            {
                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                Canvas.ForceUpdateCanvases();
                int backs = 0, settings = 0, pills = 0;
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    foreach (RectTransform rect in root.GetComponentsInChildren<RectTransform>(true))
                    {
                        if (BackButtonLayout.IsBackButton(rect))
                        {
                            Record(rect);
                            if (BackButtonLayout.Apply(rect)) backs++;
                            Recorded(rect);
                        }
                    }

                    // Level Selection: each country's Top HUD gets the Country Map's right side.
                    if (!path.EndsWith("LevelSelection.unity"))
                        continue;
                    foreach (RectTransform hud in root.GetComponentsInChildren<RectTransform>(true))
                    {
                        if (!BackButtonLayout.Matches(hud, "tophud"))
                            continue;
                        foreach (RectTransform item in hud)
                        {
                            string n = item.name.Replace(" ", "").ToLowerInvariant();
                            Record(item);
                            if (n == "settingsbutton")
                            {
                                if (BackButtonLayout.Place(item, true, BackButtonLayout.SettingsFromTopRight, BackButtonLayout.SettingsSize)) settings++;
                            }
                            else if (n == "diamondcounter" || n == "coincounter")
                            {
                                Vector2 offset = BackButtonLayout.PillFromTopRight;
                                if (n == "coincounter")
                                    offset.x -= BackButtonLayout.PillSize.x + BackButtonLayout.PillGap;   // coins left of gems
                                if (BackButtonLayout.Place(item, true, offset, BackButtonLayout.PillSize))
                                {
                                    foreach (RectTransform child in item)
                                        Record(child);
                                    BackButtonLayout.LayoutPill(item);
                                    foreach (RectTransform child in item)
                                        Recorded(child);
                                    pills++;
                                }
                            }
                            Recorded(item);
                        }
                    }
                }
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                report.AppendLine(System.IO.Path.GetFileNameWithoutExtension(path) + ": " + backs + " back buttons" +
                                  (settings + pills > 0 ? ", " + settings + " settings buttons, " + pills + " coin/gem pills" : ""));
            }

            if (!string.IsNullOrEmpty(returnTo) && System.Array.IndexOf(ScenePaths, returnTo) < 0)
                EditorSceneManager.OpenScene(returnTo, OpenSceneMode.Single);
            Debug.Log("[Top bars] " + report.ToString().Trim().Replace("\n", " | "));
            EditorUtility.DisplayDialog("Top bars", report.ToString(), "OK");
        }

        private static void Record(Object o) => Undo.RecordObject(o, "Line up top bar");

        private static void Recorded(Component c)
        {
            if (PrefabUtility.IsPartOfPrefabInstance(c))
                PrefabUtility.RecordPrefabInstancePropertyModifications(c);
        }
    }
}
