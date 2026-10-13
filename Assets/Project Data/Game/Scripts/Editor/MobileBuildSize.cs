using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// Android build settings that keep the APK small without changing how anything looks:
    /// - Textures use ASTC (modern format, supported by all Android devices this game targets,
    ///   min SDK 26). The old ETC default stored every transparent picture as ETC2 RGBA8, about
    ///   2.25x bigger than ASTC at the same visual quality.
    /// - 64-bit ARM only. Google Play requires ARM64, and including 32-bit ARMv7 as well added a
    ///   second copy of the game code to the APK.
    /// (Set Compression Method to LZ4HC in Build Settings too; Unity has no script API for it.)
    /// Applied once by itself; run again from Conveyor Chef > Build > Apply Small APK Settings.
    /// </summary>
    [InitializeOnLoad]
    public static class MobileBuildSize
    {
        private const string AppliedKey = "ConveyorChef_SmallApkSettings_v1";

        static MobileBuildSize()
        {
            if (EditorPrefs.GetBool(AppliedKey, false))
                return;
            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                    return;
                Apply();
                EditorPrefs.SetBool(AppliedKey, true);
            };
        }

        [MenuItem("Conveyor Chef/Build/Apply Small APK Settings", priority = 1)]
        public static void Apply()
        {
            PlayerSettings.Android.textureCompressionFormats = new[] { TextureCompressionFormat.ASTC };
            EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            AssetDatabase.SaveAssets();

            Debug.Log("[Build Size] Android set to ASTC textures and ARM64 only. Set Compression Method to LZ4HC in Build Settings." +
                      (EditorUserBuildSettings.development
                          ? "  NOTE: 'Development Build' is ON in Build Settings; turn it off for the release APK (it is bigger and slower)."
                          : ""));
        }
    }
}
