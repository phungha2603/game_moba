using Project.Combat;
using Project.Core;
using Project.Shared;
using Project.Stats;
using UnityEngine;

namespace Project.Character
{
    [RequireComponent(typeof(CharacterStats))]
    public class CharacterBrain : MonoBehaviour
    {
        [SerializeField] private float _attackRange = 2f;

        public CharacterStats Stats { get; private set; }
        public StateMachine StateMachine { get; private set; }
        public AttackClock AttackClock { get; private set; }
        public Vector3 MoveDirection { get; set; }
        public ITargetable Target { get; private set; }
        public float AttackRange => _attackRange;

        public IdleState IdleState { get; private set; }
        public MoveState MoveState { get; private set; }
        public ChaseState ChaseState { get; private set; }
        public AttackState AttackState { get; private set; }
        public DeadState DeadState { get; private set; }

        private void Awake()
        {
            Stats = GetComponent<CharacterStats>();
            StateMachine = new StateMachine();
            AttackClock = new AttackClock();

            IdleState = new IdleState(this);
            MoveState = new MoveState(this);
            ChaseState = new ChaseState(this);
            AttackState = new AttackState(this);
            DeadState = new DeadState(this);
        }

        private void OnEnable()
        {
            if (LogicTicker.Instance == null)
            {
                Debug.LogError("CharacterBrain requires a LogicTicker in the scene.", this);
                return;
            }

            LogicTicker.Instance.OnTick += HandleTick;
            StateMachine.ChangeState(IdleState);
        }

        private void OnDisable()
        {
            if (LogicTicker.Instance != null)
                LogicTicker.Instance.OnTick -= HandleTick;
        }

        public void Move(Vector3 direction, float deltaTime)
        {
            float speed = Stats.GetValue(StatType.MoveSpeed);
            transform.position += direction.normalized * speed * deltaTime;
        }

        /// Called by whatever already found a valid target (player's attack button + a scan,
        /// or an AI brain later) — CharacterBrain itself never touches Physics directly.
        public void EngageTarget(ITargetable target)
        {
            if (target == null) return;
            Target = target;
            StateMachine.ChangeState(ChaseState);
        }

        public void ClearTarget() => Target = null;

        public bool IsTargetInAttackRange()
        {
            if (Target == null || !Target.IsAlive) return false;
            return (Target.Position - transform.position).sqrMagnitude <= _attackRange * _attackRange;
        }

        public void PerformAttack()
        {
            AttackClock.Reset();
            // Phase 3 (Combat & Damage Calculator) applies real damage to Target from here.
        }

        private void HandleTick(float fixedDeltaTime)
        {
            AttackClock.Tick(fixedDeltaTime);
            StateMachine.Tick(fixedDeltaTime);
        }
    }
}
