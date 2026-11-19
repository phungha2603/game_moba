using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Project.UI
{
    /// Review-only (no Unity Editor/Physics-free unit test covers this): drag math over
    /// RectTransforms needs the real UI/EventSystem, which the .NET shim does not provide.
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform _background;
        [SerializeField] private RectTransform _handle;
        [SerializeField] private float _handleRange = 100f;

        public event Action<Vector2> OnDirectionChanged;

        public Vector2 Value { get; private set; }

        /// Code-driven setup path (mirrors CharacterStats.Initialize), used when no Editor
        /// is available to wire the SerializeFields by hand in the Inspector.
        public void Configure(RectTransform background, RectTransform handle, float handleRange)
        {
            _background = background;
            _handle = handle;
            _handleRange = handleRange;
        }

        public void OnPointerDown(PointerEventData eventData) => OnDrag(eventData);

        public void OnDrag(PointerEventData eventData)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _background, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            {
                return;
            }

            Vector2 clamped = Vector2.ClampMagnitude(localPoint, _handleRange);
            _handle.anchoredPosition = clamped;
            Value = clamped / _handleRange;
            OnDirectionChanged?.Invoke(Value);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _handle.anchoredPosition = Vector2.zero;
            Value = Vector2.zero;
            OnDirectionChanged?.Invoke(Value);
        }
    }
}
