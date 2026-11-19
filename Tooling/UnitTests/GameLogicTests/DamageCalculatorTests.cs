using Project.Combat;

internal static class DamageCalculatorTests
{
    public static void NoArmor_DealsFullRawDamage()
    {
        DamageResult result = DamageCalculator.CalculatePhysicalDamage(
            attackDamage: 50f, isCritical: false, lifeStealPercent: 0f,
            armorPenFlat: 0f, armorPenPercent: 0f, targetArmor: 0f);

        TestRunner.AssertEqual(50f, result.FinalDamage, "Zero armor should mitigate nothing");
    }

    public static void PositiveArmor_MitigatesUsingLeagueStyleFormula()
    {
        // mitigation = 100/(100+100) = 0.5 -> 50 * 0.5 = 25
        DamageResult result = DamageCalculator.CalculatePhysicalDamage(
            attackDamage: 50f, isCritical: false, lifeStealPercent: 0f,
            armorPenFlat: 0f, armorPenPercent: 0f, targetArmor: 100f);

        TestRunner.AssertEqual(25f, result.FinalDamage, "100 armor should halve damage under the 100/(100+armor) curve");
    }

    public static void CriticalHit_DoublesRawDamageBeforeMitigation()
    {
        DamageResult result = DamageCalculator.CalculatePhysicalDamage(
            attackDamage: 50f, isCritical: true, lifeStealPercent: 0f,
            armorPenFlat: 0f, armorPenPercent: 0f, targetArmor: 0f);

        TestRunner.AssertEqual(100f, result.FinalDamage, "Critical hit should apply the 2x multiplier");
        TestRunner.Assert(result.IsCritical, "Result should report IsCritical true");
    }

    public static void ArmorPenetrationPercent_AppliedBeforeFlat()
    {
        // armor=100, 50% pen -> 50, then -20 flat -> effective 30. mitigation=100/130.
        DamageResult result = DamageCalculator.CalculatePhysicalDamage(
            attackDamage: 130f, isCritical: false, lifeStealPercent: 0f,
            armorPenFlat: 20f, armorPenPercent: 0.5f, targetArmor: 100f);

        float expected = 130f * (100f / 130f);
        TestRunner.Assert(System.Math.Abs(result.FinalDamage - expected) < 0.01f,
            $"Percent pen should apply before flat pen (expected {expected}, got {result.FinalDamage})");
    }

    public static void ArmorPenetration_NeverMakesEffectiveArmorNegative()
    {
        DamageResult result = DamageCalculator.CalculatePhysicalDamage(
            attackDamage: 50f, isCritical: false, lifeStealPercent: 0f,
            armorPenFlat: 999f, armorPenPercent: 0.9f, targetArmor: 10f);

        TestRunner.AssertEqual(50f, result.FinalDamage, "Over-penetrating should clamp effective armor to 0, not go negative");
    }

    public static void LifeSteal_IsPercentOfFinalDamageAfterMitigation()
    {
        DamageResult result = DamageCalculator.CalculatePhysicalDamage(
            attackDamage: 50f, isCritical: false, lifeStealPercent: 0.5f,
            armorPenFlat: 0f, armorPenPercent: 0f, targetArmor: 100f);

        // finalDamage = 25 (see PositiveArmor test), lifesteal 50% of that = 12.5
        TestRunner.AssertEqual(12.5f, result.LifeStealAmount, "Lifesteal must be computed on post-mitigation damage");
    }

    public static void RollIsCritical_ComparesRandomValueAgainstChance()
    {
        TestRunner.Assert(DamageCalculator.RollIsCritical(0.5f, 0.2f), "0.2 < 0.5 chance should crit");
        TestRunner.Assert(!DamageCalculator.RollIsCritical(0.5f, 0.8f), "0.8 >= 0.5 chance should not crit");
    }

    public static void MagicDamage_NoResist_DealsFullDamage()
    {
        DamageResult result = DamageCalculator.CalculateMagicDamage(50f, 0f, 0f, 0f);
        TestRunner.AssertEqual(50f, result.FinalDamage, "Zero magic resist should mitigate nothing");
    }

    public static void MagicDamage_NeverCrits()
    {
        DamageResult result = DamageCalculator.CalculateMagicDamage(50f, 0f, 0f, 0f);
        TestRunner.Assert(!result.IsCritical, "Magic damage should never report a crit, per genre convention");
    }

    public static void MagicDamage_MitigatesByMagicResist_SameCurveAsArmor()
    {
        DamageResult result = DamageCalculator.CalculateMagicDamage(50f, 0f, 0f, 100f);
        TestRunner.AssertEqual(25f, result.FinalDamage, "100 magic resist should halve damage, same 100/(100+x) curve as armor");
    }
}
