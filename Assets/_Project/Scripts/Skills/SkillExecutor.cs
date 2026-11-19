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
                switch (effect.EffectType)
                {
                    case SkillEffectType.Damage:
                        ApplyDamage(skillData, effect, caster, effectTarget);
                        break;
                    case SkillEffectType.Heal:
                        effectTarget.Heal(effect.Value);
                        break;
                    case SkillEffectType.Stun:
                        effectTarget.ApplyStun(effect.Duration);
                        break;
                    case SkillEffectType.Slow:
                        effectTarget.ApplySlow(effect.Value, effect.Duration);
                        break;
                }
            }
        }

        private static void ApplyDamage(SkillSO skillData, SkillEffectData effect, ITargetable caster, ITargetable target)
        {
            DamageResult result;
            switch (skillData.DamageType)
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

            target.TakeDamage(result.FinalDamage, result.IsCritical);
        }
    }
}
