using Project.Skills;

internal static class SkillInstanceTests
{
    public static void IsReady_TrueInitially()
    {
        var data = new SkillSO { Cooldown = 5f };
        var instance = new SkillInstance(data);

        TestRunner.Assert(instance.IsReady, "A fresh skill instance should be ready to cast");
    }

    public static void StartCooldown_MakesNotReadyUntilElapsed()
    {
        var data = new SkillSO { Cooldown = 2f };
        var instance = new SkillInstance(data);

        instance.StartCooldown();
        TestRunner.Assert(!instance.IsReady, "Should not be ready right after starting cooldown");

        instance.Tick(1.9f);
        TestRunner.Assert(!instance.IsReady, "Should still not be ready before cooldown elapses");

        instance.Tick(0.2f);
        TestRunner.Assert(instance.IsReady, "Should be ready once cooldown fully elapses");
    }
}
