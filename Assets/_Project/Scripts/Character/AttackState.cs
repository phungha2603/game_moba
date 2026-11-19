using Project.Core;

namespace Project.Character
{
    public class AttackState : IState
    {
        private readonly CharacterBrain _brain;

        public AttackState(CharacterBrain brain)
        {
            _brain = brain;
        }

        public void OnEnter() { }

        public void OnUpdate(float deltaTime)
        {
            if (_brain.MoveDirection.sqrMagnitude > 0.0001f)
            {
                _brain.ClearTarget();
                _brain.StateMachine.ChangeState(_brain.MoveState);
                return;
            }

            if (_brain.Target == null || !_brain.Target.IsAlive)
            {
                _brain.ClearTarget();
                _brain.StateMachine.ChangeState(_brain.IdleState);
                return;
            }

            if (!_brain.IsTargetInAttackRange())
            {
                _brain.StateMachine.ChangeState(_brain.ChaseState);
                return;
            }

            if (_brain.AttackClock.CanAttack)
            {
                _brain.PerformAttack();
            }
        }

        public void OnExit() { }
    }
}
