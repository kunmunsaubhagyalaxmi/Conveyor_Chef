using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    /// <summary>
    /// The chef as the game's mascot:
    /// - <see cref="PlayWinJump"/>: on the win screen the chef jumps up from the bottom with
    ///   arms raised, lands with a little squash, bobs happily and drops back out of view.
    /// - <see cref="AttachLoading"/>: on the loading screen the chef pops in and sways/waves.
    /// Poses (optional, transparent PNG, portrait): Resources/Chef/Poses/chef_jump.png,
    /// chef_cheer.png (shown after landing) and chef_wave.png. Without them the standing chef
    /// is used. Poses are drawn for the Classic outfit; another outfit can have its own as
    /// &lt;outfit id&gt;_jump.png etc., otherwise that outfit's full picture is used.
    /// </summary>
    public sealed class ChefMascot : MonoBehaviour
    {
        private const string PoseFolder = "Chef/Poses/";

        private Image image;
        private RectTransform rect;

        /// <summary>The chosen outfit in <paramref name="pose"/> ("jump", "cheer", "wave"), or null.</summary>
        public static Sprite Pose(string pose)
        {
            ChefProgress.Outfit outfit = ChefProgress.Selected;
            Sprite sprite = Resources.Load<Sprite>(PoseFolder + outfit.Id + "_" + pose);
            if (sprite == null && outfit.Id == "classic")
                sprite = Resources.Load<Sprite>(PoseFolder + "chef_" + pose);
            return sprite;
        }

        private static Sprite Standing() => ChefProgress.FullBody(Resources.Load<MenuPanelArt>(MenuPanelArt.ResourcePath));

        public static void PlayWinJump(Transform page, float height = 820f)
        {
            ChefMascot mascot = Create(page, "Chef Celebration", Pose("jump") ?? Standing(), height);
            if (mascot != null)
                mascot.Run(mascot.WinJump(Pose("cheer")));
        }

        /// <param name="feetAnchor">Where the chef's feet stand, as a fraction of the parent (0..1).</param>
        /// <param name="heightFraction">Chef height as a fraction of the parent's height.</param>
        public static void AttachLoading(RectTransform parent, Vector2 feetAnchor, float heightFraction)
        {
            ChefMascot mascot = Create(parent, "Loading Chef", Pose("wave") ?? Standing(), 600f);
            if (mascot == null)
                return;
            mascot.rect.anchorMin = mascot.rect.anchorMax = feetAnchor;
            mascot.rect.pivot = new Vector2(0.5f, 0f);
            mascot.rect.anchoredPosition = Vector2.zero;
            mascot.Run(mascot.Loading(Pose("wave") != null, parent, heightFraction));
        }

        private static ChefMascot Create(Transform parent, string name, Sprite sprite, float height)
        {
            if (parent == null || sprite == null)
                return null;
            Transform old = parent.Find(name);
            if (old != null)
                Destroy(old.gameObject);

            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.transform.SetAsLastSibling();
            var mascot = go.AddComponent<ChefMascot>();
            mascot.rect = (RectTransform)go.transform;
            mascot.image = go.AddComponent<Image>();
            mascot.image.sprite = sprite;
            mascot.image.preserveAspect = true;
            mascot.image.raycastTarget = false;
            mascot.SetSize(sprite, height);
            mascot.rect.anchorMin = mascot.rect.anchorMax = new Vector2(0.5f, 0.5f);
            return mascot;
        }

        private void SetSize(Sprite sprite, float height)
        {
            float aspect = sprite.rect.height > 0f ? sprite.rect.width / sprite.rect.height : 0.5f;
            rect.sizeDelta = new Vector2(height * aspect, height);
        }

        // The page may still be hidden when this is called: start as soon as it shows.
        private IEnumerator pending;

        private void Run(IEnumerator routine)
        {
            pending = routine;
            if (isActiveAndEnabled)
                OnEnable();
        }

        private void OnEnable()
        {
            if (pending == null)
                return;
            IEnumerator routine = pending;
            pending = null;
            StartCoroutine(routine);
        }

        private IEnumerator WinJump(Sprite cheer)
        {
            const float start = -2000f, peak = -40f, land = -150f, exit = -2200f;
            rect.anchoredPosition = new Vector2(0f, start);
            yield return null;

            // up from below, stretched
            yield return Tween(0.38f, k =>
            {
                float e = EaseOutCubic(k);
                rect.anchoredPosition = new Vector2(0f, Mathf.Lerp(start, peak, e));
                rect.localScale = new Vector3(Mathf.Lerp(0.9f, 1f, e), Mathf.Lerp(1.14f, 1f, e), 1f);
                rect.localEulerAngles = new Vector3(0f, 0f, Mathf.Lerp(-8f, 2f, e));
            });

            // land with a squash
            yield return Tween(0.18f, k =>
            {
                rect.anchoredPosition = new Vector2(0f, Mathf.Lerp(peak, land, EaseInOut(k)));
                float squash = Mathf.Sin(k * Mathf.PI) * 0.08f;
                rect.localScale = new Vector3(1f + squash * 0.7f, 1f - squash, 1f);
                rect.localEulerAngles = new Vector3(0f, 0f, Mathf.Lerp(2f, 0f, k));
            });
            if (cheer != null)
            {
                image.sprite = cheer;
                SetSize(cheer, rect.sizeDelta.y);
            }

            // happy bob
            yield return Tween(0.75f, k =>
            {
                float wave = Mathf.Sin(k * Mathf.PI * 3f);
                rect.anchoredPosition = new Vector2(0f, land + Mathf.Abs(wave) * 18f);
                rect.localEulerAngles = new Vector3(0f, 0f, wave * 3f);
                rect.localScale = Vector3.one;
            });

            // drop out of view
            yield return Tween(0.36f, k =>
            {
                float e = k * k;
                rect.anchoredPosition = new Vector2(0f, Mathf.Lerp(land, exit, e));
                rect.localEulerAngles = new Vector3(0f, 0f, Mathf.Lerp(0f, 10f, e));
            });
            Destroy(gameObject);
        }

        private IEnumerator Loading(bool waving, RectTransform parent, float heightFraction)
        {
            rect.localScale = Vector3.zero;
            yield return null;                   // the parent has its size after the first layout
            if (parent != null && parent.rect.height > 1f)
                SetSize(image.sprite, parent.rect.height * heightFraction);

            // pop in
            yield return Tween(0.35f, k => rect.localScale = Vector3.one * EaseOutBack(k));
            float time = 0f;
            while (true)
            {
                time += Time.unscaledDeltaTime;
                float bob = Mathf.Sin(time * 4.2f);
                rect.anchoredPosition = new Vector2(0f, Mathf.Abs(bob) * 14f);
                // a waving pose rocks a little; the standing chef sways side to side
                rect.localEulerAngles = new Vector3(0f, 0f, Mathf.Sin(time * 2.1f) * (waving ? 3f : 4.5f));
                rect.localScale = new Vector3(1f + bob * 0.012f, 1f - bob * 0.012f, 1f);
                yield return null;
            }
        }

        private static IEnumerator Tween(float duration, System.Action<float> step)
        {
            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                step(t / duration);
                yield return null;
            }
            step(1f);
        }

        private static float EaseOutCubic(float k) { k = 1f - Mathf.Clamp01(k); return 1f - k * k * k; }
        private static float EaseInOut(float k) { k = Mathf.Clamp01(k); return k * k * (3f - 2f * k); }
        private static float EaseOutBack(float k) { const float c = 1.7f; float x = Mathf.Clamp01(k) - 1f; return 1f + (c + 1f) * x * x * x + c * x * x; }
    }
}
