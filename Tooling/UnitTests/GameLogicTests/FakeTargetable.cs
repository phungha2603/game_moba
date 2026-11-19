using Project.Shared;
using UnityEngine;

internal class FakeTargetable : ITargetable
{
    public Vector3 Position { get; set; }
    public Team Team { get; set; }
    public TargetType Type { get; set; }
    public bool IsAlive { get; set; } = true;

    public float ArmorValue { get; set; }
    public ITargetable CurrentAttackTarget { get; set; }
    public float GoldReward { get; set; }
    public float ExpReward { get; set; }

    public int TakeDamageCallCount { get; private set; }
    public float LastDamageAmount { get; private set; }
    public bool LastDamageWasCritical { get; private set; }
    public ITargetable LastAttacker { get; private set; }

    public int HealCallCount { get; private set; }
    public float LastHealAmount { get; private set; }

    public int StunCallCount { get; private set; }
    public float LastStunDuration { get; private set; }

    public int SlowCallCount { get; private set; }
    public float LastSlowPercent { get; private set; }
    public float LastSlowDuration { get; private set; }

    public int NotifyKillCallCount { get; private set; }
    public ITargetable LastKillVictim { get; private set; }

    public float GetStatValue(StatType statType) => ArmorValue;

    public void TakeDamage(float amount, bool isCritical, ITargetable attacker)
    {
        TakeDamageCallCount++;
        LastDamageAmount = amount;
        LastDamageWasCritical = isCritical;
        LastAttacker = attacker;
    }

    public void Heal(float amount)
    {
        HealCallCount++;
        LastHealAmount = amount;
    }

    public void ApplyStun(float duration)
    {
        StunCallCount++;
        LastStunDuration = duration;
    }

    public void ApplySlow(float percent, float duration)
    {
        SlowCallCount++;
        LastSlowPercent = percent;
        LastSlowDuration = duration;
    }

    public void NotifyKill(ITargetable victim)
    {
        NotifyKillCallCount++;
        LastKillVictim = victim;
    }
}
