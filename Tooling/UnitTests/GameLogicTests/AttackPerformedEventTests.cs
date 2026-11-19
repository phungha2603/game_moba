using System.Reflection;
using Project.Character;
using Project.Combat;
using Project.Core;
using Project.Lane;
using Project.Shared;
using Project.Stats;
using UnityEngine;

internal static class AttackPerformedEventTests
{
    private static void InvokePrivate(object target, string methodName)
    {
        MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        method.Invoke(target, null);
    }

    public static void CharacterBrain_PerformAttack_RaisesOnAttackPerformed()
    {
        LogicTickerTests.ResetSingleton();
        var tickerGO = new GameObject("Ticker");
        tickerGO.AddComponent<LogicTicker>();

        var heroGO = new GameObject("Hero");
        CharacterStats stats = heroGO.AddComponent<CharacterStats>();
        stats.Initialize(new BaseStats());
        CharacterBrain brain = heroGO.AddComponent<CharacterBrain>();

        int raisedCount = 0;
        brain.OnAttackPerformed += () => raisedCount++;
        brain.EngageTarget(new FakeTargetable());

        brain.PerformAttack();

        TestRunner.AssertEqual(1, raisedCount, "CharacterBrain should raise OnAttackPerformed exactly once per PerformAttack call");
    }

    public static void MinionBrain_PerformAttack_RaisesOnAttackPerformed()
    {
        LogicTickerTests.ResetSingleton();
        var tickerGO = new GameObject("Ticker");
        tickerGO.AddComponent<LogicTicker>();

        var minionGO = new GameObject("Minion");
        CharacterStats stats = minionGO.AddComponent<CharacterStats>();
        stats.Initialize(new BaseStats());
        minionGO.AddComponent<Targetable>().Initialize(Team.Blue, TargetType.Minion);
        MinionBrain brain = minionGO.AddComponent<MinionBrain>();
        brain.SetPath(new LanePath(Vector3.zero, new Vector3(1f, 0f, 0f)));

        int raisedCount = 0;
        brain.OnAttackPerformed += () => raisedCount++;
        brain.EngageTarget(new FakeTargetable());

        brain.PerformAttack();

        TestRunner.AssertEqual(1, raisedCount, "MinionBrain should raise OnAttackPerformed exactly once per PerformAttack call");
    }
}
