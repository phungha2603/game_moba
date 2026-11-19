using UnityEngine;

namespace Project.Shared
{
    public interface ITargetable
    {
        Vector3 Position { get; }
        Team Team { get; }
        TargetType Type { get; }
        bool IsAlive { get; }

        float GetStatValue(StatType statType);
        void TakeDamage(float amount, bool isCritical);
        void Heal(float amount);

        void ApplyStun(float duration);
        void ApplySlow(float percent, float duration);

        /// Whoever this unit is currently attacking (null if none). Lets a Tower ask any
        /// attacker — Hero, Minion, or another Tower — "are you hitting one of my allies?"
        /// without needing to know its concrete type.
        ITargetable CurrentAttackTarget { get; }
    }
}
