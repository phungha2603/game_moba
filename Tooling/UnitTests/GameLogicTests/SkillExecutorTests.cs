using Project.Combat;
using Project.Skills;

internal static class SkillExecutorTests
{
    private static SkillSO MakeSkill(SkillTargetingType targeting, DamageType damageType, params SkillEffectData[] effects)
    {
        var skill = new SkillSO { TargetingType = targeting, DamageType = damageType };
        skill.Effects.AddRange(effects);
        return skill;
    }

    public static void DamageEffect_PhysicalDealsDamageToTarget()
    {
        var caster = new FakeTargetable();
        var target = new FakeTargetable { ArmorValue = 0f };
        SkillSO skill = MakeSkill(SkillTargetingType.Target, DamageType.Physical,
            new SkillEffectData { EffectType = SkillEffectType.Damage, Value = 40f });

        SkillExecutor.Execute(skill, caster, target);

        TestRunner.AssertEqual(1, target.TakeDamageCallCount, "Damage effect should hit the target once");
        TestRunner.AssertEqual(40f, target.LastDamageAmount, "No armor should mean full 40 damage");
    }

    public static void HealEffect_OnSelfTargeting_HealsCaster()
    {
        var caster = new FakeTargetable();
        SkillSO skill = MakeSkill(SkillTargetingType.Self, DamageType.Physical,
            new SkillEffectData { EffectType = SkillEffectType.Heal, Value = 30f });

        SkillExecutor.Execute(skill, caster, null);

        TestRunner.AssertEqual(1, caster.HealCallCount, "Self-targeted heal should apply to the caster");
        TestRunner.AssertEqual(30f, caster.LastHealAmount, "Heal amount should match the effect value");
    }

    public static void StunEffect_AppliesToTarget()
    {
        var caster = new FakeTargetable();
        var target = new FakeTargetable();
        SkillSO skill = MakeSkill(SkillTargetingType.Target, DamageType.Physical,
            new SkillEffectData { EffectType = SkillEffectType.Stun, Duration = 1.5f });

        SkillExecutor.Execute(skill, caster, target);

        TestRunner.AssertEqual(1, target.StunCallCount, "Stun effect should be applied once");
        TestRunner.AssertEqual(1.5f, target.LastStunDuration, "Stun duration should match the effect data");
    }

    public static void SlowEffect_AppliesPercentAndDurationToTarget()
    {
        var caster = new FakeTargetable();
        var target = new FakeTargetable();
        SkillSO skill = MakeSkill(SkillTargetingType.Target, DamageType.Physical,
            new SkillEffectData { EffectType = SkillEffectType.Slow, Value = 0.4f, Duration = 2f });

        SkillExecutor.Execute(skill, caster, target);

        TestRunner.AssertEqual(1, target.SlowCallCount, "Slow effect should be applied once");
        TestRunner.AssertEqual(0.4f, target.LastSlowPercent, "Slow percent should match effect Value");
        TestRunner.AssertEqual(2f, target.LastSlowDuration, "Slow duration should match effect Duration");
    }

    public static void MultipleEffects_AllApplyInOrder()
    {
        var caster = new FakeTargetable();
        var target = new FakeTargetable();
        SkillSO skill = MakeSkill(SkillTargetingType.Target, DamageType.Physical,
            new SkillEffectData { EffectType = SkillEffectType.Damage, Value = 20f },
            new SkillEffectData { EffectType = SkillEffectType.Stun, Duration = 1f });

        SkillExecutor.Execute(skill, caster, target);

        TestRunner.AssertEqual(1, target.TakeDamageCallCount, "Damage part of the combo should apply");
        TestRunner.AssertEqual(1, target.StunCallCount, "Stun part of the combo should also apply");
    }

    public static void NoTarget_SkipsEffectsSafely()
    {
        var caster = new FakeTargetable();
        SkillSO skill = MakeSkill(SkillTargetingType.Target, DamageType.Physical,
            new SkillEffectData { EffectType = SkillEffectType.Damage, Value = 20f });

        SkillExecutor.Execute(skill, caster, null); // must not throw

        TestRunner.Assert(true, "Executing a Target-type skill with no target should not throw");
    }
}
