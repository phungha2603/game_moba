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

        /// Magic damage never crits/lifesteals by default (standard MOBA convention — only
        /// basic attacks do unless a specific item/skill says otherwise).
        public static DamageResult CalculateMagicDamage(
            float abilityDamage,
            float magicPenFlat,
            float magicPenPercent,
            float targetMagicResist)
        {
            float effectiveResist = targetMagicResist * (1f - magicPenPercent) - magicPenFlat;
            if (effectiveResist < 0f) effectiveResist = 0f;

            float mitigation = 100f / (100f + effectiveResist);
            float finalDamage = abilityDamage * mitigation;

            return new DamageResult(finalDamage, false, 0f);
        }

        /// True damage: ignores armor/magic resist entirely.
        public static DamageResult CalculatePureDamage(float amount) => new DamageResult(amount, false, 0f);
    }
}
