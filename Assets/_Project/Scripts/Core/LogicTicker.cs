using System;
using UnityEngine;

namespace Project.Core
{
    /// Runs before other scripts so CharacterBrain.OnEnable can subscribe safely in the same frame.
    [DefaultExecutionOrder(-1000)]
    public class LogicTicker : MonoBehaviour
    {
        public const float TickRate = 30f;
        public const float FixedDeltaTime = 1f / TickRate;

        public static LogicTicker Instance { get; private set; }

        public event Action<float> OnTick;

        private float _accumulator;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Update()
        {
            _accumulator += Time.deltaTime;

            while (_accumulator >= FixedDeltaTime)
            {
                OnTick?.Invoke(FixedDeltaTime);
                _accumulator -= FixedDeltaTime;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
