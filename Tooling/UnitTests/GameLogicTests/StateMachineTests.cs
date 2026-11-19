using System.Collections.Generic;
using System.Linq;
using Project.Core;

internal static class StateMachineTests
{
    private class RecordingState : IState
    {
        public readonly List<string> Log = new List<string>();
        public void OnEnter() => Log.Add("Enter");
        public void OnUpdate(float deltaTime) => Log.Add($"Update:{deltaTime}");
        public void OnExit() => Log.Add("Exit");
    }

    public static void ChangeState_CallsExitThenEnter()
    {
        var sm = new StateMachine();
        var a = new RecordingState();
        var b = new RecordingState();

        sm.ChangeState(a);
        sm.ChangeState(b);

        TestRunner.Assert(a.Log.SequenceEqual(new[] { "Enter", "Exit" }), "State A should Enter then Exit");
        TestRunner.Assert(b.Log.SequenceEqual(new[] { "Enter" }), "State B should only Enter");
    }

    public static void ChangeState_SameStateIsNoOp()
    {
        var sm = new StateMachine();
        var a = new RecordingState();

        sm.ChangeState(a);
        sm.ChangeState(a);

        TestRunner.Assert(a.Log.SequenceEqual(new[] { "Enter" }), "Changing to the same state must not re-enter");
    }
}
