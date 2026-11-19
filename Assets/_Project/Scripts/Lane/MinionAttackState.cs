using Project.Core;

namespace Project.Lane
{
    public class MinionAttackState : IState
    {
        private readonly MinionBrain _brain;

        public MinionAttackState(MinionBrain brain)
        {
            _brain = brain;
        }

        public void OnEnter() { }

        public void OnUpdate(float deltaTime)
        {
            if (_brain.Target == null || !_brain.Target.IsAlive)
            {
                _brain.ClearTarget();
                _brain.StateMachine.ChangeState(_brain.MoveState);
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
