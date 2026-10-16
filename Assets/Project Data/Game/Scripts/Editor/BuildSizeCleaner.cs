using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// Conveyor Chef > Build > Shrink Build Size (no visual change). Two clean-ups that make
    /// the APK smaller without changing anything on screen:
    /// 1. Material texture slots the material's shader does not have. Unity still packs those
    ///    textures into the build. Typical case: the kitchen pack's materials point at colour,
    ///    normal, metallic and AO maps, but they use the flat Toon shader, which reads none of
    ///    them (about 70 MB of textures). Only slots the shader cannot read are cleared, as
    ///    reported by Unity (Material.HasProperty), so nothing that is drawn changes.
    /// 2. Mipmaps on the World art pictures (Resources/World). They are only drawn flat in
    ///    the UI, where mipmaps are never used; they add a third to each picture. Most of
    ///    them already had mipmaps off; this makes the rest match.
    /// Offered once by itself after compiling; safe to run again.
    /// </summary>
    [InitializeOnLoad]
    public static class BuildSizeCleaner
    {
        private const string AutoRunKey = "CC_BuildSizeCleaner_AutoRun_v1";
        private const string WorldArtFolder = "Assets/Project Data/Game/Resources/World";

        static BuildSizeCleaner()
        {
            if (EditorPrefs.GetBool(AutoRunKey, false))
                return;
            EditorApplication.delayCall += () =>
            {
                if (Application.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
                    return;
                EditorPrefs.SetBool(AutoRunKey, true);
                if (EditorUtility.DisplayDialog("Build size",
                        "Make the APK smaller without changing how the game looks?\n\n" +
                        "- remove texture links that materials' shaders never use (about 90 MB of textures)\n" +
                        "- switch off mipmaps on World UI pictures\n\n(Later: Conveyor Chef > Build > Shrink Build Size)",
                        "Shrink now", "Not now"))
                    Run();
            };
        }

        [MenuItem("Conveyor Chef/Build/Shrink Build Size (no visual change)", false, 30)]
        public static void Run()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Build size", "Stop Play mode first.", "OK");
                return;
            }

            int slots = 0, materials = 0;
            var freed = new HashSet<string>();
            string[] guids = AssetDatabase.FindAssets("t:Material", new[] { "Assets" });
            try
            {
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    if (i % 50 == 0)
                        EditorUtility.DisplayProgressBar("Build size", "Materials: " + path, (float)i / guids.Length);
                    var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                    // A missing or broken shader has no properties at all: leave those alone.
                    if (material == null || material.shader == null || !material.shader.isSupported ||
                        material.shader.name == "Hidden/InternalErrorShader" || material.shader.GetPropertyCount() == 0)
                        continue;

                    int cleared = ClearUnusedTextureSlots(material, freed);
                    if (cleared > 0)
                    {
                        slots += cleared;
                        materials++;
                        EditorUtility.SetDirty(material);
                    }
                }

                int mips = 0;
                string[] textures = AssetDatabase.FindAssets("t:Texture2D", new[] { WorldArtFolder });
                AssetDatabase.StartAssetEditing();
                try
                {
                    for (int i = 0; i < textures.Length; i++)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(textures[i]);
                        if (i % 20 == 0)
                            EditorUtility.DisplayProgressBar("Build size", "Mipmaps: " + path, (float)i / textures.Length);
                        if (AssetImporter.GetAtPath(path) is TextureImporter importer &&
                            importer.textureType == TextureImporterType.Sprite && importer.mipmapEnabled)
                        {
                            importer.mipmapEnabled = false;
                            importer.SaveAndReimport();
                            mips++;
                        }
                    }
                }
                finally
                {
                    AssetDatabase.StopAssetEditing();
                }

                AssetDatabase.SaveAssets();
                string report = "Cleared " + slots + " unused texture links in " + materials + " materials (" + freed.Count + " textures no longer forced into the build).\n" +
                                "Mipmaps switched off on " + mips + " World UI pictures.";
                Debug.Log("[Build size] " + report.Replace("\n", " "));
                EditorUtility.DisplayDialog("Build size", report, "OK");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        // Clears m_TexEnvs entries whose property the shader does not have.
        private static int ClearUnusedTextureSlots(Material material, HashSet<string> freed)
        {
            var so = new SerializedObject(material);
            SerializedProperty texEnvs = so.FindProperty("m_SavedProperties.m_TexEnvs");
            if (texEnvs == null || !texEnvs.isArray)
                return 0;

            int cleared = 0;
            for (int i = 0; i < texEnvs.arraySize; i++)
            {
                SerializedProperty entry = texEnvs.GetArrayElementAtIndex(i);
                string name = entry.FindPropertyRelative("first").stringValue;
                SerializedProperty texture = entry.FindPropertyRelative("second.m_Texture");
                if (texture == null || texture.objectReferenceValue == null || material.HasProperty(name))
                    continue;
                freed.Add(AssetDatabase.GetAssetPath(texture.objectReferenceValue));
                texture.objectReferenceValue = null;
                cleared++;
            }
            if (cleared > 0)
                so.ApplyModifiedPropertiesWithoutUndo();
            return cleared;
        }
    }
}
