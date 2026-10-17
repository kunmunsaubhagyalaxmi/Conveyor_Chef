using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    /// <summary>Conveyor Chef > Tutorial > Show Tutorial Again: forgets which lessons and tips were seen.</summary>
    public static class TutorialReset
    {
        private static readonly string[] Keys =
        {
            "CC_Tutorial_Level1", "CC_Tutorial_Skipped", "CC_Tutorial_Hidden", "CC_Tutorial_Spawner",
            "CC_Tutorial_PowerUp_PUUndoSettings", "CC_Tutorial_PowerUp_PUHintSettings", "CC_Tutorial_PowerUp_PUShuffleSettings",
        };

        [MenuItem("Conveyor Chef/Tutorial/Show Tutorial Again", false, 40)]
        private static void ShowAgain()
        {
            foreach (string key in Keys)
                PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
            Debug.Log("[Tutorial] Reset: the level 1 lesson and every tip will show again. Play level 1 to see the lesson.");
        }
    }
}
