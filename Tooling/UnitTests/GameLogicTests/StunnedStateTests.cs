using System.Reflection;
using Project.Character;
using Project.Combat;
using Project.Core;
using Project.Stats;
using UnityEngine;

internal static class StunnedStateTests
{
    private static void InvokePrivate(object target, string methodName)
    {
        MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        method.Invoke(target, null);
    }

    private static (CharacterBrain brain, LogicTicker ticker) Spawn()
    {
        LogicTickerTests.ResetSingleton();

        var tickerGO = new GameObject("Ticker");
        LogicTicker ticker = tickerGO.AddComponent<LogicTicker>();

        var baseStats = new BaseStats { baseMoveSpeed = 5f };
        var heroGO = new GameObject("Hero");
        CharacterStats stats = heroGO.AddComponent<CharacterStats>();
        stats.Initialize(baseStats);
        heroGO.AddComponent<StatusEffectController>();
        CharacterBrain brain = heroGO.AddComponent<CharacterBrain>();

        return (brain, ticker);
    }

    private static void Tick(LogicTicker ticker)
    {
        Time.deltaTime = LogicTicker.FixedDeltaTime;
        InvokePrivate(ticker, "Update");
    }

    public static void BeingStunned_ForcesStunnedStateAndIgnoresMoveInput()
    {
        (CharacterBrain brain, LogicTicker ticker) = Spawn();
        brain.StatusEffects.ApplyStun(1f);
        brain.MoveDirection = new Vector3(1f, 0f, 0f);

        Tick(ticker);

        TestRunner.Assert(brain.StateMachine.CurrentState == brain.StunnedState, "Being stunned should force StunnedState regardless of MoveDirection");
        TestRunner.AssertEqual(0f, brain.transform.position.x, "Stunned brain must not move even with MoveDirection set");
    }

    public static void StunExpiring_ReturnsToIdleStateAfterDurationElapses()
    {
        (CharacterBrain brain, LogicTicker ticker) = Spawn();
        brain.StatusEffects.ApplyStun(LogicTicker.FixedDeltaTime * 2.5f);

        Tick(ticker); // tick 1: still well within duration
        TestRunner.Assert(brain.StateMachine.CurrentState == brain.StunnedState, "Should be stunned on tick 1");

        Tick(ticker); // tick 2: still within duration
        TestRunner.Assert(brain.StateMachine.CurrentState == brain.StunnedState, "Should still be stunned on tick 2");

        Tick(ticker); // tick 3: 3 ticks worth of elapsed time exceeds the 2.5-tick duration
        TestRunner.Assert(brain.StateMachine.CurrentState == brain.IdleState, "Should return to Idle once the stun duration elapses");
    }
}
