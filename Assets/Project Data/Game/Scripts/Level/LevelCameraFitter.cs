using UnityEngine;

namespace Watermelon.BusStop
{
    /// <summary>
    /// Fits the game camera to each level so the whole board is visible between the waiting
    /// slots (dock) and the power-up toolbar.
    ///
    /// The camera in Game.unity is authored for average boards; tall boards (9-10 rows) ran
    /// behind the toolbar and wide ones (9 columns) past the screen edges. After a level loads
    /// the camera starts from its authored pose; if the board already fits nothing changes.
    /// Otherwise it backs away along its view direction just enough, sliding so the dock stays
    /// at exactly the same height on screen (the conveyor, trays and backdrop stay put; the
    /// board simply gets a little smaller).
    /// </summary>
    public static class LevelCameraFitter
    {
        private const float BoardMargin = 0.35f;        // world units around the board (tile rim)
        private const float ToolbarGap = 0.012f;        // viewport space kept above the toolbar
        private const float SideGap = 0.02f;            // viewport space kept at the screen edges
        private const float FallbackToolbarTop = 0.17f; // when the toolbar cannot be found
        private const float MaxPullBack = 30f;

        private static Camera fittedCamera;
        private static Vector3 authoredPosition;
        private static Quaternion authoredRotation;

        public static void Fit(int levelWidth, int levelHeight)
        {
            Camera cam = Camera.main;
            if (cam == null || cam.orthographic || LevelController.Environment == null || LevelController.Dock == null)
                return;

            if (fittedCamera != cam)
            {
                fittedCamera = cam;
                authoredPosition = cam.transform.position;
                authoredRotation = cam.transform.rotation;
            }

            Transform t = cam.transform;
            t.SetPositionAndRotation(authoredPosition, authoredRotation);

            Vector3 dock = LevelController.Dock.transform.position;
            float dockViewportY = cam.WorldToViewportPoint(dock).y;
            float bottomLimit = GetToolbarTop(cam) + ToolbarGap;

            Vector3[] board = BoardCorners(levelWidth, levelHeight);
            if (Fits(cam, board, bottomLimit))
                return;

            // smallest pull-back that fits (fitting only gets easier the further back we go)
            float lo = 0f, hi = MaxPullBack;
            if (!TryPose(cam, hi, dock, dockViewportY, board, bottomLimit))
                return;   // even fully pulled back it does not fit; keep that pose

            for (int i = 0; i < 24; i++)
            {
                float mid = (lo + hi) * 0.5f;
                if (TryPose(cam, mid, dock, dockViewportY, board, bottomLimit))
                    hi = mid;
                else
                    lo = mid;
            }

            TryPose(cam, hi, dock, dockViewportY, board, bottomLimit);
        }

        // Pulls the camera back by `distance` along its view direction, then slides it along
        // the world Z axis until the dock is back at its authored screen height.
        private static bool TryPose(Camera cam, float distance, Vector3 dock, float dockViewportY,
                                    Vector3[] board, float bottomLimit)
        {
            Transform t = cam.transform;
            Vector3 pulled = authoredPosition - (authoredRotation * Vector3.forward) * distance;

            float lo = -60f, hi = 60f;
            for (int i = 0; i < 30; i++)
            {
                float mid = (lo + hi) * 0.5f;
                t.position = pulled + Vector3.forward * mid;
                // moving the camera forward (+Z) moves the dock down the screen
                if (cam.WorldToViewportPoint(dock).y > dockViewportY)
                    lo = mid;
                else
                    hi = mid;
            }
            t.position = pulled + Vector3.forward * ((lo + hi) * 0.5f);

            return Fits(cam, board, bottomLimit);
        }

        private static bool Fits(Camera cam, Vector3[] board, float bottomLimit)
        {
            for (int i = 0; i < board.Length; i++)
            {
                Vector3 v = cam.WorldToViewportPoint(board[i]);
                if (v.z <= 0f || v.x < SideGap || v.x > 1f - SideGap || v.y < bottomLimit)
                    return false;
            }
            return true;
        }

        private static Vector3[] BoardCorners(int width, int height)
        {
            float size = LevelController.ElementSize;
            float halfX = width * size * 0.5f + BoardMargin;
            float top = LevelController.StartZ + BoardMargin;
            float bottom = LevelController.StartZ - height * size - BoardMargin;
            float y = LevelController.PosY;

            return new[]
            {
                new Vector3(-halfX, y, top), new Vector3(halfX, y, top),
                new Vector3(-halfX, y, bottom), new Vector3(halfX, y, bottom),
            };
        }

        // Viewport height of the top edge of the power-up toolbar.
        private static float GetToolbarTop(Camera cam)
        {
            UIGame game = UIController.GetPage<UIGame>();
            RectTransform toolbar = game != null ? FindDeep(game.transform, "Bottom Toolbar") as RectTransform : null;
            if (toolbar == null || !toolbar.gameObject.activeInHierarchy)
                return FallbackToolbarTop;

            Canvas.ForceUpdateCanvases();
            Canvas canvas = toolbar.GetComponentInParent<Canvas>();
            Camera uiCam = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? (canvas.worldCamera != null ? canvas.worldCamera : cam)
                : null;

            // the highest thing actually drawn in the toolbar (buttons can stick out of its box)
            Vector3[] corners = new Vector3[4];
            float top = 0f;
            foreach (UnityEngine.UI.Graphic g in toolbar.GetComponentsInChildren<UnityEngine.UI.Graphic>(false))
            {
                if (!g.enabled || g.color.a < 0.05f)
                    continue;
                g.rectTransform.GetWorldCorners(corners);
                for (int i = 0; i < 4; i++)
                {
                    Vector2 screen = RectTransformUtility.WorldToScreenPoint(uiCam, corners[i]);
                    top = Mathf.Max(top, screen.y / Mathf.Max(1, Screen.height));
                }
            }

            return top > 0.02f && top < 0.6f ? top : FallbackToolbarTop;
        }

        private static Transform FindDeep(Transform root, string name)
        {
            if (root.name == name)
                return root;
            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindDeep(root.GetChild(i), name);
                if (found != null)
                    return found;
            }
            return null;
        }
    }
}
