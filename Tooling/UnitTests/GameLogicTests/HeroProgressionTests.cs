using System.Reflection;
using Project.Combat;
using Project.Core;
using Project.Economy;
using Project.Shared;
using Project.Stats;
using UnityEngine;

internal static class HeroProgressionTests
{
    private static void InvokePrivate(object target, string methodName)
    {
        MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        method.Invoke(target, null);
    }

    private static (HeroProgression progression, Targetable targetable, LogicTicker ticker) Spawn()
    {
        LogicTickerTests.ResetSingleton();

        var tickerGO = new GameObject("Ticker");
        LogicTicker ticker = tickerGO.AddComponent<LogicTicker>();

        var baseStats = new BaseStats();
        var heroGO = new GameObject("Hero");
        CharacterStats stats = heroGO.AddComponent<CharacterStats>();
        stats.Initialize(baseStats);
        Targetable targetable = heroGO.AddComponent<Targetable>();
        targetable.Initialize(Team.Blue, TargetType.Hero);
        HeroProgression progression = heroGO.AddComponent<HeroProgression>();

        return (progression, targetable, ticker);
    }

    private static void Tick(LogicTicker ticker)
    {
        Time.deltaTime = LogicTicker.FixedDeltaTime;
        InvokePrivate(ticker, "Update");
    }

    public static void PassiveIncome_AccruesGoldOverTime()
    {
        (HeroProgression progression, _, LogicTicker ticker) = Spawn();

        for (int i = 0; i < 30; i++) Tick(ticker); // ~1 second of ticks

        TestRunner.Assert(progression.CurrentGold > 0f, "Gold should accrue passively over time");
    }

    public static void ScoringAKill_AwardsGoldAndExpFromTheVictim()
    {
        (HeroProgression progression, Targetable targetable, _) = Spawn();
        var victim = new FakeTargetable { GoldReward = 25f, ExpReward = 40f };

        targetable.NotifyKill(victim);

        TestRunner.AssertEqual(25f, progression.CurrentGold, "Kill should award the victim's GoldReward");
        TestRunner.AssertEqual(40f, progression.CurrentExp, "Kill should award the victim's ExpReward");
    }

    public static void EnoughExp_LevelsUpAndAppliesToCharacterStats()
    {
        (HeroProgression progression, Targetable targetable, _) = Spawn();
        CharacterStats stats = targetable.GetComponent<CharacterStats>();
        float adBeforeLevelUp = stats.GetValue(StatType.AttackDamage);

        progression.AddExp(HeroProgression.ExpToReachNextLevel(1)); // exactly enough for level 2

        TestRunner.AssertEqual(2, progression.Level, "Enough EXP should trigger exactly one level-up");
        TestRunner.Assert(stats.GetValue(StatType.AttackDamage) > adBeforeLevelUp, "Leveling up should raise level-scaled stats via CharacterStats.ApplyLevel");
    }

    public static void NotEnoughExp_StaysAtLevelOne()
    {
        (HeroProgression progression, _, _) = Spawn();

        progression.AddExp(HeroProgression.ExpToReachNextLevel(1) - 1f);

        TestRunner.AssertEqual(1, progression.Level, "Insufficient EXP must not trigger a level-up");
    }

    public static void SpendGold_FailsWhenInsufficient()
    {
        (HeroProgression progression, _, _) = Spawn();

        bool spent = progression.SpendGold(999999f);

        TestRunner.Assert(!spent, "Spending more gold than available must fail");
        TestRunner.AssertEqual(0f, progression.CurrentGold, "A failed spend must not change the gold balance");
    }
}
