using System;
using Project.Combat;
using Project.Core;
using Project.Shared;
using Project.Stats;
using UnityEngine;

namespace Project.Lane
{
    /// Deliberately its own class rather than sharing a base with CharacterBrain: Hero's
    /// brain carries player-input and skill-casting concerns a minion has no use for, and
    /// this is only the second use case for Chase/Attack-style logic (Tower will be the
    /// third) — worth revisiting a shared base once that lands, not before (rule of three).
    [RequireComponent(typeof(CharacterStats))]
    [RequireComponent(typeof(Targetable))]
    public class MinionBrain : MonoBehaviour
    {
        [SerializeField] private float _attackRange = 1.5f;
        [SerializeField] private float _aggroRange = 4f;

        private Targetable _targetable;

        public CharacterStats Stats { get; private set; }
        public StateMachine StateMachine { get; private set; }
        public AttackClock AttackClock { get; private set; }
        public ITargetable Target { get; private set; }
        public LanePath Path { get; private set; }
        public float AttackRange => _attackRange;
        public float AggroRange => _aggroRange;

        public MinionMoveState MoveState { get; private set; }
        public MinionChaseState ChaseState { get; private set; }
        public MinionAttackState AttackState { get; private set; }
        public MinionDeadState DeadState { get; private set; }

        /// For visual feedback only (CharacterView) — fires on every basic attack landed.
        public event Action OnAttackPerformed;

        private void Awake()
        {
            Stats = GetComponent<CharacterStats>();
            _targetable = GetComponent<Targetable>();
            StateMachine = new StateMachine();
            AttackClock = new AttackClock();

            MoveState = new MinionMoveState(this);
            ChaseState = new MinionChaseState(this);
            AttackState = new MinionAttackState(this);
            DeadState = new MinionDeadState(this);
        }

        private void OnEnable()
        {
            if (LogicTicker.Instance == null)
            {
                Debug.LogError("MinionBrain requires a LogicTicker in the scene.", this);
                return;
            }

            LogicTicker.Instance.OnTick += HandleTick;
            Stats.OnHealthChanged += HandleHealthChanged;
            StateMachine.ChangeState(MoveState);
        }

        private void OnDisable()
        {
            if (LogicTicker.Instance != null) LogicTicker.Instance.OnTick -= HandleTick;
            Stats.OnHealthChanged -= HandleHealthChanged;
        }

        public void SetPath(LanePath path) => Path = path;

        public void Move(Vector3 direction, float deltaTime)
        {
            float speed = Stats.GetValue(StatType.MoveSpeed);
            transform.position += direction.normalized * speed * deltaTime;
        }

        /// Called by whatever already found a valid target (MinionAggroController's scan) —
        /// MinionBrain itself never touches Physics directly.
        public void EngageTarget(ITargetable target)
        {
            if (target == null) return;
            Target = target;
            _targetable.SetCurrentAttackTarget(target);
            StateMachine.ChangeState(ChaseState);
        }

        public void ClearTarget()
        {
            Target = null;
            _targetable.SetCurrentAttackTarget(null);
        }

        public bool IsTargetInAttackRange()
        {
            if (Target == null || !Target.IsAlive) return false;
            return (Target.Position - transform.position).sqrMagnitude <= _attackRange * _attackRange;
        }

        public bool IsTargetBeyondAggroRange()
        {
            if (Target == null) return true;
            return (Target.Position - transform.position).sqrMagnitude > _aggroRange * _aggroRange;
        }

        public void PerformAttack()
        {
            AttackClock.Reset();
            if (Target == null) return;

            DamageResult result = DamageCalculator.CalculatePhysicalDamage(
                Stats.GetValue(StatType.AttackDamage), false, 0f, 0f, 0f,
                Target.GetStatValue(StatType.Armor));

            Target.TakeDamage(result.FinalDamage, false, _targetable);
            OnAttackPerformed?.Invoke();
        }

        private void HandleHealthChanged(float current, float max)
        {
            if (current <= 0f && StateMachine.CurrentState != DeadState)
            {
                StateMachine.ChangeState(DeadState);
            }
        }

        private void HandleTick(float fixedDeltaTime)
        {
            AttackClock.AttacksPerSecond = Stats.GetValue(StatType.AttackSpeed);
            AttackClock.Tick(fixedDeltaTime);
            StateMachine.Tick(fixedDeltaTime);
        }
    }
}
