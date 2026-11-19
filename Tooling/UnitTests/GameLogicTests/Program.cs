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
        };

        return TestRunner.Run(tests);
    }
}
