using System;
using System.Collections.Generic;
using Project.Shared;
using UnityEngine;

namespace Project.Stats
{
    public class CharacterStats : MonoBehaviour
    {
        [SerializeField] private BaseStats _baseStats;

        private readonly Dictionary<StatType, RuntimeStat> _stats = new Dictionary<StatType, RuntimeStat>();

        /// Current HP, independent from the Health RuntimeStat (which is the max/cap). Damage
        /// and healing only ever change this, never the stat itself.
        public float CurrentHealth { get; private set; }
        public bool IsAlive => CurrentHealth > 0f;

        public event Action<float, float> OnHealthChanged; // (current, max)

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

        public void ApplyDamage(float amount)
        {
            float max = GetValue(StatType.Health);
            CurrentHealth = Clamp(CurrentHealth - amount, 0f, max);
            OnHealthChanged?.Invoke(CurrentHealth, max);
        }

        public void Heal(float amount)
        {
            float max = GetValue(StatType.Health);
            CurrentHealth = Clamp(CurrentHealth + amount, 0f, max);
            OnHealthChanged?.Invoke(CurrentHealth, max);
        }

        private static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            return value > max ? max : value;
        }

        private void BuildFromBaseStats()
        {
            _stats[StatType.Health] = new RuntimeStat(StatType.Health, _baseStats.baseHealth);
            _stats[StatType.Mana] = new RuntimeStat(StatType.Mana, _baseStats.baseMana);
            _stats[StatType.AttackDamage] = new RuntimeStat(StatType.AttackDamage, _baseStats.baseAttackDamage);
            _stats[StatType.AbilityPower] = new RuntimeStat(StatType.AbilityPower, _baseStats.baseAbilityPower);
            _stats[StatType.AttackSpeed] = new RuntimeStat(StatType.AttackSpeed, _baseStats.baseAttackSpeed);
            _stats[StatType.CritChance] = new RuntimeStat(StatType.CritChance, _baseStats.baseCritChance);
            _stats[StatType.LifeSteal] = new RuntimeStat(StatType.LifeSteal, _baseStats.baseLifeSteal);
            _stats[StatType.Armor] = new RuntimeStat(StatType.Armor, _baseStats.baseArmor);
            _stats[StatType.MagicResist] = new RuntimeStat(StatType.MagicResist, _baseStats.baseMagicResist);
            _stats[StatType.ArmorPenetrationFlat] = new RuntimeStat(StatType.ArmorPenetrationFlat, _baseStats.baseArmorPenetrationFlat);
            _stats[StatType.ArmorPenetrationPercent] = new RuntimeStat(StatType.ArmorPenetrationPercent, _baseStats.baseArmorPenetrationPercent);
            _stats[StatType.MoveSpeed] = new RuntimeStat(StatType.MoveSpeed, _baseStats.baseMoveSpeed);
            _stats[StatType.CooldownReduction] = new RuntimeStat(StatType.CooldownReduction, _baseStats.baseCooldownReduction);

            CurrentHealth = GetValue(StatType.Health);
        }
    }
}
