using Project.Shared;
using Project.Stats;
using UnityEngine;

namespace Project.Combat
{
    [RequireComponent(typeof(CharacterStats))]
    public class Targetable : MonoBehaviour, ITargetable
    {
        [SerializeField] private Team _team;
        [SerializeField] private TargetType _type;

        private CharacterStats _stats;
        private StatusEffectController _statusEffects;
        private bool _isTargetableOverride = true;
        private ITargetable _currentAttackTarget;

        public Vector3 Position => transform.position;
        public Team Team => _team;
        public TargetType Type => _type;
        public bool IsAlive => _isTargetableOverride && _stats.IsAlive;
        public ITargetable CurrentAttackTarget => _currentAttackTarget;

        private void Awake()
        {
            _stats = GetComponent<CharacterStats>();
            _statusEffects = GetComponent<StatusEffectController>();
        }

        public void Initialize(Team team, TargetType type)
        {
            _team = team;
            _type = type;
        }

        /// Lets a skill (e.g. untargetability) hide this unit from TargetSelector without touching its CharacterBrain.
        public void SetTargetableOverride(bool value) => _isTargetableOverride = value;

        public float GetStatValue(StatType statType) => _stats.GetValue(statType);

        public void TakeDamage(float amount, bool isCritical) => _stats.ApplyDamage(amount);

        public void Heal(float amount) => _stats.Heal(amount);

        public void ApplyStun(float duration) => _statusEffects?.ApplyStun(duration);

        public void ApplySlow(float percent, float duration) => _statusEffects?.ApplySlow(percent, duration);

        /// Called by whatever brain owns this unit (CharacterBrain, MinionBrain, TowerBrain)
        /// whenever its own attack target changes — see ITargetable.CurrentAttackTarget.
        public void SetCurrentAttackTarget(ITargetable target) => _currentAttackTarget = target;
    }
}
