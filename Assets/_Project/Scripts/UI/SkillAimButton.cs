using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Project.UI
{
    /// Phase 2 scope: outputs an aim direction/trigger only. Phase 3 (Modular Skill System)
    /// is the first thing that will actually listen to OnSkillTriggered and cast something.
    public class SkillAimButton : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform _origin;

        public event Action<Vector2> OnAimDirectionChanged;
        public event Action<Vector2> OnSkillTriggered;

        private Vector2 _currentDirection;

        /// Code-driven setup path (mirrors CharacterStats.Initialize); see VirtualJoystick.Configure.
        public void Configure(RectTransform origin) => _origin = origin;

        public void OnPointerDown(PointerEventData eventData)
        {
            _currentDirection = Vector2.zero;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _origin, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            {
                return;
            }

            _currentDirection = localPoint.normalized;
            OnAimDirectionChanged?.Invoke(_currentDirection);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnSkillTriggered?.Invoke(_currentDirection);
        }
    }
}
