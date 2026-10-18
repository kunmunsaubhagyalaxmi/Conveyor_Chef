using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// Keeps the Android build as small as it can be without touching quality:
    /// * the game data is packed with LZ4HC instead of LZ4 (same files, tighter packing; loads
    ///   just as fast, the build takes a little longer);
    /// * an App Bundle (.aab, for Google Play) puts the game data in an install-time asset
    ///   pack ("Split Application Binary"), so the 200 MB base-module limit is never hit; an
    ///   .apk stays one single file that installs anywhere (phone, BlueStacks).
    /// </summary>
    [InitializeOnLoad]
    public sealed class AndroidBuildSize : IPreprocessBuildWithReport
    {
        public int callbackOrder => -100;

        static AndroidBuildSize()
        {
            EditorApplication.delayCall += UseLz4HC;
        }

        private static void UseLz4HC()
        {
            try
            {
                Type settings = typeof(EditorUserBuildSettings);
                const BindingFlags Flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
                MethodInfo get = settings.GetMethod("GetCompressionType", Flags, null, new[] { typeof(BuildTargetGroup) }, null);
                MethodInfo set = null;
                foreach (MethodInfo m in settings.GetMethods(Flags))
                    if (m.Name == "SetCompressionType" && m.GetParameters().Length == 2 && m.GetParameters()[0].ParameterType == typeof(BuildTargetGroup))
                        set = m;
                if (get == null || set == null)
                    return;

                object lz4hc = Enum.Parse(set.GetParameters()[1].ParameterType, "Lz4HC");
                if (Equals(get.Invoke(null, new object[] { BuildTargetGroup.Android }), lz4hc))
                    return;
                set.Invoke(null, new[] { (object)BuildTargetGroup.Android, lz4hc });
                Debug.Log("[Build Size] Android compression set to LZ4HC (smaller build, same quality).");
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[Build Size] Could not set LZ4HC, set it in Build Profiles > Android > Compression Method: " + ex.Message);
            }
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.Android)
                return;
            UseLz4HC();
            bool bundle = EditorUserBuildSettings.buildAppBundle;
            if (PlayerSettings.Android.splitApplicationBinary != bundle)
            {
                PlayerSettings.Android.splitApplicationBinary = bundle;
                Debug.Log("[Build Size] Split Application Binary " + (bundle ? "on (App Bundle: game data in an install-time asset pack)." : "off (APK stays a single file)."));
            }
        }
    }
}
