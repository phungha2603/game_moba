using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Project.UI
{
    public class AttackButton : MonoBehaviour, IPointerDownHandler
    {
        public event Action OnPressed;

        public void OnPointerDown(PointerEventData eventData) => OnPressed?.Invoke();
    }
}
