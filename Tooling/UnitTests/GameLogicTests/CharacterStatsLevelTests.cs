using Project.Shared;
using Project.Stats;
using UnityEngine;

internal static class CharacterStatsLevelTests
{
    private static CharacterStats Spawn()
    {
        var baseStats = new BaseStats
        {
            baseHealth = 500f,
            healthGrowth = 80f,
            baseAttackDamage = 50f,
            attackDamageGrowth = 3f
        };
        var go = new GameObject("Unit");
        CharacterStats stats = go.AddComponent<CharacterStats>();
        stats.Initialize(baseStats);
        return stats;
    }

    public static void ApplyLevel_ScalesStatsLinearlyWithGrowth()
    {
        CharacterStats stats = Spawn();

        stats.ApplyLevel(3); // steps = 2

        TestRunner.AssertEqual(500f + 80f * 2, stats.GetValue(StatType.Health), "Health should scale as base + growth*(level-1)");
        TestRunner.AssertEqual(50f + 3f * 2, stats.GetValue(StatType.AttackDamage), "AttackDamage should scale the same way");
    }

    public static void ApplyLevel_AddsTheMaxHealthGainOntoCurrentHealth()
    {
        CharacterStats stats = Spawn();
        stats.ApplyDamage(100f); // CurrentHealth = 400, MaxHealth = 500

        stats.ApplyLevel(5); // steps=4 -> new MaxHealth = 500 + 80*4 = 820, gain = 320

        TestRunner.AssertEqual(720f, stats.CurrentHealth, "CurrentHealth should rise by exactly the Max Health gained, so the health bar never looks like it shrank");
    }

    public static void ApplyLevel_NeverLetsCurrentHealthExceedNewMax()
    {
        CharacterStats stats = Spawn(); // full HP: Current == Max == 500

        stats.ApplyLevel(5); // Max becomes 820; gain is added, Current should land exactly at the new Max, not beyond

        TestRunner.AssertEqual(stats.GetValue(StatType.Health), stats.CurrentHealth, "Starting at full HP, leveling up should keep CurrentHealth exactly at the new Max");
    }

    public static void ApplyLevel_StillRespectsExistingItemModifiers()
    {
        CharacterStats stats = Spawn();
        stats.GetStat(StatType.AttackDamage).AddModifier(new StatModifier(10f, ModifierType.Flat, "item"));

        stats.ApplyLevel(2); // AD base becomes 53

        TestRunner.AssertEqual(63f, stats.GetValue(StatType.AttackDamage), "Item modifier should still apply on top of the new leveled base value");
    }
}
