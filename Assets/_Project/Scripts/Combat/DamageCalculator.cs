namespace Project.Combat
{
    public static class DamageCalculator
    {
        public const float CriticalDamageMultiplier = 2f;

        public static bool RollIsCritical(float critChance, float randomValue01) => randomValue01 < critChance;

        /// Armor penetration order: percent first, then flat (standard modern-MOBA convention).
        /// Mitigation uses the classic League-style 100/(100+armor) curve, which never reaches
        /// 0 (or negative) damage even against very high armor.
        public static DamageResult CalculatePhysicalDamage(
            float attackDamage,
            bool isCritical,
            float lifeStealPercent,
            float armorPenFlat,
            float armorPenPercent,
            float targetArmor)
        {
            float rawDamage = isCritical ? attackDamage * CriticalDamageMultiplier : attackDamage;

            float effectiveArmor = targetArmor * (1f - armorPenPercent) - armorPenFlat;
            if (effectiveArmor < 0f) effectiveArmor = 0f;

            float mitigation = 100f / (100f + effectiveArmor);
            float finalDamage = rawDamage * mitigation;
            float lifeSteal = finalDamage * lifeStealPercent;

            return new DamageResult(finalDamage, isCritical, lifeSteal);
        }

        /// True damage: ignores armor/magic resist entirely. No caller yet (reserved for
        /// future skills/items) but trivial enough to not be worth withholding.
        public static DamageResult CalculatePureDamage(float amount) => new DamageResult(amount, false, 0f);
    }
}
