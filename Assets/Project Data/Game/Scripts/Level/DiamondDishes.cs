using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.BusStop;

namespace Watermelon
{
    /// <summary>
    /// Diamond dishes: from level 3 on, a few dishes on the board carry a small diamond badge
    /// (1 per level, 2 on hard levels, 3 on boss levels). When such a dish is seated on its
    /// tray, a diamond flies to the diamond counter and the player gets +1. Only on a level's
    /// first win: replaying a completed level has no diamond dishes, so they can't be farmed.
    /// LevelController calls <see cref="OnLevelStarted"/> / <see cref="Clear"/>; BusBehavior
    /// calls <see cref="OnSeated"/>.
    /// </summary>
    public static class DiamondDishes
    {
        private const int FirstLevel = 2;                 // after the two tutorial levels

        public static void OnLevelStarted(int levelIndex)
        {
            Clear();
            if (levelIndex < FirstLevel || LevelController.IsLevelCompleted(levelIndex))
                return;

            int k = levelIndex % WorldCatalog.LevelsPerCountry;
            int count = k % 10 == 9 ? 3 : k % 5 == 4 ? 2 : 1;

            var candidates = new List<BaseCharacterBehavior>();
            IReadOnlyList<LevelElementBehavior> elements = LevelController.Elements;
            if (elements == null)
                return;
            foreach (LevelElementBehavior e in elements)
            {
                if (e is BaseCharacterBehavior dish && dish.gameObject.activeInHierarchy &&
                    dish.GetComponent<CountryFoodVisual>() != null && !IsHidden(dish))
                    candidates.Add(dish);
            }

            // the same dishes every time this level is played (seeded by the level)
            var rng = new System.Random(levelIndex * 7919 + 17);
            for (int i = 0; i < count && candidates.Count > 0; i++)
            {
                int pick = rng.Next(candidates.Count);
                candidates[pick].gameObject.AddComponent<DiamondDishMark>();
                candidates.RemoveAt(pick);
            }
        }

        /// <summary>Removes every badge (dishes are pooled and reused by the next level).</summary>
        public static void Clear()
        {
            foreach (DiamondDishMark mark in Object.FindObjectsByType<DiamondDishMark>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                mark.Remove();
        }

        /// <summary>A dish was seated on its tray: a diamond dish pays out.</summary>
        public static void OnSeated(BaseCharacterBehavior dish)
        {
            DiamondDishMark mark = dish != null ? dish.GetComponent<DiamondDishMark>() : null;
            if (mark == null)
                return;
            Vector3 from = mark.BadgePosition;
            mark.Remove();
            DiamondFlight.Play(from);
        }

        private static bool IsHidden(BaseCharacterBehavior dish)
        {
            foreach (ElementSpecialEffect effect in dish.GetComponentsInChildren<ElementSpecialEffect>(true))
                if (effect.EffectType == LevelData.SpecialEffectType.Hidden)
                    return true;
            return false;
        }
    }

    /// <summary>The badge on a diamond dish: the game's diamond icon above the food, bobbing.</summary>
    public sealed class DiamondDishMark : MonoBehaviour
    {
        private static readonly Vector3 BadgeOffset = new Vector3(0.34f, 0.5f, -0.02f);
        private const float BadgeSize = 0.46f;

        private SpriteRenderer badge;
        private float time;

        public Vector3 BadgePosition => badge != null ? badge.transform.position : transform.position;

        private void LateUpdate()
        {
            if (badge == null)
            {
                CountryFoodVisual visual = GetComponent<CountryFoodVisual>();
                Sprite icon = CurrenciesController.GetCurrency(CurrencyType.Diamonds)?.Icon;
                if (visual == null || visual.Card == null || icon == null)
                    return;
                badge = visual.AddBadge("Diamond Badge", icon, BadgeOffset, BadgeSize);
                if (badge == null)
                    return;
            }

            time += Time.deltaTime;
            float bob = Mathf.Sin(time * 3.2f);
            badge.transform.localPosition = BadgeOffset + new Vector3(0f, bob * 0.05f, 0f);
            float pulse = 1f + 0.08f * Mathf.Max(0f, Mathf.Sin(time * 6.4f));
            badge.transform.localScale = Vector3.one * BadgeSize * pulse;
        }

        public void Remove()
        {
            if (badge != null)
                Destroy(badge.gameObject);
            Destroy(this);
        }
    }

    /// <summary>A diamond flying from the board to the diamond counter, +1 when it lands.</summary>
    public sealed class DiamondFlight : MonoBehaviour
    {
        public static void Play(Vector3 worldFrom)
        {
            CurrencyUIPanelSimple counter = CoinShower.VisibleCounter(CurrencyType.Diamonds, null);
            Camera cam = Camera.main;
            if (counter == null || cam == null)
            {
                Grant();
                return;
            }

            Canvas canvas = counter.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Grant();
                return;
            }
            canvas = canvas.rootCanvas;
            var go = new GameObject("Diamond Flight", typeof(RectTransform));
            go.transform.SetParent(canvas.transform, false);
            go.transform.SetAsLastSibling();
            var flight = go.AddComponent<DiamondFlight>();
            flight.StartCoroutine(flight.Fly(canvas, counter, cam.WorldToScreenPoint(worldFrom)));
        }

        private static void Grant()
        {
            try
            {
                CurrenciesController.Add(CurrencyType.Diamonds, 1);
                SaveController.MarkAsSaveIsRequired();
            }
            catch (System.Exception ex) { Debug.LogWarning("[DiamondDishes] " + ex.Message); }
        }

        private IEnumerator Fly(Canvas canvas, CurrencyUIPanelSimple counter, Vector2 screenFrom)
        {
            var layer = (RectTransform)canvas.transform;
            Camera uiCam = canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            Image image = gameObject.AddComponent<Image>();
            image.sprite = CurrenciesController.GetCurrency(CurrencyType.Diamonds).Icon;
            image.preserveAspect = true;
            image.raycastTarget = false;
            var r = (RectTransform)transform;
            r.sizeDelta = new Vector2(96f, 96f);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(layer, screenFrom, uiCam, out Vector2 from);
            r.anchoredPosition = from;
            try { AudioController.PlaySound(AudioController.Sounds.clickSound, 0.8f, 1.35f); } catch { }

            // a little pop where the dish was, then an arc to the counter
            for (float t = 0f; t < 0.18f; t += Time.unscaledDeltaTime)
            {
                r.localScale = Vector3.one * Mathf.Lerp(0.4f, 1.25f, t / 0.18f);
                yield return null;
            }
            Transform icon = counter.transform;
            for (float t = 0f; t < 0.6f; t += Time.unscaledDeltaTime)
            {
                float k = t / 0.6f;
                float e = k * k * (3f - 2f * k);
                Vector2 screenTo = RectTransformUtility.WorldToScreenPoint(uiCam, icon.position);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(layer, screenTo, uiCam, out Vector2 to);
                Vector2 control = (from + to) * 0.5f + new Vector2(-160f, 200f);
                float u = 1f - e;
                r.anchoredPosition = u * u * from + 2f * u * e * control + e * e * to;
                r.localScale = Vector3.one * Mathf.Lerp(1.25f, 0.6f, e);
                r.localEulerAngles = new Vector3(0f, 0f, 360f * e);
                yield return null;
            }

            Grant();
            try { AudioController.PlaySound(AudioController.Sounds.clickSound, 0.8f, 1.6f); } catch { }
            for (float t = 0f; t < 0.18f; t += Time.unscaledDeltaTime)
            {
                icon.localScale = Vector3.one * (1f + 0.2f * Mathf.Sin(t / 0.18f * Mathf.PI));
                yield return null;
            }
            icon.localScale = Vector3.one;
            Destroy(gameObject);
        }
    }
}
