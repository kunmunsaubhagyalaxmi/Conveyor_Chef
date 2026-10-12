using UnityEngine;
using UnityEngine.EventSystems;

namespace Watermelon.BusStop
{
    /// <summary>
    /// Swipe left / right over the Level Selection cards to turn the page (three levels per page).
    /// Sits on the cards' parent; drags that start on a card bubble up to it, and a drag never
    /// counts as a tap on the card.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LevelSelectionSwipe : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Tooltip("How far (as a share of the screen width) a swipe must travel to turn the page.")]
        [SerializeField, Range(0.02f, 0.4f)] private float threshold = 0.08f;

        private LevelSelectionController owner;
        private Vector2 start;

        public void Bind(LevelSelectionController controller) => owner = controller;

        public void OnBeginDrag(PointerEventData eventData) => start = eventData.position;

        public void OnDrag(PointerEventData eventData) { }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (owner == null)
                return;

            Vector2 delta = eventData.position - start;
            if (Mathf.Abs(delta.x) < Screen.width * threshold || Mathf.Abs(delta.x) < Mathf.Abs(delta.y))
                return;

            // finger moving left shows the next levels
            owner.TurnPage(delta.x < 0f ? 1 : -1);
        }
    }
}
