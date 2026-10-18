using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ConveyorChef.Cloud;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Watermelon
{
    /// <summary>
    /// Keeps the player's progress in the cloud (see <see cref="CloudBackend"/>):
    /// * at start it signs in (Google Play Games on Android when installed) and reads the
    ///   cloud copy. If that copy is further along than this phone (a new phone, a reinstall)
    ///   it asks "Load it?"; yes restores it and restarts the game, no keeps this phone's
    ///   progress (the cloud copy is kept aside as "progress_previous").
    /// * after that it uploads the progress whenever it changed: when a menu screen opens,
    ///   when the app goes to the background, and every few minutes on menu screens - never
    ///   during a level, so play stays smooth.
    /// Without the cloud packages, or offline, nothing happens and the game works as before.
    /// </summary>
    public sealed class CloudSaveSync : MonoBehaviour
    {
        private const string CloudKey = "progress";
        private const string PreviousKey = "progress_previous";
        private const float UploadEvery = 180f;
        private const string GameScene = "Game";

        private static CloudSaveSync instance;

        private CloudSnapshot uploaded;     // what the cloud holds now
        private bool ready, busy;
        private float nextUpload;

        public static bool SignedIn => CloudBackend.SignedIn;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (instance != null || !CloudBackend.Available)
                return;
            var go = new GameObject("Cloud Save");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<CloudSaveSync>();
        }

        private IEnumerator Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            while (!SaveController.IsSaveLoaded)
                yield return null;
            yield return new WaitForSecondsRealtime(1f);
            Task first = FirstSync();
            while (!first.IsCompleted)
                yield return null;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private async Task FirstSync()
        {
            if (!await CloudBackend.SignInAsync())
                return;

            CloudSnapshot cloud;
            try
            {
                cloud = CloudSnapshot.FromJson(await CloudBackend.LoadAsync(CloudKey));
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[Cloud] Could not read the cloud save: " + ex.Message);
                return;
            }

            CloudSnapshot local = CloudSnapshot.Capture();
            if (local == null)
                return;

            if (cloud != null && cloud.IsAheadOf(local))
            {
                bool? load = null;
                StartCoroutine(AskWhenOnMenu(cloud, answer => load = answer));
                while (!load.HasValue)
                    await Task.Yield();

                if (load.Value)
                {
                    cloud.Restore();
                    Restart();
                    return;
                }
                // keep this phone's progress; the older cloud copy is kept aside, not lost
                await Upload(local, new Dictionary<string, string> { [PreviousKey] = cloud.ToJson() });
            }
            else if (cloud == null || !cloud.SameProgress(local))
            {
                await Upload(local, null);
            }
            else
            {
                uploaded = cloud;
            }

            ready = true;
            nextUpload = Time.unscaledTime + UploadEvery;
        }

        // ------------------------------------------------------------------ uploads

        private void Update()
        {
            if (ready && Time.unscaledTime >= nextUpload && !InLevel())
                UploadIfChanged();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (ready && scene.name != GameScene)
                nextUpload = Mathf.Min(nextUpload, Time.unscaledTime + 2f);   // just after the screen settles
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
                UploadIfChanged();
        }

        private async void UploadIfChanged()
        {
            nextUpload = Time.unscaledTime + UploadEvery;
            if (!ready || busy || SaveController.IsLocked)
                return;
            CloudSnapshot local = CloudSnapshot.Capture();
            if (local == null || local.SameProgress(uploaded))
                return;
            await Upload(local, null);
        }

        private async Task Upload(CloudSnapshot local, Dictionary<string, string> extra)
        {
            busy = true;
            try
            {
                var values = extra ?? new Dictionary<string, string>();
                values[CloudKey] = local.ToJson();
                await CloudBackend.SaveAsync(values);
                uploaded = local;
                Debug.Log("[Cloud] Progress saved (" + local.levels + " levels).");
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[Cloud] Upload failed, will retry: " + ex.Message);
            }
            finally
            {
                busy = false;
            }
        }

        private static bool InLevel() => SceneManager.GetActiveScene().name == GameScene;

        // ------------------------------------------------------------------ restart

        // The restored save is read when the game starts, so the game starts again.
        private static void Restart()
        {
#if UNITY_EDITOR
            Debug.Log("[Cloud] Progress restored. Press Play again to load it.");
            UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_ANDROID
            try
            {
                using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (AndroidJavaObject activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
                using (AndroidJavaObject packages = activity.Call<AndroidJavaObject>("getPackageManager"))
                using (AndroidJavaObject intent = packages.Call<AndroidJavaObject>("getLaunchIntentForPackage", activity.Call<string>("getPackageName")))
                {
                    intent.Call<AndroidJavaObject>("addFlags", 0x10000000 | 0x00008000);   // NEW_TASK | CLEAR_TASK
                    activity.Call("startActivity", intent);
                    using (var system = new AndroidJavaClass("java.lang.System"))
                        system.CallStatic("exit", 0);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[Cloud] Restart failed: " + ex.Message);
                Application.Quit();
            }
#else
            Application.Quit();
#endif
        }

        // ------------------------------------------------------------------ "load it?" popup

        private IEnumerator AskWhenOnMenu(CloudSnapshot cloud, Action<bool> answer)
        {
            // not during a level, nor on the start-up / loading screens
            while (InLevel() || SceneManager.GetActiveScene().buildIndex < 2)
                yield return new WaitForSecondsRealtime(0.5f);
            BuildPopup(cloud, answer);
        }

        private void BuildPopup(CloudSnapshot cloud, Action<bool> answer)
        {
            MenuPanelArt art = Resources.Load<MenuPanelArt>(MenuPanelArt.ResourcePath);
            if (MenuUI.Font == null && art != null)
                MenuUI.Font = art.font;

            var root = new GameObject("Cloud Save Popup", typeof(RectTransform));
            root.transform.SetParent(transform, false);
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;
            root.AddComponent<GraphicRaycaster>();

            Image dim = MenuUI.Image("Dim", root.transform, null, new Color(0f, 0f, 0f, 0.65f), false);
            MenuUI.Stretch(dim.rectTransform);
            dim.raycastTarget = true;   // the screen behind can't be tapped

            Image frame = MenuUI.Image("Frame", root.transform, art != null ? art.frame : null,
                art != null && art.frame != null ? (Color?)null : MenuUI.CardFill, false);
            frame.pixelsPerUnitMultiplier = 1.25f;
            frame.raycastTarget = true;
            MenuUI.Place(frame.rectTransform, new Vector2(0f, -20f), new Vector2(900f, 760f));

            Image ribbon = MenuUI.Image("Title Ribbon", frame.transform, art != null ? art.titleRibbon : null);
            MenuUI.Anchor(ribbon.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -40f), new Vector2(760f, 204f));
            TextMeshProUGUI title = MenuUI.OutlinedText("Title", ribbon.transform, "CLOUD SAVE", 62f, new Color32(110, 20, 20, 255));
            MenuUI.Stretch(title.rectTransform, 150f, 70f, 150f, 50f);

            Image chef = MenuUI.Image("Chef", frame.transform, ChefProgress.Avatar(art));
            MenuUI.Anchor(chef.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -150f), new Vector2(150f, 150f));

            string who = CloudBackend.GoogleAccount && !string.IsNullOrEmpty(CloudBackend.PlayerName)
                ? "\n<size=80%>" + CloudBackend.PlayerName + "</size>" : string.Empty;
            TextMeshProUGUI message = MenuUI.Text("Message", frame.transform,
                "Saved progress found!" + who + "\n<b>" + cloud.levels + " levels  -  " + cloud.stars + " stars</b>\nLoad it on this phone?",
                44f, MenuUI.TextDark, TextAlignmentOptions.Center, true);
            MenuUI.Anchor(message.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -315f), new Vector2(760f, 230f));

            void Close(bool load)
            {
                Destroy(root);
                answer(load);
            }

            Button no = MenuUI.LabelButton("Keep", frame.transform, art != null ? art.buttonOrange : null, "NO", 50f,
                () => Close(false), null, new Color32(110, 50, 10, 255));
            MenuUI.Anchor((RectTransform)no.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-190f, 70f), new Vector2(320f, 120f));
            Button yes = MenuUI.LabelButton("Load", frame.transform, art != null ? art.buttonGreen : null, "LOAD", 50f,
                () => Close(true));
            MenuUI.Anchor((RectTransform)yes.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(190f, 70f), new Vector2(320f, 120f));
        }
    }
}
