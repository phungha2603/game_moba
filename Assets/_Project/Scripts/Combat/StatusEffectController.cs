using Project.Shared;
using Project.Stats;
using UnityEngine;

namespace Project.Combat
{
    /// Tracks Stun/Slow durations and applies Tenacity. Deliberately knows nothing about
    /// CharacterBrain/StateMachine (Project.Character depends on Project.Combat, never the
    /// reverse) — CharacterBrain polls IsStunned itself and decides what to do about it.
    [RequireComponent(typeof(CharacterStats))]
    public class StatusEffectController : MonoBehaviour
    {
        private const float TenacityCap = 0.8f;

        private CharacterStats _stats;
        private float _stunRemaining;
        private float _slowRemaining;
        private float _slowPercent;

        public bool IsStunned => _stunRemaining > 0f;
        public bool IsSlowed => _slowRemaining > 0f;

        private void Awake()
        {
            _stats = GetComponent<CharacterStats>();
        }

        public void ApplyStun(float duration)
        {
            float finalDuration = ApplyTenacity(duration);
            if (finalDuration > _stunRemaining) _stunRemaining = finalDuration;
        }

        public void ApplySlow(float percent, float duration)
        {
            float finalDuration = ApplyTenacity(duration);

            // Refresh semantics: keep the stronger slow and the longer remaining duration,
            // never stack multiple Slow modifiers from this controller.
            if (finalDuration > _slowRemaining) _slowRemaining = finalDuration;

            if (percent > _slowPercent)
            {
                if (_slowPercent > 0f) _stats.GetStat(StatType.MoveSpeed).RemoveModifiersFromSource(this);
                _slowPercent = percent;
                _stats.GetStat(StatType.MoveSpeed).AddModifier(new StatModifier(-percent, ModifierType.Percent, this));
            }
        }

        public void Tick(float deltaTime)
        {
            if (_stunRemaining > 0f)
            {
                _stunRemaining -= deltaTime;
                if (_stunRemaining < 0f) _stunRemaining = 0f;
            }

            if (_slowRemaining > 0f)
            {
                _slowRemaining -= deltaTime;
                if (_slowRemaining <= 0f)
                {
                    _slowRemaining = 0f;
                    _slowPercent = 0f;
                    _stats.GetStat(StatType.MoveSpeed).RemoveModifiersFromSource(this);
                }
            }
        }

        private float ApplyTenacity(float duration)
        {
            float tenacity = _stats.GetValue(StatType.Tenacity);
            if (tenacity > TenacityCap) tenacity = TenacityCap;
            if (tenacity < 0f) tenacity = 0f;
            return duration * (1f - tenacity);
        }
    }
}
