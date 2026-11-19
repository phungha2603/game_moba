using System.Reflection;
using Project.Character;
using Project.Core;
using Project.Shared;
using Project.Stats;
using UnityEngine;

internal static class ChaseAttackStateTests
{
    private static void InvokePrivate(object target, string methodName)
    {
        MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        method.Invoke(target, null);
    }

    private static (CharacterBrain brain, LogicTicker ticker) Spawn(float moveSpeed, float attackSpeed)
    {
        LogicTickerTests.ResetSingleton();

        var tickerGO = new GameObject("Ticker");
        LogicTicker ticker = tickerGO.AddComponent<LogicTicker>();

        var baseStats = new BaseStats { baseMoveSpeed = moveSpeed, baseAttackSpeed = attackSpeed };
        var heroGO = new GameObject("Hero");
        CharacterStats stats = heroGO.AddComponent<CharacterStats>();
        stats.Initialize(baseStats);
        CharacterBrain brain = heroGO.AddComponent<CharacterBrain>();

        return (brain, ticker);
    }

    private static void Tick(LogicTicker ticker)
    {
        Time.deltaTime = LogicTicker.FixedDeltaTime;
        InvokePrivate(ticker, "Update");
    }

    public static void EngageTarget_TransitionsToChaseState()
    {
        (CharacterBrain brain, _) = Spawn(moveSpeed: 5f, attackSpeed: 1f);
        var target = new FakeTargetable { Position = new Vector3(10f, 0f, 0f), Team = Team.Red, Type = TargetType.Hero };

        brain.EngageTarget(target);

        TestRunner.Assert(brain.StateMachine.CurrentState == brain.ChaseState, "EngageTarget should switch the brain into ChaseState");
    }

    public static void ChaseState_ClosesDistance_ThenTransitionsToAttackStateInRange()
    {
        (CharacterBrain brain, LogicTicker ticker) = Spawn(moveSpeed: 5f, attackSpeed: 1f);
        var target = new FakeTargetable { Position = new Vector3(3f, 0f, 0f), Team = Team.Red, Type = TargetType.Hero };
        brain.EngageTarget(target);

        bool reachedAttackState = false;
        for (int i = 0; i < 60 && !reachedAttackState; i++)
        {
            Tick(ticker);
            reachedAttackState = brain.StateMachine.CurrentState == brain.AttackState;
        }

        TestRunner.Assert(reachedAttackState, "Chasing a target within 60 ticks should eventually enter AttackState");
        float distance = Vector3.Distance(brain.transform.position, target.Position);
        TestRunner.Assert(distance <= brain.AttackRange + 0.01f, $"Brain should have closed to within AttackRange (got distance {distance})");
    }

    public static void AttackState_PerformsAttack_OnceClockIsReady()
    {
        // attackSpeed=30 => 1 attack/tick at 30Hz, so AttackState becomes ready on tick 1.
        (CharacterBrain brain, LogicTicker ticker) = Spawn(moveSpeed: 5f, attackSpeed: 30f);
        var target = new FakeTargetable { Position = new Vector3(1f, 0f, 0f), Team = Team.Red, Type = TargetType.Hero };
        brain.EngageTarget(target);

        Tick(ticker); // already in range (1 <= default AttackRange 2) -> ChaseState switches straight to AttackState this tick
        TestRunner.Assert(brain.StateMachine.CurrentState == brain.AttackState, "Target already in range should enter AttackState on the first tick");

        Tick(ticker); // AttackClock should be ready by now and PerformAttack() resets it
        TestRunner.Assert(!brain.AttackClock.CanAttack, "AttackClock should have been reset by PerformAttack after becoming ready");
    }

    public static void MovingWhileChasing_InterruptsChaseAndClearsTarget()
    {
        (CharacterBrain brain, LogicTicker ticker) = Spawn(moveSpeed: 5f, attackSpeed: 1f);
        var target = new FakeTargetable { Position = new Vector3(10f, 0f, 0f), Team = Team.Red, Type = TargetType.Hero };
        brain.EngageTarget(target);

        brain.MoveDirection = new Vector3(0f, 0f, 1f);
        Tick(ticker);

        TestRunner.Assert(brain.StateMachine.CurrentState == brain.MoveState, "Player-issued movement should interrupt ChaseState");
        TestRunner.Assert(brain.Target == null, "Interrupting Chase should clear the previous target");
    }
}
