using Project.Character;
using Project.Combat;
using Project.Core;
using Project.Economy;
using Project.Shared;
using Project.Skills;
using Project.Stats;
using UnityEngine;

internal static class OnHitItemTests
{
    private static (CharacterBrain brain, Inventory inventory) Spawn()
    {
        LogicTickerTests.ResetSingleton();

        var tickerGO = new GameObject("Ticker");
        tickerGO.AddComponent<LogicTicker>();

        var baseStats = new BaseStats { baseMoveSpeed = 5f };
        var heroGO = new GameObject("Hero");
        CharacterStats stats = heroGO.AddComponent<CharacterStats>();
        stats.Initialize(baseStats);
        heroGO.AddComponent<Targetable>().Initialize(Team.Blue, TargetType.Hero);
        HeroProgression progression = heroGO.AddComponent<HeroProgression>();
        progression.AddGold(1000f);
        Inventory inventory = heroGO.AddComponent<Inventory>();
        CharacterBrain brain = heroGO.AddComponent<CharacterBrain>();

        return (brain, inventory);
    }

    public static void EquippedOnHitItem_TriggersItsEffectOnPerformAttack()
    {
        (CharacterBrain brain, Inventory inventory) = Spawn();
        var onHitItem = new ItemSO
        {
            ItemName = "Frost Gauntlet",
            Price = 100f,
            HasOnHitEffect = true,
            OnHitEffect = new SkillEffectData { EffectType = SkillEffectType.Slow, Value = 0.3f, Duration = 1f }
        };
        inventory.BuyItem(onHitItem);

        var target = new FakeTargetable { Position = Vector3.zero };
        brain.EngageTarget(target); // sets Target without needing to be in attack range for this direct PerformAttack call

        brain.PerformAttack();

        TestRunner.AssertEqual(1, target.SlowCallCount, "The on-hit Slow effect should trigger once per basic attack");
        TestRunner.AssertEqual(0.3f, target.LastSlowPercent, "The on-hit effect's values should match the item's configured effect");
    }

    public static void NoOnHitItemEquipped_PerformAttackDoesNotTriggerAnyEffect()
    {
        (CharacterBrain brain, Inventory inventory) = Spawn();
        var plainItem = new ItemSO { ItemName = "Boots", Price = 50f };
        inventory.BuyItem(plainItem);

        var target = new FakeTargetable { Position = Vector3.zero };
        brain.EngageTarget(target);

        brain.PerformAttack();

        TestRunner.AssertEqual(0, target.SlowCallCount, "No on-hit item equipped should mean no on-hit effect ever fires");
    }

    public static void KillingBlow_DoesNotAlsoTriggerOnHitEffectOnTheDeadTarget()
    {
        (CharacterBrain brain, Inventory inventory) = Spawn();
        var onHitItem = new ItemSO
        {
            ItemName = "Frost Gauntlet",
            Price = 100f,
            HasOnHitEffect = true,
            OnHitEffect = new SkillEffectData { EffectType = SkillEffectType.Slow, Value = 0.3f, Duration = 1f }
        };
        inventory.BuyItem(onHitItem);

        var target = new FakeTargetable { Position = Vector3.zero, IsAlive = false }; // simulates the hit itself being lethal
        brain.EngageTarget(target);

        brain.PerformAttack();

        TestRunner.AssertEqual(1, target.TakeDamageCallCount, "The basic attack itself should still land");
        TestRunner.AssertEqual(0, target.SlowCallCount, "An on-hit effect must not fire on a target the basic attack just killed");
    }
}
