using Project.Shared;
using Project.Stats;

internal static class StatModifierTests
{
    public static void FlatAndPercentStack_ComputesCorrectFinalValue()
    {
        var stat = new RuntimeStat(StatType.AttackDamage, 50f);
        stat.AddModifier(new StatModifier(10f, ModifierType.Flat, "itemA"));
        stat.AddModifier(new StatModifier(0.2f, ModifierType.Percent, "buffA"));

        // Final = (50 + 10) * (1 + 0.2) = 72
        TestRunner.AssertEqual(72f, stat.CurrentValue, "Flat+Percent stacking formula");
    }

    public static void DirtyFlag_RecalculatesOnlyWhenModifiersChange()
    {
        var stat = new RuntimeStat(StatType.Health, 100f);
        TestRunner.AssertEqual(100f, stat.CurrentValue, "Base value before any modifier");

        stat.AddModifier(new StatModifier(50f, ModifierType.Flat, "potion"));
        TestRunner.AssertEqual(150f, stat.CurrentValue, "Value after adding flat modifier");

        stat.RemoveModifiersFromSource("potion");
        TestRunner.AssertEqual(100f, stat.CurrentValue, "Value after removing modifier");
    }

    public static void SetBaseValue_InvalidatesCacheEvenAfterCurrentValueWasAlreadyRead()
    {
        var stat = new RuntimeStat(StatType.Health, 100f);
        TestRunner.AssertEqual(100f, stat.CurrentValue, "Sanity check: read once to populate the cache");

        stat.SetBaseValue(250f);

        TestRunner.AssertEqual(250f, stat.CurrentValue, "Changing BaseValue after a read must invalidate the cached CurrentValue");
    }
}
