using System;
using Project.Combat;
using Project.Core;
using Project.Shared;
using Project.Stats;
using UnityEngine;

namespace Project.Economy
{
    /// Gold/EXP/Level live here, not on CharacterStats — Minion/Tower share CharacterStats
    /// but have no progression. Listens to its own Targetable's OnKillScored (raised when
    /// THIS unit lands a killing blow) rather than Project.Combat ever depending on Economy.
    [RequireComponent(typeof(CharacterStats))]
    [RequireComponent(typeof(Targetable))]
    public class HeroProgression : MonoBehaviour
    {
        private const float PassiveGoldPerSecond = 2f;
        private const int MaxLevel = 18;

        private CharacterStats _stats;
        private Targetable _targetable;

        public int Level { get; private set; } = 1;
        public float CurrentExp { get; private set; }
        public float CurrentGold { get; private set; }

        public event Action<float> OnGoldChanged;
        public event Action<int> OnLevelChanged;

        public static float ExpToReachNextLevel(int level) => 100f * level;

        private void Awake()
        {
            _stats = GetComponent<CharacterStats>();
            _targetable = GetComponent<Targetable>();
        }

        private void OnEnable()
        {
            if (LogicTicker.Instance != null) LogicTicker.Instance.OnTick += HandleTick;
            _targetable.OnKillScored += HandleKillScored;
        }

        private void OnDisable()
        {
            if (LogicTicker.Instance != null) LogicTicker.Instance.OnTick -= HandleTick;
            _targetable.OnKillScored -= HandleKillScored;
        }

        public void AddGold(float amount)
        {
            if (amount <= 0f) return;
            CurrentGold += amount;
            OnGoldChanged?.Invoke(CurrentGold);
        }

        public bool SpendGold(float amount)
        {
            if (CurrentGold < amount) return false;
            CurrentGold -= amount;
            OnGoldChanged?.Invoke(CurrentGold);
            return true;
        }

        public void AddExp(float amount)
        {
            if (amount <= 0f || Level >= MaxLevel) return;

            CurrentExp += amount;
            while (Level < MaxLevel && CurrentExp >= ExpToReachNextLevel(Level))
            {
                CurrentExp -= ExpToReachNextLevel(Level);
                Level++;
                _stats.ApplyLevel(Level);
                OnLevelChanged?.Invoke(Level);
            }
        }

        private void HandleKillScored(ITargetable victim)
        {
            AddGold(victim.GoldReward);
            AddExp(victim.ExpReward);
        }

        private void HandleTick(float fixedDeltaTime)
        {
            AddGold(PassiveGoldPerSecond * fixedDeltaTime);
        }
    }
}
