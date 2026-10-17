using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// Turns Google Play Games sign-in for cloud saves on or off by itself: when the Google
    /// Play Games plugin is in the project (imported from GooglePlayGamesPlugin-x.unitypackage)
    /// the Android build gets the CC_PLAY_GAMES define, which CloudBackend uses; when the plugin
    /// is removed the define goes too, so the game always compiles.
    /// </summary>
    [InitializeOnLoad]
    public static class PlayGamesDefine
    {
        private const string Define = "CC_PLAY_GAMES";
        private const string PluginType = "GooglePlayGames.PlayGamesPlatform";

        static PlayGamesDefine()
        {
            EditorApplication.delayCall += Sync;
        }

        private static void Sync()
        {
            bool installed = PluginInstalled();
            NamedBuildTarget target = NamedBuildTarget.Android;
            PlayerSettings.GetScriptingDefineSymbols(target, out string[] defines);
            var list = new List<string>(defines);
            if (installed == list.Contains(Define))
                return;

            if (installed)
                list.Add(Define);
            else
                list.Remove(Define);
            PlayerSettings.SetScriptingDefineSymbols(target, list.ToArray());
            Debug.Log("[Cloud] Google Play Games " + (installed ? "found: Google sign-in is on for Android." : "not found: Google sign-in is off."));
        }

        private static bool PluginInstalled()
        {
            foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetType(PluginType, false) != null)
                    return true;
            }
            return false;
        }
    }
}
