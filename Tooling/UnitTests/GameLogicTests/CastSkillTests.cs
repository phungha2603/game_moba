using Project.Character;
using Project.Combat;
using Project.Core;
using Project.Shared;
using Project.Skills;
using Project.Stats;
using UnityEngine;

internal static class CastSkillTests
{
    private static (CharacterBrain brain, LogicTicker ticker) Spawn(float mana)
    {
        LogicTickerTests.ResetSingleton();

        var tickerGO = new GameObject("Ticker");
        LogicTicker ticker = tickerGO.AddComponent<LogicTicker>();

        var baseStats = new BaseStats { baseMana = mana, baseMoveSpeed = 5f };
        var heroGO = new GameObject("Hero");
        CharacterStats stats = heroGO.AddComponent<CharacterStats>();
        stats.Initialize(baseStats);
        heroGO.AddComponent<StatusEffectController>();
        Targetable targetable = heroGO.AddComponent<Targetable>();
        targetable.Initialize(Team.Blue, TargetType.Hero);
        CharacterBrain brain = heroGO.AddComponent<CharacterBrain>();

        return (brain, ticker);
    }

    private static SkillSO MakeSelfHealSkill(float manaCost, float cooldown, float healAmount)
    {
        var skill = new SkillSO { ManaCost = manaCost, Cooldown = cooldown, TargetingType = SkillTargetingType.Self };
        skill.Effects.Add(new SkillEffectData { EffectType = SkillEffectType.Heal, Value = healAmount });
        return skill;
    }

    public static void TryCastSkill_SelfHeal_SpendsManaAndHealsCaster()
    {
        (CharacterBrain brain, _) = Spawn(mana: 100f);
        SkillInstance skill = brain.AddSkill(MakeSelfHealSkill(manaCost: 40f, cooldown: 5f, healAmount: 50f));
        brain.Stats.ApplyDamage(80f); // give the heal somewhere to go

        bool cast = brain.TryCastSkill(skill);

        TestRunner.Assert(cast, "Casting with enough mana should succeed");
        TestRunner.AssertEqual(60f, brain.Stats.CurrentMana, "Mana should be reduced by ManaCost");
        TestRunner.Assert(brain.Stats.CurrentHealth > 20f, "Self-heal should have raised the caster's HP above the pre-heal value");
    }

    public static void TryCastSkill_NotEnoughMana_FailsWithoutSpendingOrStartingCooldown()
    {
        (CharacterBrain brain, _) = Spawn(mana: 10f);
        SkillInstance skill = brain.AddSkill(MakeSelfHealSkill(manaCost: 40f, cooldown: 5f, healAmount: 50f));

        bool cast = brain.TryCastSkill(skill);

        TestRunner.Assert(!cast, "Casting without enough mana should fail");
        TestRunner.AssertEqual(10f, brain.Stats.CurrentMana, "Failed cast must not spend mana");
        TestRunner.Assert(skill.IsReady, "Failed cast must not start the cooldown");
    }

    public static void TryCastSkill_WhileOnCooldown_Fails()
    {
        (CharacterBrain brain, _) = Spawn(mana: 100f);
        SkillInstance skill = brain.AddSkill(MakeSelfHealSkill(manaCost: 10f, cooldown: 5f, healAmount: 10f));

        TestRunner.Assert(brain.TryCastSkill(skill), "First cast should succeed");
        TestRunner.Assert(!brain.TryCastSkill(skill), "Second cast immediately after should fail (on cooldown)");
    }

    public static void TryCastSkill_TargetedSkill_FailsWhenTargetOutOfRange()
    {
        (CharacterBrain brain, _) = Spawn(mana: 100f);
        var skillData = new SkillSO { ManaCost = 10f, Cooldown = 1f, TargetingType = SkillTargetingType.Target, CastRange = 3f };
        skillData.Effects.Add(new SkillEffectData { EffectType = SkillEffectType.Damage, Value = 10f });
        SkillInstance skill = brain.AddSkill(skillData);

        var farTarget = new FakeTargetable { Position = new Vector3(10f, 0f, 0f), Team = Team.Red };
        brain.EngageTarget(farTarget);

        bool cast = brain.TryCastSkill(skill);

        TestRunner.Assert(!cast, "Casting a Target-type skill should fail when the target is out of CastRange");
    }

    public static void TryCastSkill_Skillshot_SpendsManaAndRaisesSkillshotEventInsteadOfApplyingEffectsDirectly()
    {
        (CharacterBrain brain, _) = Spawn(mana: 100f);
        var skillData = new SkillSO { ManaCost = 30f, Cooldown = 1f, TargetingType = SkillTargetingType.Skillshot };
        skillData.Effects.Add(new SkillEffectData { EffectType = SkillEffectType.Damage, Value = 10f });
        SkillInstance skill = brain.AddSkill(skillData);

        SkillSO raisedSkill = null;
        Vector3 raisedDirection = default;
        brain.OnSkillshotRequested += (s, d) => { raisedSkill = s; raisedDirection = d; };

        bool cast = brain.TryCastSkill(skill, new Vector3(1f, 0f, 0f));

        TestRunner.Assert(cast, "Casting a Skillshot with a valid direction and enough mana should succeed");
        TestRunner.AssertEqual(70f, brain.Stats.CurrentMana, "Mana should still be spent for a Skillshot cast");
        TestRunner.Assert(ReferenceEquals(raisedSkill, skillData), "OnSkillshotRequested should carry the skill's data");
        TestRunner.AssertEqual(1f, raisedDirection.x, "OnSkillshotRequested should carry the aim direction passed in");
    }

    public static void TryCastSkill_Skillshot_FailsWithoutAimDirection()
    {
        (CharacterBrain brain, _) = Spawn(mana: 100f);
        var skillData = new SkillSO { ManaCost = 30f, Cooldown = 1f, TargetingType = SkillTargetingType.Skillshot };
        SkillInstance skill = brain.AddSkill(skillData);

        bool cast = brain.TryCastSkill(skill, Vector3.zero);

        TestRunner.Assert(!cast, "A Skillshot cast with no aim direction must fail");
        TestRunner.AssertEqual(100f, brain.Stats.CurrentMana, "A failed Skillshot cast must not spend mana");
    }
}
