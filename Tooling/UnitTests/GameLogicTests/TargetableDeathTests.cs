using Project.Combat;
using Project.Shared;
using Project.Stats;
using UnityEngine;

internal static class TargetableDeathTests
{
    private static Targetable Spawn(float health)
    {
        var baseStats = new BaseStats { baseHealth = health };
        var go = new GameObject("Unit");
        CharacterStats stats = go.AddComponent<CharacterStats>();
        stats.Initialize(baseStats);
        Targetable targetable = go.AddComponent<Targetable>();
        targetable.Initialize(Team.Red, TargetType.Minion);
        return targetable;
    }

    public static void LethalDamage_NotifiesAttackerOfKill()
    {
        Targetable victim = Spawn(health: 50f);
        var attacker = new FakeTargetable();

        victim.TakeDamage(999f, false, attacker);

        TestRunner.AssertEqual(1, attacker.NotifyKillCallCount, "A killing blow must notify the attacker exactly once");
        TestRunner.Assert(ReferenceEquals(attacker.LastKillVictim, victim), "The attacker should be told who it killed");
    }

    public static void NonLethalDamage_DoesNotNotifyKill()
    {
        Targetable victim = Spawn(health: 100f);
        var attacker = new FakeTargetable();

        victim.TakeDamage(30f, false, attacker);

        TestRunner.AssertEqual(0, attacker.NotifyKillCallCount, "Non-lethal damage must not count as a kill");
    }

    public static void AlreadyDead_FurtherDamageDoesNotNotifyKillAgain()
    {
        Targetable victim = Spawn(health: 50f);
        var attacker = new FakeTargetable();

        victim.TakeDamage(999f, false, attacker); // kill
        victim.TakeDamage(999f, false, attacker); // overkill

        TestRunner.AssertEqual(1, attacker.NotifyKillCallCount, "A unit can only be killed once, however many hits land afterward");
    }

    public static void NullAttacker_DoesNotThrow()
    {
        Targetable victim = Spawn(health: 50f);

        victim.TakeDamage(999f, false, null); // must not throw

        TestRunner.Assert(!victim.IsAlive, "Victim should still die even with no attacker reference (e.g. environmental damage)");
    }
}
