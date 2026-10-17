using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
#if CC_UGS_AUTH && CC_UGS_CLOUDSAVE
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.Core;
#endif
#if CC_PLAY_GAMES && UNITY_ANDROID && !UNITY_EDITOR
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

namespace ConveyorChef.Cloud
{
    /// <summary>
    /// The online side of cloud saves: sign-in and player data storage.
    ///
    /// * Unity Gaming Services: Authentication (com.unity.services.authentication) and
    ///   Cloud Save (com.unity.services.cloudsave). The project must be linked to a Unity
    ///   Cloud project (Project Settings > Services) for them to work.
    /// * Google Play Games (com.google.play.games, optional): on Android the player is signed
    ///   in with their Google account, so their progress follows them to a new phone. Without
    ///   it (or in the Editor) the player gets an anonymous account on this device.
    ///
    /// Each part is compiled only when its package is installed (version defines in
    /// ConveyorChef.Cloud.asmdef), so the game builds with or without them; without the
    /// packages every call simply reports "not available".
    /// </summary>
    public static class CloudBackend
    {
        public static bool Available =>
#if CC_UGS_AUTH && CC_UGS_CLOUDSAVE
            true;
#else
            false;
#endif

        public static bool SignedIn { get; private set; }

        /// <summary>True when signed in with a Google Play Games account (not anonymous).</summary>
        public static bool GoogleAccount { get; private set; }

        /// <summary>The Google Play Games name, when signed in with Google.</summary>
        public static string PlayerName { get; private set; }

        public static string LastError { get; private set; }

        public static async Task<bool> SignInAsync()
        {
#if CC_UGS_AUTH && CC_UGS_CLOUDSAVE
            try
            {
                if (UnityServices.State == ServicesInitializationState.Uninitialized)
                    await UnityServices.InitializeAsync();
                while (UnityServices.State == ServicesInitializationState.Initializing)
                    await Task.Yield();

                if (AuthenticationService.Instance.IsSignedIn)
                {
                    SignedIn = true;
                    return true;
                }

#if CC_PLAY_GAMES && UNITY_ANDROID && !UNITY_EDITOR
                string code = await GooglePlayServerCode();
                if (!string.IsNullOrEmpty(code))
                {
                    await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(code);
                    GoogleAccount = true;
                    SignedIn = true;
                    return true;
                }
#endif
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                SignedIn = true;
                return true;
            }
            catch (Exception ex)
            {
                Fail("sign-in", ex);
                return false;
            }
#else
            await Task.Yield();
            LastError = "Cloud Save packages are not installed.";
            return false;
#endif
        }

        /// <summary>The stored text under <paramref name="key"/>, or null if there is none.</summary>
        public static async Task<string> LoadAsync(string key)
        {
#if CC_UGS_AUTH && CC_UGS_CLOUDSAVE
            if (!SignedIn)
                return null;
            var data = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { key });
            return data != null && data.TryGetValue(key, out var item) && item != null ? item.Value.GetAs<string>() : null;
#else
            await Task.Yield();
            return null;
#endif
        }

        /// <summary>Stores each text under its key (one request).</summary>
        public static async Task SaveAsync(Dictionary<string, string> values)
        {
#if CC_UGS_AUTH && CC_UGS_CLOUDSAVE
            if (!SignedIn || values == null || values.Count == 0)
                return;
            var data = new Dictionary<string, object>();
            foreach (KeyValuePair<string, string> pair in values)
                data[pair.Key] = pair.Value;
            await CloudSaveService.Instance.Data.Player.SaveAsync(data);
#else
            await Task.Yield();
#endif
        }

#if CC_PLAY_GAMES && UNITY_ANDROID && !UNITY_EDITOR
        // Google Play Games v2 signs the player in by itself; the server code lets Unity
        // Authentication sign in to the account linked to that Google player.
        private static Task<string> GooglePlayServerCode()
        {
            var done = new TaskCompletionSource<string>();
            try
            {
                PlayGamesPlatform.Activate();
                PlayGamesPlatform.Instance.Authenticate(status =>
                {
                    if (status != SignInStatus.Success)
                    {
                        Debug.Log("[Cloud] Google Play Games sign-in: " + status);
                        done.TrySetResult(null);
                        return;
                    }
                    PlayerName = PlayGamesPlatform.Instance.GetUserDisplayName();
                    PlayGamesPlatform.Instance.RequestServerSideAccess(true, code => done.TrySetResult(code));
                });
            }
            catch (Exception ex)
            {
                Fail("Google Play Games", ex);
                done.TrySetResult(null);
            }
            return done.Task;
        }
#endif

        private static void Fail(string what, Exception ex)
        {
            LastError = ex.Message;
            Debug.LogWarning("[Cloud] " + what + " failed: " + ex.Message);
        }
    }
}
