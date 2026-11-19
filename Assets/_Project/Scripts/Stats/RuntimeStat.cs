using System.Collections.Generic;
using Project.Shared;

namespace Project.Stats
{
    public class RuntimeStat
    {
        public readonly StatType Type;
        public float BaseValue;

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
