using Project.Core;
using UnityEngine;

namespace Project.Character
{
    public class ChaseState : IState
    {
        private readonly CharacterBrain _brain;

        public ChaseState(CharacterBrain brain)
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
