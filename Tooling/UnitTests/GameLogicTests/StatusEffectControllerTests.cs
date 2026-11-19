using Project.Combat;
using Project.Shared;
using Project.Stats;
using UnityEngine;

internal static class StatusEffectControllerTests
{
    private static (StatusEffectController controller, CharacterStats stats) Spawn(float tenacity = 0f)
    {
        var baseStats = new BaseStats { baseTenacity = tenacity };
        var go = new GameObject("Unit");
        CharacterStats stats = go.AddComponent<CharacterStats>();
        stats.Initialize(baseStats);
        StatusEffectController controller = go.AddComponent<StatusEffectController>();
        return (controller, stats);
    }

    public static void ApplyStun_SetsIsStunnedUntilDurationElapses()
    {
        (StatusEffectController controller, _) = Spawn();
        controller.ApplyStun(1f);

        TestRunner.Assert(controller.IsStunned, "Should be stunned immediately after ApplyStun");

        controller.Tick(0.5f);
        TestRunner.Assert(controller.IsStunned, "Should still be stunned halfway through duration");

        controller.Tick(0.6f);
        TestRunner.Assert(!controller.IsStunned, "Should no longer be stunned after duration elapses");
    }

    public static void ApplyStun_ShorterReapplication_DoesNotShortenRemainingDuration()
    {
        (StatusEffectController controller, _) = Spawn();
        controller.ApplyStun(1f);
        controller.Tick(0.5f); // 0.5s remaining
        controller.ApplyStun(0.3f); // shorter than remaining -> must not shorten it

        controller.Tick(0.4f); // 0.1s should remain from the original stun
        TestRunner.Assert(controller.IsStunned, "A shorter re-application must not shorten the remaining stun");
    }

    public static void Tenacity_ReducesStunDuration()
    {
        (StatusEffectController controller, _) = Spawn(tenacity: 0.5f);
        controller.ApplyStun(1f); // 1 * (1-0.5) = 0.5s

        controller.Tick(0.4f);
        TestRunner.Assert(controller.IsStunned, "0.4s elapsed of a 0.5s (post-tenacity) stun should still be active");

        controller.Tick(0.2f);
        TestRunner.Assert(!controller.IsStunned, "0.6s total should exceed the 0.5s post-tenacity duration");
    }

    public static void Tenacity_IsCappedAt80Percent()
    {
        (StatusEffectController controller, _) = Spawn(tenacity: 2.0f); // absurd value, should clamp to 0.8
        controller.ApplyStun(1f); // 1 * (1-0.8) = 0.2s floor, never full immunity

        controller.Tick(0.19f);
        TestRunner.Assert(controller.IsStunned, "Even with huge tenacity, stun should last at least the 20% floor");

        controller.Tick(0.02f);
        TestRunner.Assert(!controller.IsStunned, "Stun should still expire eventually");
    }

    public static void ApplySlow_AddsMoveSpeedModifier_RemovedAfterDuration()
    {
        (StatusEffectController controller, CharacterStats stats) = Spawn();
        float baseline = stats.GetValue(StatType.MoveSpeed);

        controller.ApplySlow(0.5f, 1f);
        TestRunner.Assert(stats.GetValue(StatType.MoveSpeed) < baseline, "MoveSpeed should drop while slowed");

        controller.Tick(1.1f);
        TestRunner.AssertEqual(baseline, stats.GetValue(StatType.MoveSpeed), "MoveSpeed should return to baseline once the slow expires");
    }

    public static void ApplySlow_StrongerSlowReplacesWeakerOne()
    {
        (StatusEffectController controller, CharacterStats stats) = Spawn();

        controller.ApplySlow(0.2f, 1f);
        float afterWeak = stats.GetValue(StatType.MoveSpeed);

        controller.ApplySlow(0.6f, 1f);
        float afterStrong = stats.GetValue(StatType.MoveSpeed);

        TestRunner.Assert(afterStrong < afterWeak, "A stronger slow should reduce MoveSpeed further than the weaker one it replaces");
    }
}
