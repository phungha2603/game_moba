using Project.Core;

namespace Project.Character
{
    public class MoveState : IState
    {
        private readonly CharacterBrain _brain;

        public MoveState(CharacterBrain brain)
        {
            _brain = brain;
        }

        public void OnEnter() { }

        public void OnUpdate(float deltaTime)
        {
            if (_brain.MoveDirection.sqrMagnitude <= 0.0001f)
            {
                _brain.StateMachine.ChangeState(_brain.IdleState);
                return;
            }

            _brain.Move(_brain.MoveDirection, deltaTime);
        }

        public void OnExit() { }
    }
}
