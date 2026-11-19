using System.Collections.Generic;
using Project.Shared;

namespace Project.Stats
{
    public class RuntimeStat
    {
        public readonly StatType Type;
        public float BaseValue { get; private set; }

        private readonly List<StatModifier> _modifiers = new List<StatModifier>();
        private float _cachedValue;
        private bool _isDirty = true;

        public RuntimeStat(StatType type, float baseValue)
        {
            Type = type;
            BaseValue = baseValue;
        }

        public float CurrentValue
        {
            get
            {
                if (_isDirty) Recalculate();
                return _cachedValue;
            }
        }

        /// Only way to change BaseValue after construction (e.g. leveling up) — a direct
        /// field write would leave a stale CurrentValue cached if it had already been read.
        public void SetBaseValue(float value)
        {
            if (value == BaseValue) return;
            BaseValue = value;
            _isDirty = true;
        }

        public void AddModifier(StatModifier modifier)
        {
            _modifiers.Add(modifier);
            _isDirty = true;
        }

        public void RemoveModifiersFromSource(object source)
        {
            _modifiers.RemoveAll(m => ReferenceEquals(m.Source, source));
            _isDirty = true;
        }

        private void Recalculate()
        {
            float flatSum = 0f;
            float percentSum = 0f;

            foreach (StatModifier modifier in _modifiers)
            {
                if (modifier.Type == ModifierType.Flat) flatSum += modifier.Value;
                else percentSum += modifier.Value;
            }

            _cachedValue = (BaseValue + flatSum) * (1f + percentSum);
            _isDirty = false;
        }
    }
}
