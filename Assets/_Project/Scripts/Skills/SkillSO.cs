using System.Collections.Generic;
using Project.Combat;
using UnityEngine;

namespace Project.Skills
{
    [CreateAssetMenu(menuName = "Project/Skills/Skill", fileName = "NewSkill")]
    public class SkillSO : ScriptableObject
    {
        public string SkillName = "New Skill";
        public float ManaCost = 50f;
        public float Cooldown = 5f;
        public float CastRange = 5f;
        public SkillTargetingType TargetingType = SkillTargetingType.Target;

        /// Only relevant when TargetingType == Skillshot.
        public float ProjectileSpeed = 10f;
        public float ProjectileRange = 8f;

        /// Only relevant for Damage effects; Magic/Pure deliberately never crit (see DamageCalculator).
        public DamageType DamageType = DamageType.Physical;

        public List<SkillEffectData> Effects = new List<SkillEffectData>();
    }
}
