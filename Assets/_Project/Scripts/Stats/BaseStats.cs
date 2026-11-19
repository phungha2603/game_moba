using UnityEngine;

namespace Project.Stats
{
    [CreateAssetMenu(menuName = "Project/Stats/Base Stats", fileName = "NewBaseStats")]
    public class BaseStats : ScriptableObject
    {
        [Header("Resources")]
        public float baseHealth = 600f;
        public float baseMana = 300f;

        [Header("Offense")]
        public float baseAttackDamage = 50f;
        public float baseAbilityPower = 0f;
        public float baseAttackSpeed = 1f;
        public float baseCritChance = 0f;
        public float baseLifeSteal = 0f;

        [Header("Defense")]
        public float baseArmor = 20f;
        public float baseMagicResist = 20f;
        public float baseArmorPenetrationFlat = 0f;
        public float baseArmorPenetrationPercent = 0f;

        [Header("Utility")]
        public float baseMoveSpeed = 3.6f;
        public float baseCooldownReduction = 0f;
    }
}
