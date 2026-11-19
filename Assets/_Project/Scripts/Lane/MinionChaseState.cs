using Project.Core;
using UnityEngine;

namespace Project.Lane
{
    public class MinionChaseState : IState
    {
        private readonly MinionBrain _brain;

        public MinionChaseState(MinionBrain brain)
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

            if (_brain.IsTargetBeyondAggroRange())
            {
                _brain.ClearTarget();
                _brain.StateMachine.ChangeState(_brain.MoveState);
                return;
            }

            if (_brain.IsTargetInAttackRange())
            {
                _brain.StateMachine.ChangeState(_brain.AttackState);
                return;
            }

            Vector3 direction = _brain.Target.Position - _brain.transform.position;
            _brain.Move(direction, deltaTime);
        }

        public void OnExit() { }
    }
}
