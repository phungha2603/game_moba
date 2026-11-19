using Project.Combat;

internal static class AttackClockTests
{
    public static void CanAttack_IsFalseUntilIntervalElapsed_ThenTrue()
    {
        var clock = new AttackClock { AttacksPerSecond = 2f }; // interval = 0.5s

        TestRunner.Assert(!clock.CanAttack, "Freshly created clock should not be ready to attack yet");

        clock.Tick(0.4f);
        TestRunner.Assert(!clock.CanAttack, "0.4s elapsed should not be enough for a 0.5s interval");

        clock.Tick(0.1f);
        TestRunner.Assert(clock.CanAttack, "0.5s elapsed should be enough for a 0.5s interval");
    }

    public static void Reset_ClearsElapsedTime()
    {
        var clock = new AttackClock { AttacksPerSecond = 10f }; // interval = 0.1s
        clock.Tick(1f);
        TestRunner.Assert(clock.CanAttack, "Sanity check: should be ready before Reset");

        clock.Reset();
        TestRunner.Assert(!clock.CanAttack, "Reset should clear the elapsed time back to not-ready");
    }

    public static void ZeroAttacksPerSecond_NeverReady()
    {
        var clock = new AttackClock { AttacksPerSecond = 0f };
        clock.Tick(1000f);

        TestRunner.Assert(!clock.CanAttack, "Zero attack speed must never become ready to attack");
    }
}
