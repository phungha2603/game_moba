using System.Collections.Generic;
using System.Linq;
using Project.Combat;
using Project.Economy;
using Project.Shared;
using Project.Skills;
using Project.Stats;
using UnityEngine;

internal static class InventoryTests
{
    private static (Inventory inventory, HeroProgression progression, CharacterStats stats) Spawn(float gold = 1000f)
    {
        var baseStats = new BaseStats();
        var heroGO = new GameObject("Hero");
        CharacterStats stats = heroGO.AddComponent<CharacterStats>();
        stats.Initialize(baseStats);
        heroGO.AddComponent<Targetable>().Initialize(Team.Blue, TargetType.Hero);
        HeroProgression progression = heroGO.AddComponent<HeroProgression>();
        progression.AddGold(gold);
        Inventory inventory = heroGO.AddComponent<Inventory>();

        return (inventory, progression, stats);
    }

    private static ItemSO MakeItem(string name, float price, params ItemStatBonus[] bonuses)
    {
        var item = new ItemSO { ItemName = name, Price = price };
        item.StatBonuses.AddRange(bonuses);
        return item;
    }

    public static void BuyItem_DeductsGoldAndAppliesStatBonus()
    {
        (Inventory inventory, HeroProgression progression, CharacterStats stats) = Spawn(gold: 1000f);
        float adBefore = stats.GetValue(StatType.AttackDamage);
        ItemSO sword = MakeItem("Sword", 300f, new ItemStatBonus { Stat = StatType.AttackDamage, Value = 10f, ModType = ModifierType.Flat });

        bool bought = inventory.BuyItem(sword);

        TestRunner.Assert(bought, "Buying with enough gold should succeed");
        TestRunner.AssertEqual(700f, progression.CurrentGold, "Gold should be reduced by the item's price");
        TestRunner.AssertEqual(adBefore + 10f, stats.GetValue(StatType.AttackDamage), "AttackDamage should increase by the item's bonus");
    }

    public static void BuyItem_FailsWithoutEnoughGold()
    {
        (Inventory inventory, HeroProgression progression, _) = Spawn(gold: 10f);
        ItemSO sword = MakeItem("Sword", 300f);

        bool bought = inventory.BuyItem(sword);

        TestRunner.Assert(!bought, "Buying without enough gold should fail");
        TestRunner.AssertEqual(10f, progression.CurrentGold, "A failed purchase must not spend gold");
    }

    public static void BuyItem_FailsWhenInventoryFull()
    {
        (Inventory inventory, _, _) = Spawn(gold: 100000f);
        for (int i = 0; i < 6; i++)
        {
            TestRunner.Assert(inventory.BuyItem(MakeItem($"Filler{i}", 1f)), $"Slot {i} should still be available");
        }

        bool bought = inventory.BuyItem(MakeItem("Overflow", 1f));

        TestRunner.Assert(!bought, "A 7th item must not fit in a 6-slot inventory");
    }

    public static void SellItem_RefundsPartialGoldAndRemovesStatBonus()
    {
        (Inventory inventory, HeroProgression progression, CharacterStats stats) = Spawn(gold: 1000f);
        float adBefore = stats.GetValue(StatType.AttackDamage);
        ItemSO sword = MakeItem("Sword", 300f, new ItemStatBonus { Stat = StatType.AttackDamage, Value = 10f, ModType = ModifierType.Flat });
        inventory.BuyItem(sword);

        bool sold = inventory.SellItem(sword);

        TestRunner.Assert(sold, "Selling an equipped item should succeed");
        TestRunner.AssertEqual(700f + 210f, progression.CurrentGold, "Selling should refund 70% of the price");
        TestRunner.AssertEqual(adBefore, stats.GetValue(StatType.AttackDamage), "Selling should remove the item's stat bonus");
    }

    public static void BuyingTwoCopies_SellingOne_LeavesTheOtherCopysBonusIntact()
    {
        (Inventory inventory, _, CharacterStats stats) = Spawn(gold: 1000f);
        float adBefore = stats.GetValue(StatType.AttackDamage);
        ItemSO dagger = MakeItem("Dagger", 100f, new ItemStatBonus { Stat = StatType.AttackDamage, Value = 5f, ModType = ModifierType.Flat });

        inventory.BuyItem(dagger);
        inventory.BuyItem(dagger); // same ItemSO asset, bought twice
        TestRunner.AssertEqual(adBefore + 10f, stats.GetValue(StatType.AttackDamage), "Two copies should stack to +10 AD");

        inventory.SellItem(dagger); // sell exactly one copy

        TestRunner.AssertEqual(adBefore + 5f, stats.GetValue(StatType.AttackDamage), "Selling one copy must leave the other copy's +5 AD intact (per-purchase token, not per-asset)");
    }

    public static void GetOnHitItems_ReturnsOnlyItemsFlaggedForOnHit()
    {
        (Inventory inventory, _, _) = Spawn(gold: 1000f);
        ItemSO plain = MakeItem("Plain Boots", 50f);
        var onHit = MakeItem("Frost Gauntlet", 50f);
        onHit.HasOnHitEffect = true;
        onHit.OnHitEffect = new SkillEffectData { EffectType = SkillEffectType.Slow, Value = 0.3f, Duration = 1f };

        inventory.BuyItem(plain);
        inventory.BuyItem(onHit);

        List<ItemSO> onHitItems = inventory.GetOnHitItems().ToList();

        TestRunner.AssertEqual(1, onHitItems.Count, "Only the item flagged HasOnHitEffect should be returned");
        TestRunner.Assert(ReferenceEquals(onHitItems[0], onHit), "The returned item should be the Frost Gauntlet");
    }
}
