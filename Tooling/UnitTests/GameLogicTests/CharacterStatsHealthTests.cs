using Project.Stats;

internal static class CharacterStatsHealthTests
{
    private static CharacterStats Spawn(float health)
    {
        var baseStats = new BaseStats { baseHealth = health };
        var go = new UnityEngine.GameObject("Unit");
        CharacterStats stats = go.AddComponent<CharacterStats>();
        stats.Initialize(baseStats);
        return stats;
    }

    public static void Initialize_SetsCurrentHealthToMax()
    {
        CharacterStats stats = Spawn(health: 600f);

        TestRunner.AssertEqual(600f, stats.CurrentHealth, "CurrentHealth should start at full Max Health");
        TestRunner.Assert(stats.IsAlive, "A freshly spawned unit should be alive");
    }

    public static void ApplyDamage_ReducesCurrentHealth_ClampedAtZero()
    {
        CharacterStats stats = Spawn(health: 100f);

        stats.ApplyDamage(40f);
        TestRunner.AssertEqual(60f, stats.CurrentHealth, "40 damage on 100 HP should leave 60");

        stats.ApplyDamage(1000f);
        TestRunner.AssertEqual(0f, stats.CurrentHealth, "Damage must clamp at 0, not go negative");
        TestRunner.Assert(!stats.IsAlive, "0 HP should mean not alive");
    }

    public static void Heal_IncreasesCurrentHealth_ClampedAtMax()
    {
        CharacterStats stats = Spawn(health: 100f);
        stats.ApplyDamage(80f);

        stats.Heal(10f);
        TestRunner.AssertEqual(30f, stats.CurrentHealth, "20 remaining + 10 heal = 30");

        stats.Heal(1000f);
        TestRunner.AssertEqual(100f, stats.CurrentHealth, "Heal must clamp at Max Health, not overflow");
    }

    public static void OnHealthChanged_FiresWithCurrentAndMax()
    {
        CharacterStats stats = Spawn(health: 100f);
        float? reportedCurrent = null;
        float? reportedMax = null;
        stats.OnHealthChanged += (current, max) =>
        {
            reportedCurrent = current;
            reportedMax = max;
        };

        stats.ApplyDamage(30f);

        TestRunner.AssertEqual(70f, reportedCurrent ?? -1f, "OnHealthChanged should report the new current health");
        TestRunner.AssertEqual(100f, reportedMax ?? -1f, "OnHealthChanged should report max health");
    }

    public static void OnDamaged_FiresOnDamage_ButNotOnHeal()
    {
        CharacterStats stats = Spawn(health: 100f);
        stats.ApplyDamage(50f);
        int damagedCallCount = 0;
        stats.OnDamaged += amount => damagedCallCount++;

        stats.Heal(10f);
        TestRunner.AssertEqual(0, damagedCallCount, "OnDamaged must not fire on a heal");

        stats.ApplyDamage(5f);
        TestRunner.AssertEqual(1, damagedCallCount, "OnDamaged must fire exactly once per ApplyDamage call");
    }
}
