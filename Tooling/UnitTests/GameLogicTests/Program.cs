using System;
using System.Collections.Generic;

internal static class Program
{
    private static int Main()
    {
        var tests = new List<(string Name, Action Test)>
        {
            ("StatModifierTests.FlatAndPercentStack_ComputesCorrectFinalValue", StatModifierTests.FlatAndPercentStack_ComputesCorrectFinalValue),
            ("StatModifierTests.DirtyFlag_RecalculatesOnlyWhenModifiersChange", StatModifierTests.DirtyFlag_RecalculatesOnlyWhenModifiersChange),
            ("StateMachineTests.ChangeState_CallsExitThenEnter", StateMachineTests.ChangeState_CallsExitThenEnter),
            ("StateMachineTests.ChangeState_SameStateIsNoOp", StateMachineTests.ChangeState_SameStateIsNoOp),
            ("PoolManagerTests.Get_ReusesPrewarmedInstances", PoolManagerTests.Get_ReusesPrewarmedInstances),
            ("PoolManagerTests.Return_MakesInstanceAvailableAgainAndCallsOnDespawned", PoolManagerTests.Return_MakesInstanceAvailableAgainAndCallsOnDespawned),
            ("LogicTickerTests.Update_AccumulatesAndFiresOnTick_AtFixedRate", LogicTickerTests.Update_AccumulatesAndFiresOnTick_AtFixedRate),
            ("CharacterBrainTests.OnEnable_StartsInIdleState", CharacterBrainTests.OnEnable_StartsInIdleState),
            ("CharacterBrainTests.SettingMoveDirection_TransitionsToMoveStateAndMovesByMoveSpeed", CharacterBrainTests.SettingMoveDirection_TransitionsToMoveStateAndMovesByMoveSpeed),
            ("TargetSelectorTests.PicksHeroOverFartherMinion_ByPriorityNotDistance", TargetSelectorTests.PicksHeroOverFartherMinion_ByPriorityNotDistance),
            ("TargetSelectorTests.TiesOnPriority_PicksClosestCandidate", TargetSelectorTests.TiesOnPriority_PicksClosestCandidate),
            ("TargetSelectorTests.SkipsAllies", TargetSelectorTests.SkipsAllies),
            ("TargetSelectorTests.SkipsDeadCandidates", TargetSelectorTests.SkipsDeadCandidates),
            ("TargetSelectorTests.SkipsCandidatesOutOfRange", TargetSelectorTests.SkipsCandidatesOutOfRange),
            ("AttackClockTests.CanAttack_IsFalseUntilIntervalElapsed_ThenTrue", AttackClockTests.CanAttack_IsFalseUntilIntervalElapsed_ThenTrue),
            ("AttackClockTests.Reset_ClearsElapsedTime", AttackClockTests.Reset_ClearsElapsedTime),
            ("AttackClockTests.ZeroAttacksPerSecond_NeverReady", AttackClockTests.ZeroAttacksPerSecond_NeverReady),
            ("ChaseAttackStateTests.EngageTarget_TransitionsToChaseState", ChaseAttackStateTests.EngageTarget_TransitionsToChaseState),
            ("ChaseAttackStateTests.ChaseState_ClosesDistance_ThenTransitionsToAttackStateInRange", ChaseAttackStateTests.ChaseState_ClosesDistance_ThenTransitionsToAttackStateInRange),
            ("ChaseAttackStateTests.AttackState_PerformsAttack_OnceClockIsReady", ChaseAttackStateTests.AttackState_PerformsAttack_OnceClockIsReady),
            ("ChaseAttackStateTests.MovingWhileChasing_InterruptsChaseAndClearsTarget", ChaseAttackStateTests.MovingWhileChasing_InterruptsChaseAndClearsTarget),
        };

        return TestRunner.Run(tests);
    }
}
