using Project.Character;
using Project.Lane;
using Project.Shared;
using UnityEngine;

namespace Project.Bootstrap
{
    /// The enemy Hero "Bot" — not a CharacterBrain subclass, just another input source
    /// sitting beside it (like DebugKeyboardInput for the player), calling the exact same
    /// public API a human would: MoveDirection and AutoAttackController.RequestAttack().
    /// Patrols the lane like a Minion, retreats toward its own base at low HP, and leaves
    /// movement alone entirely while CharacterBrain is Chasing/Attacking so it doesn't
    /// immediately self-interrupt its own engagement (MoveDirection != 0 cancels Chase/Attack
    /// by design — see ChaseState/AttackState).
    [RequireComponent(typeof(CharacterBrain))]
    [RequireComponent(typeof(AutoAttackController))]
    public class SimpleHeroAI : MonoBehaviour
    {
        [SerializeField] private float _lowHealthRetreatThreshold = 0.3f;
        [SerializeField] private float _attackCheckInterval = 1f;

        private CharacterBrain _brain;
        private AutoAttackController _autoAttack;
        private LanePath _path;
        private float _timer;

        private void Awake()
        {
            _brain = GetComponent<CharacterBrain>();
            _autoAttack = GetComponent<AutoAttackController>();
        }

        public void SetPath(LanePath path) => _path = path;

        private void Update()
        {
            if (_path == null) return;

            bool isEngaging = _brain.StateMachine.CurrentState == _brain.ChaseState
                || _brain.StateMachine.CurrentState == _brain.AttackState;

            _brain.MoveDirection = isEngaging ? Vector3.zero : ChoosePatrolDirection();

            _timer += Time.deltaTime;
            if (_timer >= _attackCheckInterval)
            {
                _timer = 0f;
                if (!isEngaging) _autoAttack.RequestAttack();
            }
        }

        private Vector3 ChoosePatrolDirection()
        {
            float healthPercent = _brain.Stats.CurrentHealth / _brain.Stats.GetValue(StatType.Health);
            return healthPercent < _lowHealthRetreatThreshold
                ? (_path.Start - transform.position).normalized
                : _path.Direction;
        }
    }
}
