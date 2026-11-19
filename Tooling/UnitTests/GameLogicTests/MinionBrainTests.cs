using System.Reflection;
using Project.Combat;
using Project.Core;
using Project.Lane;
using Project.Shared;
using Project.Stats;
using UnityEngine;

internal static class MinionBrainTests
{
    private static void InvokePrivate(object target, string methodName)
    {
        MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        method.Invoke(target, null);
    }

    private static (MinionBrain brain, LogicTicker ticker) Spawn(float moveSpeed, float attackSpeed, Vector3 start, Vector3 end)
    {
        LogicTickerTests.ResetSingleton();

        var tickerGO = new GameObject("Ticker");
        LogicTicker ticker = tickerGO.AddComponent<LogicTicker>();

        var baseStats = new BaseStats { baseMoveSpeed = moveSpeed, baseAttackSpeed = attackSpeed };
        var minionGO = new GameObject("Minion");
        minionGO.transform.position = start;
        CharacterStats stats = minionGO.AddComponent<CharacterStats>();
        stats.Initialize(baseStats);
        Targetable targetable = minionGO.AddComponent<Targetable>();
        targetable.Initialize(Team.Blue, TargetType.Minion);
        MinionBrain brain = minionGO.AddComponent<MinionBrain>();
        brain.SetPath(new LanePath(start, end));

        return (brain, ticker);
    }

    private static void Tick(LogicTicker ticker)
    {
        Time.deltaTime = LogicTicker.FixedDeltaTime;
        InvokePrivate(ticker, "Update");
    }

    public static void OnEnable_StartsInMoveState()
    {
        (MinionBrain brain, _) = Spawn(moveSpeed: 3f, attackSpeed: 1f, Vector3.zero, new Vector3(10f, 0f, 0f));

        TestRunner.Assert(brain.StateMachine.CurrentState == brain.MoveState, "Minion should start marching");
    }

    public static void MoveState_WalksTowardPathEnd()
    {
        (MinionBrain brain, LogicTicker ticker) = Spawn(moveSpeed: 3f, attackSpeed: 1f, Vector3.zero, new Vector3(10f, 0f, 0f));

        Tick(ticker);

        float expected = 3f * LogicTicker.FixedDeltaTime;
        TestRunner.Assert(System.Math.Abs(brain.transform.position.x - expected) < 0.0001f, "Minion should move toward path end by MoveSpeed*dt");
    }

    public static void EngageTarget_TransitionsToChaseState()
    {
        (MinionBrain brain, _) = Spawn(moveSpeed: 3f, attackSpeed: 1f, Vector3.zero, new Vector3(10f, 0f, 0f));
        var target = new FakeTargetable { Position = new Vector3(2f, 0f, 0f), Team = Team.Red };

        brain.EngageTarget(target);

        TestRunner.Assert(brain.StateMachine.CurrentState == brain.ChaseState, "Engaging a target should switch to Chase");
    }

    public static void ChaseState_TargetBeyondAggroRange_ReturnsToMove()
    {
        (MinionBrain brain, LogicTicker ticker) = Spawn(moveSpeed: 3f, attackSpeed: 1f, Vector3.zero, new Vector3(10f, 0f, 0f));
        var farTarget = new FakeTargetable { Position = new Vector3(100f, 0f, 0f), Team = Team.Red };
        brain.EngageTarget(farTarget);

        Tick(ticker);

        TestRunner.Assert(brain.StateMachine.CurrentState == brain.MoveState, "A target beyond AggroRange should make the minion give up and resume marching");
        TestRunner.Assert(brain.Target == null, "Giving up the chase should clear Target");
    }

    public static void ChaseState_ClosesIn_ThenAttacks()
    {
        (MinionBrain brain, LogicTicker ticker) = Spawn(moveSpeed: 3f, attackSpeed: 1f, Vector3.zero, new Vector3(10f, 0f, 0f));
        var target = new FakeTargetable { Position = new Vector3(2f, 0f, 0f), Team = Team.Red };
        brain.EngageTarget(target);

        bool reachedAttack = false;
        for (int i = 0; i < 60 && !reachedAttack; i++)
        {
            Tick(ticker);
            reachedAttack = brain.StateMachine.CurrentState == brain.AttackState;
        }

        TestRunner.Assert(reachedAttack, "Minion should eventually close the distance and start attacking");
    }

    public static void AttackState_DealsDamageOnceClockReady()
    {
        (MinionBrain brain, LogicTicker ticker) = Spawn(moveSpeed: 3f, attackSpeed: 30f, Vector3.zero, new Vector3(10f, 0f, 0f));
        var target = new FakeTargetable { Position = new Vector3(0.5f, 0f, 0f), Team = Team.Red };
        brain.EngageTarget(target);

        Tick(ticker); // already in range -> AttackState this tick
        TestRunner.Assert(brain.StateMachine.CurrentState == brain.AttackState, "Target already in range should enter AttackState immediately");

        Tick(ticker); // clock ready by now
        TestRunner.AssertEqual(1, target.TakeDamageCallCount, "Minion should land exactly one hit once the attack clock is ready");
    }

    public static void TakingLethalDamage_TransitionsToDeadState()
    {
        (MinionBrain brain, _) = Spawn(moveSpeed: 3f, attackSpeed: 1f, Vector3.zero, new Vector3(10f, 0f, 0f));

        brain.Stats.ApplyDamage(99999f);

        TestRunner.Assert(brain.StateMachine.CurrentState == brain.DeadState, "Lethal damage should move the minion to DeadState");
    }
}
