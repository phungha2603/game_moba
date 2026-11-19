using System.Collections.Generic;
using Project.Shared;
using UnityEngine;

namespace Project.Stats
{
    public class CharacterStats : MonoBehaviour
    {
        [SerializeField] private BaseStats _baseStats;

        private readonly Dictionary<StatType, RuntimeStat> _stats = new Dictionary<StatType, RuntimeStat>();

        private void Awake()
        {
            if (_baseStats != null) BuildFromBaseStats();
        }

        /// Lets code-driven setup (e.g. GlobalLoader) assign stats before the Inspector would.
        public void Initialize(BaseStats baseStats)
        {
            _baseStats = baseStats;
            BuildFromBaseStats();
        }

        public RuntimeStat GetStat(StatType type)
        {
            if (!_stats.TryGetValue(type, out RuntimeStat stat))
            {
                throw new System.InvalidOperationException(
                    $"CharacterStats has no RuntimeStat for {type}. Did BuildFromBaseStats() forget to add it?");
            }

            return stat;
        }

        public float GetValue(StatType type) => GetStat(type).CurrentValue;

        private void BuildFromBaseStats()
        {
            _stats[StatType.Health] = new RuntimeStat(StatType.Health, _baseStats.baseHealth);
            _stats[StatType.Mana] = new RuntimeStat(StatType.Mana, _baseStats.baseMana);
            _stats[StatType.AttackDamage] = new RuntimeStat(StatType.AttackDamage, _baseStats.baseAttackDamage);
            _stats[StatType.AbilityPower] = new RuntimeStat(StatType.AbilityPower, _baseStats.baseAbilityPower);
            _stats[StatType.AttackSpeed] = new RuntimeStat(StatType.AttackSpeed, _baseStats.baseAttackSpeed);
            _stats[StatType.Armor] = new RuntimeStat(StatType.Armor, _baseStats.baseArmor);
            _stats[StatType.MagicResist] = new RuntimeStat(StatType.MagicResist, _baseStats.baseMagicResist);
            _stats[StatType.MoveSpeed] = new RuntimeStat(StatType.MoveSpeed, _baseStats.baseMoveSpeed);
            _stats[StatType.CooldownReduction] = new RuntimeStat(StatType.CooldownReduction, _baseStats.baseCooldownReduction);
        }
    }
}
