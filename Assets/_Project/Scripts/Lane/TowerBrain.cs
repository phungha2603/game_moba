using System;
using Project.Combat;
using Project.Core;
using Project.Shared;
using Project.Stats;
using UnityEngine;

namespace Project.Lane
{
    /// No State Machine: a tower never moves or chases, so "pick a target, attack if ready"
    /// collapses into one simple loop — a Chase/Attack FSM would add states that never
    /// actually differ in behavior. Physics-touching (TargetScanner), so — like
    /// MinionAggroController/AutoAttackController — this stays untested; TowerTargetSelector
    /// carries the tested priority logic.
    [RequireComponent(typeof(CharacterStats))]
    [RequireComponent(typeof(Targetable))]
    public class TowerBrain : MonoBehaviour
    {
        [SerializeField] private float _range = 6f;

        private CharacterStats _stats;
        private Targetable _targetable;
        private readonly AttackClock _attackClock = new AttackClock();
        private ITargetable _currentTarget;

        /// For visual feedback only (CharacterView) — fires on every basic attack landed.
        public event Action OnAttackPerformed;

        private void Awake()
        {
            _stats = GetComponent<CharacterStats>();
            _targetable = GetComponent<Targetable>();
        }

        private void OnEnable()
        {
            if (LogicTicker.Instance != null) LogicTicker.Instance.OnTick += HandleTick;
        }

        private void OnDisable()
        {
            if (LogicTicker.Instance != null) LogicTicker.Instance.OnTick -= HandleTick;
        }

        private void HandleTick(float fixedDeltaTime)
        {
            _attackClock.AttacksPerSecond = _stats.GetValue(StatType.AttackSpeed);
            _attackClock.Tick(fixedDeltaTime);

            var candidates = TargetScanner.FindInRadius(transform.position, _range);
            _currentTarget = TowerTargetSelector.SelectTarget(transform.position, _targetable.Team, _range, candidates);
            _targetable.SetCurrentAttackTarget(_currentTarget);

            if (_currentTarget != null && _attackClock.CanAttack)
            {
                PerformAttack();
            }
        }

        private void PerformAttack()
        {
            _attackClock.Reset();

            DamageResult result = DamageCalculator.CalculatePhysicalDamage(
                _stats.GetValue(StatType.AttackDamage), false, 0f, 0f, 0f,
                _currentTarget.GetStatValue(StatType.Armor));

            _currentTarget.TakeDamage(result.FinalDamage, false, _targetable);
            OnAttackPerformed?.Invoke();
        }
    }
}
