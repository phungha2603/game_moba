using Project.Combat;
using Project.Shared;

namespace Project.Skills
{
    /// Takes caster/target purely as ITargetable so this module never has to depend on
    /// CharacterBrain/CharacterStats directly (Project.Character depends on Project.Skills,
    /// never the reverse).
    public static class SkillExecutor
    {
        public static void Execute(SkillSO skillData, ITargetable caster, ITargetable target)
        {
            ITargetable effectTarget = skillData.TargetingType == SkillTargetingType.Self ? caster : target;
            if (effectTarget == null) return;

            foreach (SkillEffectData effect in skillData.Effects)
            {
                ApplyEffect(effect, skillData.DamageType, caster, effectTarget);
            }
        }

        /// Applies one effect directly — used by SkillSO's effect list above, and reused as-is
        /// by item on-hit passives (Project.Economy), so neither has to duplicate this switch.
        public static void ApplyEffect(SkillEffectData effect, DamageType damageType, ITargetable caster, ITargetable target)
        {
            if (target == null) return;

            switch (effect.EffectType)
            {
                case SkillEffectType.Damage:
                    ApplyDamage(effect, damageType, caster, target);
                    break;
                case SkillEffectType.Heal:
                    target.Heal(effect.Value);
                    break;
                case SkillEffectType.Stun:
                    target.ApplyStun(effect.Duration);
                    break;
                case SkillEffectType.Slow:
                    target.ApplySlow(effect.Value, effect.Duration);
                    break;
            }
        }

        private static void ApplyDamage(SkillEffectData effect, DamageType damageType, ITargetable caster, ITargetable target)
        {
            DamageResult result;
            switch (damageType)
            {
                case DamageType.Magic:
                    result = DamageCalculator.CalculateMagicDamage(
                        effect.Value,
                        caster.GetStatValue(StatType.MagicPenetrationFlat),
                        caster.GetStatValue(StatType.MagicPenetrationPercent),
                        target.GetStatValue(StatType.MagicResist));
                    break;
                case DamageType.Pure:
                    result = DamageCalculator.CalculatePureDamage(effect.Value);
                    break;
                default:
                    result = DamageCalculator.CalculatePhysicalDamage(
                        effect.Value, false, 0f,
                        caster.GetStatValue(StatType.ArmorPenetrationFlat),
                        caster.GetStatValue(StatType.ArmorPenetrationPercent),
                        target.GetStatValue(StatType.Armor));
                    break;
            }

            target.TakeDamage(result.FinalDamage, result.IsCritical, caster);
        }
    }
}
