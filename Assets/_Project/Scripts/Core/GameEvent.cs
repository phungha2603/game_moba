using System.Collections.Generic;
using UnityEngine;

namespace Project.Core
{
    [CreateAssetMenu(menuName = "Project/Core/Game Event", fileName = "NewGameEvent")]
    public class GameEvent : ScriptableObject
    {
        private readonly List<IGameEventListener> _listeners = new List<IGameEventListener>();

        public void Raise()
        {
            for (int i = _listeners.Count - 1; i >= 0; i--)
            {
                _listeners[i].OnEventRaised();
            }
        }

        public void RegisterListener(IGameEventListener listener)
        {
            if (!_listeners.Contains(listener)) _listeners.Add(listener);
        }

        public void UnregisterListener(IGameEventListener listener)
        {
            _listeners.Remove(listener);
        }
    }
}
