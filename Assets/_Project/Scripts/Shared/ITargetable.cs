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
    }
}
