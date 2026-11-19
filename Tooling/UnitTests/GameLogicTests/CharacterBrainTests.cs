using System;
using System.Reflection;
using Project.Character;
using Project.Core;
using Project.Stats;
using UnityEngine;

internal static class CharacterBrainTests
{
    private static void InvokePrivate(object target, string methodName)
    {
        MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        method.Invoke(target, null);
    }

    private static (CharacterBrain brain, LogicTicker ticker) Spawn(float moveSpeed)
    {
        LogicTickerTests.ResetSingleton();

        var tickerGO = new GameObject("Ticker");
        LogicTicker ticker = tickerGO.AddComponent<LogicTicker>();

        var baseStats = new BaseStats { baseMoveSpeed = moveSpeed };
        var heroGO = new GameObject("Hero");
        CharacterStats stats = heroGO.AddComponent<CharacterStats>();
        stats.Initialize(baseStats);
        CharacterBrain brain = heroGO.AddComponent<CharacterBrain>();

        return (brain, ticker);
    }

    public static void OnEnable_StartsInIdleState()
    {
        (CharacterBrain brain, _) = Spawn(moveSpeed: 5f);

        TestRunner.Assert(brain.StateMachine.CurrentState == brain.IdleState, "Brain should start in IdleState after OnEnable");
    }

    public static void SettingMoveDirection_TransitionsToMoveStateAndMovesByMoveSpeed()
    {
        (CharacterBrain brain, LogicTicker ticker) = Spawn(moveSpeed: 5f);
        brain.MoveDirection = new Vector3(1f, 0f, 0f);
        Time.deltaTime = LogicTicker.FixedDeltaTime;

        // Tick 1: IdleState.OnUpdate only transitions to MoveState (OnEnter), it does not also
        // invoke the new state's OnUpdate in the same tick. Movement itself happens on tick 2.
        InvokePrivate(ticker, "Update");
        TestRunner.Assert(brain.StateMachine.CurrentState == brain.MoveState, "Brain should transition to MoveState once MoveDirection is non-zero");
        TestRunner.AssertEqual(0f, brain.transform.position.x, "No movement yet on the tick that only entered MoveState");

        InvokePrivate(ticker, "Update");
        float expectedDistance = 5f * LogicTicker.FixedDeltaTime;
        TestRunner.Assert(Math.Abs(brain.transform.position.x - expectedDistance) < 0.0001f,
            $"Hero should move by MoveSpeed*dt along X on the following tick (expected {expectedDistance}, got {brain.transform.position.x})");
    }
}
