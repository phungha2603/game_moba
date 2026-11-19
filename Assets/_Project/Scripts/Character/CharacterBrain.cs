using System.Collections.Generic;
using Project.Combat;
using Project.Core;
using Project.Economy;
using Project.Shared;
using Project.Skills;
using Project.Stats;
using UnityEngine;

namespace Project.Character
{
    [RequireComponent(typeof(CharacterStats))]
    public class CharacterBrain : MonoBehaviour
    {
        [SerializeField] private float _attackRange = 2f;

        private readonly List<SkillInstance> _skills = new List<SkillInstance>();
        private Targetable _targetable;
        private Inventory _inventory;

        public CharacterStats Stats { get; private set; }
        public StatusEffectController StatusEffects { get; private set; }
        public ITargetable Self { get; private set; }
        public StateMachine StateMachine { get; private set; }
        public AttackClock AttackClock { get; private set; }
        public Vector3 MoveDirection { get; set; }
        public ITargetable Target { get; private set; }
        public float AttackRange => _attackRange;

        public IdleState IdleState { get; private set; }
        public MoveState MoveState { get; private set; }
        public ChaseState ChaseState { get; private set; }
        public AttackState AttackState { get; private set; }
        public StunnedState StunnedState { get; private set; }
        public DeadState DeadState { get; private set; }

        private void Awake()
        {
            Stats = GetComponent<CharacterStats>();
            StatusEffects = GetComponent<StatusEffectController>();
            _targetable = GetComponent<Targetable>();
            _inventory = GetComponent<Inventory>();
            Self = GetComponent<ITargetable>();
            StateMachine = new StateMachine();
            AttackClock = new AttackClock();

            IdleState = new IdleState(this);
            MoveState = new MoveState(this);
            ChaseState = new ChaseState(this);
            AttackState = new AttackState(this);
            StunnedState = new StunnedState(this);
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
            _targetable?.SetCurrentAttackTarget(target);
            StateMachine.ChangeState(ChaseState);
        }

        public void ClearTarget()
        {
            Target = null;
            _targetable?.SetCurrentAttackTarget(null);
        }

        public bool IsTargetInAttackRange()
        {
            if (Target == null || !Target.IsAlive) return false;
            return (Target.Position - transform.position).sqrMagnitude <= _attackRange * _attackRange;
        }

        public void PerformAttack()
        {
            AttackClock.Reset();

            if (Target == null) return;

            bool isCritical = DamageCalculator.RollIsCritical(Stats.GetValue(StatType.CritChance), Random.value);
            DamageResult result = DamageCalculator.CalculatePhysicalDamage(
                Stats.GetValue(StatType.AttackDamage),
                isCritical,
                Stats.GetValue(StatType.LifeSteal),
                Stats.GetValue(StatType.ArmorPenetrationFlat),
                Stats.GetValue(StatType.ArmorPenetrationPercent),
                Target.GetStatValue(StatType.Armor));

            Target.TakeDamage(result.FinalDamage, result.IsCritical, Self);
            if (result.LifeStealAmount > 0f) Stats.Heal(result.LifeStealAmount);

            if (Target.IsAlive) ApplyOnHitItems(Target);
        }

        /// No ICD/proc-chance yet — every on-hit item equipped triggers on every basic attack
        /// (skipped by the IsAlive check above if the basic attack itself was the killing blow).
        private void ApplyOnHitItems(ITargetable target)
        {
            if (_inventory == null) return;

            foreach (ItemSO item in _inventory.GetOnHitItems())
            {
                SkillExecutor.ApplyEffect(item.OnHitEffect, DamageType.Physical, Self, target);
            }
        }

        public SkillInstance AddSkill(SkillSO skillData)
        {
            var instance = new SkillInstance(skillData);
            _skills.Add(instance);
            return instance;
        }

        /// Checks range/target/mana before committing — nothing is spent on a failed cast.
        public bool TryCastSkill(SkillInstance skill)
        {
            if (skill == null || !skill.IsReady) return false;

            if (skill.Data.TargetingType == SkillTargetingType.Target)
            {
                if (Target == null || !Target.IsAlive) return false;

                float rangeSqr = skill.Data.CastRange * skill.Data.CastRange;
                if ((Target.Position - transform.position).sqrMagnitude > rangeSqr) return false;
            }

            if (!Stats.SpendMana(skill.Data.ManaCost)) return false;

            skill.StartCooldown();
            SkillExecutor.Execute(skill.Data, Self, Target);
            return true;
        }

        private void HandleTick(float fixedDeltaTime)
        {
            AttackClock.AttacksPerSecond = Stats.GetValue(StatType.AttackSpeed);
            AttackClock.Tick(fixedDeltaTime);

            // Check before ticking the controller down, so a stun still active at the start
            // of this tick is honored this tick (StunnedState.OnUpdate re-checks afterwards
            // and exits on its own once the controller reports it has expired).
            if (StatusEffects != null && StatusEffects.IsStunned && StateMachine.CurrentState != StunnedState)
            {
                StateMachine.ChangeState(StunnedState);
            }

            foreach (SkillInstance skill in _skills) skill.Tick(fixedDeltaTime);
            StatusEffects?.Tick(fixedDeltaTime);

            StateMachine.Tick(fixedDeltaTime);
        }
    }
}
