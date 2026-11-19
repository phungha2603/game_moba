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

        /// attacker may be null (environmental damage, or the source despawned) — implementers
        /// must tolerate that.
        void TakeDamage(float amount, bool isCritical, ITargetable attacker);
        void Heal(float amount);

        void ApplyStun(float duration);
        void ApplySlow(float percent, float duration);

        /// Whoever this unit is currently attacking (null if none). Lets a Tower ask any
        /// attacker — Hero, Minion, or another Tower — "are you hitting one of my allies?"
        /// without needing to know its concrete type.
        ITargetable CurrentAttackTarget { get; }

        /// Gold/EXP this unit is worth when killed. Not combat stats (never modified by
        /// buffs/items), so these live here rather than in StatType/RuntimeStat.
        float GoldReward { get; }
        float ExpReward { get; }

        /// Called on the attacker (never on the victim) the instant its damage is the killing
        /// blow — lets Project.Economy award a kill without Project.Combat ever depending on it.
        void NotifyKill(ITargetable victim);
    }
}
