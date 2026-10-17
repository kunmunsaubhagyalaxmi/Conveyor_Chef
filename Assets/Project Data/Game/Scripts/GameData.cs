using UnityEngine;
using Watermelon.SkinStore;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Game Data", menuName = "Content/Game Data")]
    public class GameData : ScriptableObject
    {
        [SerializeField] bool activateVehicles;
        public bool ActivateVehicles => activateVehicles;

#if UNITY_EDITOR
        [System.NonSerialized] bool? appliedVehicles;

        // Loading the skins database inside OnValidate made Unity log thousands of
        // "SendMessage cannot be called during ... OnValidate" warnings on every import / build,
        // so the tab state is applied a moment later, and only when the toggle changed.
        public void OnValidate()
        {
            if (appliedVehicles == activateVehicles)
                return;
            appliedVehicles = activateVehicles;
            UnityEditor.EditorApplication.delayCall -= ApplyTabState;
            UnityEditor.EditorApplication.delayCall += ApplyTabState;
        }

        private void ApplyTabState()
        {
            if (this == null || UnityEditor.BuildPipeline.isBuildingPlayer)
                return;
            SkinStoreHelper.SetTabState(SkinTab.Bus, activateVehicles);
        }
#endif
    }
}
