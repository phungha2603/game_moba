using Project.Shared;
using UnityEngine;

internal class FakeTargetable : ITargetable
{
    public Vector3 Position { get; set; }
    public Team Team { get; set; }
    public TargetType Type { get; set; }
    public bool IsAlive { get; set; } = true;

    public float ArmorValue { get; set; }

    public int TakeDamageCallCount { get; private set; }
    public float LastDamageAmount { get; private set; }
    public bool LastDamageWasCritical { get; private set; }

    public float GetStatValue(StatType statType) => ArmorValue;

    public void TakeDamage(float amount, bool isCritical)
    {
        TakeDamageCallCount++;
        LastDamageAmount = amount;
        LastDamageWasCritical = isCritical;
    }
}
