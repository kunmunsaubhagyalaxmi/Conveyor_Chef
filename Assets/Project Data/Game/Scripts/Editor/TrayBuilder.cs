using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Watermelon.BusStop;
using Watermelon.SkinStore;

namespace Watermelon
{
    /// <summary>
    /// Conveyor Chef > Trays > Build Trays: makes the serving trays the customers carry.
    ///
    /// For every design (Snack Tray, Royal Platter, Bento Box, Serving Cart) it builds:
    ///   - the 3D shape (a mesh asset, made here; no outside model needed),
    ///   - one prefab per order colour (10), working like the old buses: BusBehavior,
    ///     BusStateMachine, three seats for the dishes, a little tilt when it starts/stops,
    ///   - "Tray &lt;Design&gt; Data.prefab" (BusData: colour -> prefab), the skin's prefab,
    ///   - a preview picture for Customize > TRAYS (Images/Skins/Trays),
    /// and puts them in the Skins Database before the buses, so the first design is the
    /// default tray. The buses stay as skins (renamed Food Bus / Snack Van).
    /// Running it again rebuilds everything in place (same files, same skin ids).
    /// </summary>
    public static class TrayBuilder
    {
        private const string Root = "Assets/Project Data/Game/Prefabs/Vehicles/Trays/";
        private const string MeshFolder = "Assets/Project Data/Game/Models/Trays/";
        private const string MaterialFolder = "Assets/Project Data/Game/Materials/Trays/";
        private const string PreviewFolder = "Assets/Project Data/Game/Images/Skins/Trays/";
        private const string AnimFolder = "Assets/Project Data/Game/Animations/Trays/";
        private const string BaseMaterial = "Assets/Project Data/Game/Prefabs/Characters/MAterials/Red 1.mat";
        private const string SkinsDatabasePath = "Assets/Project Data/Content/Data/Skins Database.asset";
        public const string SkinIdPrefix = "tray_";

        private static readonly string[] ColourNames = { "Red", "Green", "Pink", "Blue", "Yellow", "Teal", "Purple", "Orange", "Brown", "White" };

        // Material slots of every tray mesh.
        private const int Body = 0, Light = 1, Accent = 2, Dark = 3;

        private enum Accents { Gold, Metal, Wood }

        private sealed class Design
        {
            public string Id, Name;
            public int Cost;
            public CurrencyType Currency;
            public Accents Accent;
            public float SeatY;                 // top of the surface the dishes stand on
            public System.Action<MeshMaker> Build;
        }

        // Tray footprint: long side along x (the three dishes), depth along z.
        private const float Length = 3.9f, Depth = 1.7f, SeatSpacing = 1.15f;

        private static readonly Design[] Designs =
        {
            new Design { Id = "snack", Name = "Snack Tray", Cost = 0, Currency = CurrencyType.Coins, Accent = Accents.Metal, SeatY = 0.16f, Build = BuildSnackTray },
            new Design { Id = "platter", Name = "Royal Platter", Cost = 800, Currency = CurrencyType.Coins, Accent = Accents.Gold, SeatY = 0.15f, Build = BuildPlatter },
            new Design { Id = "bento", Name = "Bento Box", Cost = 1500, Currency = CurrencyType.Coins, Accent = Accents.Wood, SeatY = 0.13f, Build = BuildBento },
            new Design { Id = "cart", Name = "Serving Cart", Cost = 25, Currency = CurrencyType.Diamonds, Accent = Accents.Metal, SeatY = 1.06f, Build = BuildCart },
        };

        [MenuItem("Conveyor Chef/Trays/Build Trays", false, 1)]
        public static void BuildAll()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Trays", "Stop Play mode first.", "OK");
                return;
            }

            foreach (string folder in new[] { Root, MeshFolder, MaterialFolder, PreviewFolder, AnimFolder })
                Directory.CreateDirectory(folder);

            try
            {
                RuntimeAnimatorController animator = BuildAnimator();
                Material[] accents = BuildAccentMaterials();
                Material[][] colours = BuildColourMaterials();

                var dataPrefabs = new GameObject[Designs.Length];
                for (int d = 0; d < Designs.Length; d++)
                {
                    EditorUtility.DisplayProgressBar("Trays", Designs[d].Name, (float)d / Designs.Length);
                    dataPrefabs[d] = BuildDesign(Designs[d], animator, accents, colours);
                }
                AssetDatabase.SaveAssets();

                var previews = new Sprite[Designs.Length];
                for (int d = 0; d < Designs.Length; d++)
                {
                    EditorUtility.DisplayProgressBar("Trays", "Preview: " + Designs[d].Name, (float)d / Designs.Length);
                    previews[d] = RenderPreview(Designs[d]);
                }

                RegisterSkins(dataPrefabs, previews);
                AssetDatabase.SaveAssets();
                Debug.Log("[Trays] Built " + Designs.Length + " tray designs x " + ColourNames.Length + " colours. Default: " + Designs[0].Name);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        #region Designs

        private static void BuildSnackTray(MeshMaker m)
        {
            // Cafeteria tray: thick coloured base, raised rim, lighter floor, steel corner feet.
            const float radius = 0.34f, rim = 0.13f;
            List<Vector2> outer = RoundedRect(Length, Depth, radius, 6);
            List<Vector2> inner = RoundedRect(Length - 2f * rim, Depth - 2f * rim, radius - rim, 6);
            const float y0 = 0.04f, floor = 0.14f, top = 0.30f;
            m.Wall(outer, y0, top, true, Body);
            m.Cap(outer, y0, false, Body);
            m.Ring(outer, inner, top, Body);
            m.Wall(inner, floor, top, false, Body);
            m.Cap(inner, floor, true, Light);
            foreach (Vector2 c in Corners(Length - 0.7f, Depth - 0.5f))
                m.Cylinder(new Vector3(c.x, 0f, c.y), 0.09f, 0.05f, Vector3.up, 12, Accent);
        }

        private static void BuildPlatter(MeshMaker m)
        {
            // Oval platter with a gold rim and two gold handles at the ends.
            const float a = Length * 0.5f + 0.1f, b = Depth * 0.5f + 0.08f, rim = 0.16f;
            List<Vector2> outer = Ellipse(a, b, 48);
            List<Vector2> lip = Ellipse(a - 0.05f, b - 0.05f, 48);
            List<Vector2> inner = Ellipse(a - rim, b - rim, 48);
            const float y0 = 0f, floor = 0.13f, top = 0.25f;
            m.Wall(Ellipse(a - 0.25f, b - 0.2f, 48), y0, 0.06f, true, Body);   // foot
            m.Cap(Ellipse(a - 0.25f, b - 0.2f, 48), y0, false, Body);
            m.Cap(outer, 0.06f, false, Body);
            m.Wall(outer, 0.06f, top, true, Body);
            m.Ring(outer, lip, top, Accent);
            m.Ring(lip, inner, top, Accent);
            m.Wall(inner, floor, top, false, Body);
            m.Cap(inner, floor, true, Light);
            foreach (float side in new[] { -1f, 1f })
            {
                // handle: a flat gold loop sticking out of each end
                List<Vector2> handleOuter = RoundedRect(0.34f, 0.8f, 0.16f, 5, new Vector2(side * (a + 0.12f), 0f));
                List<Vector2> handleInner = RoundedRect(0.14f, 0.5f, 0.06f, 5, new Vector2(side * (a + 0.14f), 0f));
                m.Ring(handleOuter, handleInner, top - 0.02f, Accent);
                m.Ring(handleInner, handleOuter, top - 0.08f, Accent, true);
                m.Wall(handleOuter, top - 0.08f, top - 0.02f, true, Accent);
                m.Wall(handleInner, top - 0.08f, top - 0.02f, false, Accent);
            }
        }

        private static void BuildBento(MeshMaker m)
        {
            // Lacquered box in the order colour, dark inside, wooden dividers between the dishes.
            const float radius = 0.14f, wall = 0.11f;
            List<Vector2> outer = RoundedRect(Length, Depth, radius, 4);
            List<Vector2> inner = RoundedRect(Length - 2f * wall, Depth - 2f * wall, radius - 0.06f, 4);
            const float y0 = 0f, floor = 0.12f, top = 0.40f;
            m.Wall(outer, y0, top, true, Body);
            m.Cap(outer, y0, false, Body);
            m.Ring(outer, inner, top, Body);
            m.Wall(inner, floor, top, false, Dark);
            m.Cap(inner, floor, true, Dark);
            foreach (float x in new[] { -SeatSpacing * 0.5f, SeatSpacing * 0.5f })
                m.Box(new Vector3(x, (floor + top - 0.06f) * 0.5f, 0f), new Vector3(0.06f, top - 0.06f - floor, Depth - 2f * wall), Accent);
            // lid band: a thin lighter stripe around the top edge
            m.Wall(RoundedRect(Length + 0.02f, Depth + 0.02f, radius + 0.01f, 4), top - 0.08f, top - 0.03f, true, Light);
        }

        private static void BuildCart(MeshMaker m)
        {
            // Tray on a steel trolley: top tray in the order colour, lower shelf, legs, wheels.
            const float radius = 0.24f, rim = 0.12f;
            const float trayBase = 0.92f, floor = 1.02f, top = 1.16f;
            List<Vector2> outer = RoundedRect(Length, Depth, radius, 5);
            List<Vector2> inner = RoundedRect(Length - 2f * rim, Depth - 2f * rim, radius - rim * 0.8f, 5);
            m.Wall(outer, trayBase, top, true, Body);
            m.Cap(outer, trayBase, false, Body);
            m.Ring(outer, inner, top, Body);
            m.Wall(inner, floor, top, false, Body);
            m.Cap(inner, floor, true, Light);

            List<Vector2> shelf = RoundedRect(Length - 0.3f, Depth - 0.25f, 0.15f, 4);
            m.Wall(shelf, 0.34f, 0.40f, true, Accent);
            m.Cap(shelf, 0.40f, true, Accent);
            m.Cap(shelf, 0.34f, false, Accent);

            foreach (Vector2 c in Corners(Length - 0.5f, Depth - 0.4f))
            {
                m.Cylinder(new Vector3(c.x, 0.2f, c.y), 0.05f, trayBase - 0.2f, Vector3.up, 10, Accent);
                m.Cylinder(new Vector3(c.x, 0.14f, c.y - 0.04f), 0.14f, 0.08f, Vector3.forward, 16, Dark);
            }
        }

        private static IEnumerable<Vector2> Corners(float w, float d)
        {
            yield return new Vector2(-w * 0.5f, -d * 0.5f);
            yield return new Vector2(w * 0.5f, -d * 0.5f);
            yield return new Vector2(w * 0.5f, d * 0.5f);
            yield return new Vector2(-w * 0.5f, d * 0.5f);
        }

        #endregion

        #region Prefabs

        private static GameObject BuildDesign(Design design, RuntimeAnimatorController animator, Material[] accents, Material[][] colours)
        {
            string folder = Root + design.Name + "/";
            Directory.CreateDirectory(folder);

            var maker = new MeshMaker();
            design.Build(maker);
            Mesh mesh = maker.ToMesh("Tray " + design.Name);
            mesh = SaveAsset(mesh, MeshFolder + "Tray " + design.Name + ".asset");

            var data = new List<BusData.BusPrefab>();
            for (int c = 0; c < ColourNames.Length; c++)
            {
                var mats = new Material[4];
                mats[Body] = colours[c][0];
                mats[Light] = colours[c][1];
                mats[Dark] = colours[c][2];
                mats[Accent] = accents[(int)design.Accent];

                GameObject tray = BuildTrayObject(design, ColourNames[c], mesh, mats, animator);
                string path = folder + "Tray " + design.Name + " " + ColourNames[c] + ".prefab";
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(tray, path);
                Object.DestroyImmediate(tray);
                data.Add(new BusData.BusPrefab { prefab = prefab, type = (LevelElement.Type)((int)LevelElement.Type.Block_Red + c) });
            }

            var dataObject = new GameObject("Tray " + design.Name + " Data");
            dataObject.AddComponent<BusData>().busData = data;
            GameObject dataPrefab = PrefabUtility.SaveAsPrefabAsset(dataObject, folder + "Tray " + design.Name + " Data.prefab");
            Object.DestroyImmediate(dataObject);
            return dataPrefab;
        }

        private static GameObject BuildTrayObject(Design design, string colour, Mesh mesh, Material[] mats, RuntimeAnimatorController animator)
        {
            var root = new GameObject("Tray " + design.Name + " " + colour);

            var seatsRoot = new GameObject("Seats").transform;
            seatsRoot.SetParent(root.transform, false);
            var seats = new List<Transform>();
            for (int i = 0; i < 3; i++)
            {
                var seat = new GameObject("Seat " + (i + 1)).transform;
                seat.SetParent(seatsRoot, false);
                seat.localPosition = new Vector3((i - 1) * SeatSpacing, design.SeatY, 0f);
                seats.Add(seat);
            }

            var enter = new GameObject("Enter Pos").transform;
            enter.SetParent(root.transform, false);
            enter.localPosition = new Vector3(0f, 0f, -1.363f);

            // Model > Body: the animator tilts Body when the tray starts and stops.
            var model = new GameObject("Model");
            model.transform.SetParent(root.transform, false);
            var body = new GameObject("Body");
            body.transform.SetParent(model.transform, false);
            body.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = body.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = mats;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            Animator modelAnimator = model.AddComponent<Animator>();
            modelAnimator.runtimeAnimatorController = animator;

            // The dishes sit on the seats, so they tilt with the tray too.
            seatsRoot.SetParent(body.transform, true);

            var bus = root.AddComponent<BusBehavior>();
            root.AddComponent<BusStateMachine>();

            var so = new SerializedObject(bus);
            so.FindProperty("enterPosition").objectReferenceValue = enter;
            so.FindProperty("animator").objectReferenceValue = modelAnimator;
            SerializedProperty seatList = so.FindProperty("seats");
            seatList.arraySize = seats.Count;
            for (int i = 0; i < seats.Count; i++)
                seatList.GetArrayElementAtIndex(i).objectReferenceValue = seats[i];
            so.ApplyModifiedPropertiesWithoutUndo();
            return root;
        }

        #endregion

        #region Materials and animation

        private static Material NewMaterial(string name, Color colour, float smoothness)
        {
            string path = MaterialFolder + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Material source = AssetDatabase.LoadAssetAtPath<Material>(BaseMaterial);
            if (material == null)
            {
                material = source != null ? new Material(source) : new Material(UnityEngine.Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(material, path);
            }
            material.name = name;
            foreach (string property in new[] { "_BaseColor", "_Color" })
            {
                if (material.HasProperty(property))
                    material.SetColor(property, colour);
            }
            if (material.HasProperty("_Smoothness"))
                material.SetFloat("_Smoothness", smoothness);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material[][] BuildColourMaterials()
        {
            var result = new Material[ColourNames.Length][];
            for (int c = 0; c < ColourNames.Length; c++)
            {
                Color colour = CountryFood.GetTypeColour((LevelElement.Type)((int)LevelElement.Type.Block_Red + c));
                Color body = Color.Lerp(colour, Color.black, ColourNames[c] == "White" ? 0.08f : 0.12f);
                Color light = Color.Lerp(colour, Color.white, ColourNames[c] == "White" ? 0.4f : 0.55f);
                Color dark = Color.Lerp(colour, Color.black, ColourNames[c] == "White" ? 0.35f : 0.6f);
                result[c] = new[]
                {
                    NewMaterial("Tray_" + ColourNames[c], body, 0.55f),
                    NewMaterial("Tray_" + ColourNames[c] + "_Light", light, 0.35f),
                    NewMaterial("Tray_" + ColourNames[c] + "_Dark", dark, 0.3f),
                };
            }
            return result;
        }

        private static Material[] BuildAccentMaterials()
        {
            return new[]
            {
                NewMaterial("Tray_Gold", new Color32(240, 186, 52, 255), 0.75f),
                NewMaterial("Tray_Metal", new Color32(196, 204, 214, 255), 0.7f),
                NewMaterial("Tray_Wood", new Color32(176, 120, 68, 255), 0.25f),
            };
        }

        private static RuntimeAnimatorController BuildAnimator()
        {
            AnimationClip idle = SaveClip("Tray Idle", new[] { 0f }, new[] { 0f });
            // a short wobble: the load sways when the tray sets off and when it brakes
            AnimationClip start = SaveClip("Tray Start", new[] { 0f, 0.12f, 0.3f, 0.45f, 0.6f }, new[] { 0f, 5f, -2.5f, 1f, 0f });
            AnimationClip brake = SaveClip("Tray Break", new[] { 0f, 0.12f, 0.3f, 0.45f, 0.6f }, new[] { 0f, -6f, 3f, -1f, 0f });

            string path = AnimFolder + "Tray Animator.controller";
            AssetDatabase.DeleteAsset(path);
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            controller.AddParameter("Start", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Break", AnimatorControllerParameterType.Trigger);
            AnimatorStateMachine machine = controller.layers[0].stateMachine;
            AnimatorState idleState = machine.AddState("Idle");
            idleState.motion = idle;
            machine.defaultState = idleState;
            foreach ((string trigger, AnimationClip clip) in new[] { ("Start", start), ("Break", brake) })
            {
                AnimatorState state = machine.AddState(clip.name);
                state.motion = clip;
                AnimatorStateTransition enter = machine.AddAnyStateTransition(state);
                enter.AddCondition(AnimatorConditionMode.If, 0f, trigger);
                enter.duration = 0f;
                AnimatorStateTransition back = state.AddTransition(idleState);
                back.hasExitTime = true;
                back.exitTime = 1f;
                back.duration = 0.05f;
            }
            return controller;
        }

        private static AnimationClip SaveClip(string name, float[] times, float[] tilt)
        {
            var clip = new AnimationClip { name = name };
            var keys = new Keyframe[times.Length];
            for (int i = 0; i < times.Length; i++)
                keys[i] = new Keyframe(times[i], tilt[i]);
            var curve = new AnimationCurve(keys);
            for (int i = 0; i < keys.Length; i++)
                curve.SmoothTangents(i, 0f);
            clip.SetCurve("Body", typeof(Transform), "localEulerAnglesRaw.z", curve);
            clip.SetCurve("Body", typeof(Transform), "localEulerAnglesRaw.x", AnimationCurve.Constant(0f, times[times.Length - 1], 0f));
            clip.SetCurve("Body", typeof(Transform), "localEulerAnglesRaw.y", AnimationCurve.Constant(0f, times[times.Length - 1], 0f));
            return SaveAsset(clip, AnimFolder + name + ".anim");
        }

        private static T SaveAsset<T>(T asset, string path) where T : Object
        {
            T existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null)
            {
                EditorUtility.CopySerialized(asset, existing);
                EditorUtility.SetDirty(existing);
                return existing;
            }
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        #endregion

        #region Preview pictures

        // Three trays (red, yellow, blue) in a row, rendered on black and on white to get a
        // clean transparent background.
        private static Sprite RenderPreview(Design design)
        {
            const int size = 512;
            Scene scene = EditorSceneManager.NewPreviewScene();
            var objects = new List<GameObject>();
            Texture2D picture = null;
            try
            {
                int[] colours = { 3, 4, 0 };          // blue in front, then yellow, red
                for (int i = 0; i < colours.Length; i++)
                {
                    string path = Root + design.Name + "/Tray " + design.Name + " " + ColourNames[colours[i]] + ".prefab";
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab == null)
                        continue;
                    var tray = (GameObject)Object.Instantiate(prefab);
                    tray.transform.position = new Vector3((i - 1) * 1.25f, 0f, (i - 1) * 2.0f);
                    SceneManager.MoveGameObjectToScene(tray, scene);
                    objects.Add(tray);
                }

                var lightObject = new GameObject("Light");
                Light light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.25f;
                lightObject.transform.rotation = Quaternion.Euler(50f, -35f, 0f);
                SceneManager.MoveGameObjectToScene(lightObject, scene);

                var cameraObject = new GameObject("Camera");
                Camera camera = cameraObject.AddComponent<Camera>();
                camera.scene = scene;
                camera.cameraType = CameraType.Preview;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.fieldOfView = 32f;
                camera.nearClipPlane = 0.1f;
                camera.farClipPlane = 100f;
                camera.allowHDR = false;
                camera.allowMSAA = true;
                cameraObject.transform.position = new Vector3(3.5f, 6.4f, -12.5f);
                cameraObject.transform.LookAt(new Vector3(0f, design.SeatY * 0.5f + 0.2f, 0f));
                SceneManager.MoveGameObjectToScene(cameraObject, scene);

                Texture2D onBlack = Render(camera, size, Color.black);
                Texture2D onWhite = Render(camera, size, Color.white);
                if (onBlack != null && onWhite != null)
                    picture = Matte(onBlack, onWhite);
                // URP may ignore the background colour (both renders black): cut the black out.
                if (picture == null && onBlack != null)
                    picture = KeyOutBlack(onBlack);
                if (picture != null)
                    picture = CropToContent(picture, size, 0.06f);
                Object.DestroyImmediate(onBlack);
                Object.DestroyImmediate(onWhite);
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(scene);
            }

            if (picture == null)
            {
                Debug.LogWarning("[Trays] Could not render the preview of " + design.Name);
                return null;
            }

            string file = PreviewFolder + "Tray_" + design.Id + ".png";
            File.WriteAllBytes(file, picture.EncodeToPNG());
            Object.DestroyImmediate(picture);
            AssetDatabase.ImportAsset(file, ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(file);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(file);
        }

        private static Texture2D Render(Camera camera, int size, Color background)
        {
            var target = RenderTexture.GetTemporary(size, size, 24, RenderTextureFormat.ARGB32);
            target.antiAliasing = 4;
            camera.backgroundColor = background;
            camera.targetTexture = target;
            try
            {
                var request = new RenderPipeline.StandardRequest { destination = target };
                if (RenderPipeline.SupportsRenderRequest(camera, request))
                    RenderPipeline.SubmitRenderRequest(camera, request);
                else
                    camera.Render();

                RenderTexture previous = RenderTexture.active;
                RenderTexture.active = target;
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                texture.ReadPixels(new Rect(0, 0, size, size), 0, 0);
                texture.Apply();
                RenderTexture.active = previous;
                return texture;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[Trays] Preview render failed: " + ex.Message);
                return null;
            }
            finally
            {
                camera.targetTexture = null;
                RenderTexture.ReleaseTemporary(target);
            }
        }

        private static Texture2D KeyOutBlack(Texture2D source)
        {
            Color[] pixels = source.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                Color c = pixels[i];
                float alpha = Mathf.Clamp01(Mathf.Max(c.r, Mathf.Max(c.g, c.b)) / 0.07f);
                pixels[i] = alpha <= 0f ? Color.clear
                    : new Color(Mathf.Clamp01(c.r / alpha), Mathf.Clamp01(c.g / alpha), Mathf.Clamp01(c.b / alpha), alpha);
            }
            var texture = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        /// <summary>Square picture of <paramref name="size"/> with the trays filling it (like the bus pictures).</summary>
        private static Texture2D CropToContent(Texture2D source, int size, float margin)
        {
            Color32[] pixels = source.GetPixels32();
            int w = source.width, h = source.height;
            int minX = w, minY = h, maxX = -1, maxY = -1;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (pixels[y * w + x].a < 8)
                        continue;
                    minX = Mathf.Min(minX, x); maxX = Mathf.Max(maxX, x);
                    minY = Mathf.Min(minY, y); maxY = Mathf.Max(maxY, y);
                }
            }
            if (maxX < 0)
                return source;

            int side = Mathf.Max(maxX - minX + 1, maxY - minY + 1);
            int pad = Mathf.RoundToInt(side * margin);
            side += pad * 2;
            int cx = (minX + maxX) / 2, cy = (minY + maxY) / 2;
            var result = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var output = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = cx - side * 0.5f + (x + 0.5f) * side / size;
                    float v = cy - side * 0.5f + (y + 0.5f) * side / size;
                    output[y * size + x] = u < 0 || v < 0 || u >= w || v >= h ? Color.clear
                        : source.GetPixelBilinear(u / w, v / h);
                }
            }
            result.SetPixels(output);
            result.Apply();
            Object.DestroyImmediate(source);
            return result;
        }

        // alpha = 1 - (white - black); colour = black / alpha
        private static Texture2D Matte(Texture2D onBlack, Texture2D onWhite)
        {
            Color[] black = onBlack.GetPixels(), white = onWhite.GetPixels();
            var result = new Color[black.Length];
            bool any = false;
            for (int i = 0; i < black.Length; i++)
            {
                float diff = ((white[i].r - black[i].r) + (white[i].g - black[i].g) + (white[i].b - black[i].b)) / 3f;
                float alpha = Mathf.Clamp01(1f - diff);
                if (alpha < 0.004f)
                {
                    result[i] = Color.clear;
                    continue;
                }
                any = true;
                result[i] = new Color(Mathf.Clamp01(black[i].r / alpha), Mathf.Clamp01(black[i].g / alpha), Mathf.Clamp01(black[i].b / alpha), alpha);
            }
            if (!any || System.Array.TrueForAll(result, c => c.a >= 0.999f))
                return null;
            var texture = new Texture2D(onBlack.width, onBlack.height, TextureFormat.RGBA32, false);
            texture.SetPixels(result);
            texture.Apply();
            return texture;
        }

        #endregion

        #region Skins database

        private static void RegisterSkins(GameObject[] dataPrefabs, Sprite[] previews)
        {
            var database = AssetDatabase.LoadAssetAtPath<SkinsDatabase>(SkinsDatabasePath);
            if (database == null)
            {
                Debug.LogError("[Trays] Skins Database not found at " + SkinsDatabasePath);
                return;
            }

            var so = new SerializedObject(database);
            SerializedProperty products = so.FindProperty("products");

            // Remove earlier tray entries; they are added again below, before the buses.
            for (int i = products.arraySize - 1; i >= 0; i--)
            {
                if (products.GetArrayElementAtIndex(i).FindPropertyRelative("uniqueId").stringValue.StartsWith(SkinIdPrefix))
                    products.DeleteArrayElementAtIndex(i);
            }

            int insertAt = products.arraySize;
            for (int i = 0; i < products.arraySize; i++)
            {
                if (products.GetArrayElementAtIndex(i).FindPropertyRelative("tabId").intValue == (int)SkinTab.Bus)
                {
                    insertAt = i;
                    break;
                }
            }

            for (int d = 0; d < Designs.Length; d++)
            {
                products.InsertArrayElementAtIndex(insertAt + d);
                SerializedProperty p = products.GetArrayElementAtIndex(insertAt + d);
                Design design = Designs[d];
                p.FindPropertyRelative("uniqueId").stringValue = SkinIdPrefix + design.Id;
                p.FindPropertyRelative("isExpanded").boolValue = false;
                p.FindPropertyRelative("tabId").intValue = (int)SkinTab.Bus;
                p.FindPropertyRelative("isDummy").boolValue = false;
                p.FindPropertyRelative("name").stringValue = design.Name;
                p.FindPropertyRelative("openedSprite").objectReferenceValue = previews[d];
                p.FindPropertyRelative("lockedSprite").objectReferenceValue = previews[d];
                p.FindPropertyRelative("preview2DSprite").objectReferenceValue = previews[d];
                p.FindPropertyRelative("prefab").objectReferenceValue = dataPrefabs[d];
                p.FindPropertyRelative("previewPrefab").objectReferenceValue = null;
                p.FindPropertyRelative("purchaseType").enumValueIndex = (int)SkinData.PurchaseType.InGameCurrency;
                p.FindPropertyRelative("currency").enumValueIndex = (int)design.Currency;
                p.FindPropertyRelative("cost").intValue = design.Cost;
            }

            // Number the tray tab in order and give the buses their own names.
            int id = 0;
            for (int i = 0; i < products.arraySize; i++)
            {
                SerializedProperty p = products.GetArrayElementAtIndex(i);
                if (p.FindPropertyRelative("tabId").intValue != (int)SkinTab.Bus)
                    continue;
                p.FindPropertyRelative("id").intValue = id++;
                SerializedProperty name = p.FindPropertyRelative("name");
                if (name.stringValue == "Bus 1" || name.stringValue == "City Bus")
                    name.stringValue = "Food Bus";
                else if (name.stringValue == "Bus 2" || name.stringValue == "Camper Van")
                    name.stringValue = "Snack Van";
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(database);
        }

        #endregion

        #region Shapes

        private static List<Vector2> RoundedRect(float width, float depth, float radius, int segments, Vector2 centre = default)
        {
            radius = Mathf.Clamp(radius, 0.01f, Mathf.Min(width, depth) * 0.5f);
            float hx = width * 0.5f - radius, hz = depth * 0.5f - radius;
            Vector2[] corners = { new Vector2(hx, hz), new Vector2(-hx, hz), new Vector2(-hx, -hz), new Vector2(hx, -hz) };
            var points = new List<Vector2>();
            for (int c = 0; c < 4; c++)
            {
                for (int s = 0; s <= segments; s++)
                {
                    float angle = (c * 90f + 90f * s / segments) * Mathf.Deg2Rad;
                    points.Add(centre + corners[c] + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
                }
            }
            return points;
        }

        private static List<Vector2> Ellipse(float a, float b, int count)
        {
            var points = new List<Vector2>();
            for (int i = 0; i < count; i++)
            {
                float angle = i * Mathf.PI * 2f / count;
                points.Add(new Vector2(Mathf.Cos(angle) * a, Mathf.Sin(angle) * b));
            }
            return points;
        }

        /// <summary>Simple mesh assembly: flat-shaded caps, smooth walls, one submesh per material slot.</summary>
        private sealed class MeshMaker
        {
            private readonly List<Vector3> vertices = new List<Vector3>();
            private readonly List<Vector3> normals = new List<Vector3>();
            private readonly List<Vector2> uvs = new List<Vector2>();
            private readonly List<int>[] triangles = { new List<int>(), new List<int>(), new List<int>(), new List<int>() };

            private int Vertex(Vector3 position, Vector3 normal)
            {
                vertices.Add(position);
                normals.Add(normal);
                uvs.Add(new Vector2(position.x * 0.25f, position.z * 0.25f + position.y * 0.25f));
                return vertices.Count - 1;
            }

            // Triangle facing along `facing`.
            private void Triangle(int a, int b, int c, Vector3 facing, int slot)
            {
                Vector3 cross = Vector3.Cross(vertices[b] - vertices[a], vertices[c] - vertices[a]);
                if (Vector3.Dot(cross, facing) < 0f)
                    (b, c) = (c, b);
                triangles[slot].Add(a);
                triangles[slot].Add(b);
                triangles[slot].Add(c);
            }

            private static Vector2 Centre(List<Vector2> loop)
            {
                Vector2 sum = Vector2.zero;
                foreach (Vector2 p in loop)
                    sum += p;
                return sum / loop.Count;
            }

            /// <summary>Side wall of a loop from y0 to y1, facing out (or in).</summary>
            public void Wall(List<Vector2> loop, float y0, float y1, bool outward, int slot)
            {
                Vector2 centre = Centre(loop);
                int n = loop.Count, start = vertices.Count;
                for (int i = 0; i < n; i++)
                {
                    Vector2 tangent = loop[(i + 1) % n] - loop[(i - 1 + n) % n];
                    var normal2 = new Vector2(tangent.y, -tangent.x).normalized;
                    if (Vector2.Dot(normal2, loop[i] - centre) < 0f)
                        normal2 = -normal2;
                    if (!outward)
                        normal2 = -normal2;
                    var normal = new Vector3(normal2.x, 0f, normal2.y);
                    Vertex(new Vector3(loop[i].x, y0, loop[i].y), normal);
                    Vertex(new Vector3(loop[i].x, y1, loop[i].y), normal);
                }
                for (int i = 0; i < n; i++)
                {
                    int a = start + i * 2, b = start + ((i + 1) % n) * 2;
                    Vector3 facing = normals[a] + normals[b];
                    Triangle(a, a + 1, b + 1, facing, slot);
                    Triangle(a, b + 1, b, facing, slot);
                }
            }

            /// <summary>Flat lid over a convex loop.</summary>
            public void Cap(List<Vector2> loop, float y, bool up, int slot)
            {
                Vector3 normal = up ? Vector3.up : Vector3.down;
                Vector2 centre = Centre(loop);
                int middle = Vertex(new Vector3(centre.x, y, centre.y), normal);
                int start = vertices.Count;
                foreach (Vector2 p in loop)
                    Vertex(new Vector3(p.x, y, p.y), normal);
                for (int i = 0; i < loop.Count; i++)
                    Triangle(middle, start + i, start + (i + 1) % loop.Count, normal, slot);
            }

            /// <summary>Flat band between two loops with the same number of points.</summary>
            public void Ring(List<Vector2> outer, List<Vector2> inner, float y, int slot, bool down = false)
            {
                Vector3 normal = down ? Vector3.down : Vector3.up;
                int n = outer.Count, start = vertices.Count;
                for (int i = 0; i < n; i++)
                {
                    Vertex(new Vector3(outer[i].x, y, outer[i].y), normal);
                    Vertex(new Vector3(inner[i].x, y, inner[i].y), normal);
                }
                for (int i = 0; i < n; i++)
                {
                    int a = start + i * 2, b = start + ((i + 1) % n) * 2;
                    Triangle(a, a + 1, b + 1, normal, slot);
                    Triangle(a, b + 1, b, normal, slot);
                }
            }

            public void Box(Vector3 centre, Vector3 size, int slot)
            {
                Vector3 h = size * 0.5f;
                Vector3[] axes = { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back };
                foreach (Vector3 n in axes)
                {
                    Vector3 u = Mathf.Abs(n.y) > 0.5f ? Vector3.right : Vector3.up;
                    Vector3 v = Vector3.Cross(n, u);
                    Vector3 c = centre + Vector3.Scale(n, h);
                    Vector3 du = Vector3.Scale(u, h), dv = Vector3.Scale(v, h);
                    int a = Vertex(c - du - dv, n), b = Vertex(c + du - dv, n), d = Vertex(c + du + dv, n), e = Vertex(c - du + dv, n);
                    Triangle(a, b, d, n, slot);
                    Triangle(a, d, e, n, slot);
                }
            }

            /// <summary>Capped cylinder starting at `start`, `length` long along `axis`.</summary>
            public void Cylinder(Vector3 start, float radius, float length, Vector3 axis, int segments, int slot)
            {
                axis = axis.normalized;
                Vector3 u = Vector3.Cross(axis, Mathf.Abs(axis.y) > 0.9f ? Vector3.right : Vector3.up).normalized;
                Vector3 v = Vector3.Cross(axis, u);
                Vector3 end = start + axis * length;
                int side = vertices.Count;
                for (int i = 0; i < segments; i++)
                {
                    float angle = i * Mathf.PI * 2f / segments;
                    Vector3 n = u * Mathf.Cos(angle) + v * Mathf.Sin(angle);
                    Vertex(start + n * radius, n);
                    Vertex(end + n * radius, n);
                }
                for (int i = 0; i < segments; i++)
                {
                    int a = side + i * 2, b = side + ((i + 1) % segments) * 2;
                    Vector3 facing = normals[a] + normals[b];
                    Triangle(a, a + 1, b + 1, facing, slot);
                    Triangle(a, b + 1, b, facing, slot);
                }
                foreach ((Vector3 centre, Vector3 n) in new[] { (start, -axis), (end, axis) })
                {
                    int middle = Vertex(centre, n);
                    int ring = vertices.Count;
                    for (int i = 0; i < segments; i++)
                    {
                        float angle = i * Mathf.PI * 2f / segments;
                        Vertex(centre + (u * Mathf.Cos(angle) + v * Mathf.Sin(angle)) * radius, n);
                    }
                    for (int i = 0; i < segments; i++)
                        Triangle(middle, ring + i, ring + (i + 1) % segments, n, slot);
                }
            }

            public Mesh ToMesh(string name)
            {
                var mesh = new Mesh { name = name };
                if (vertices.Count > 65000)
                    mesh.indexFormat = IndexFormat.UInt32;
                mesh.SetVertices(vertices);
                mesh.SetNormals(normals);
                mesh.SetUVs(0, uvs);
                mesh.subMeshCount = triangles.Length;
                for (int i = 0; i < triangles.Length; i++)
                    mesh.SetTriangles(triangles[i], i);
                mesh.RecalculateBounds();
                mesh.RecalculateTangents();
                return mesh;
            }
        }

        #endregion
    }
}
