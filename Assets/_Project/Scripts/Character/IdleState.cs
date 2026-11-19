using Project.Core;

namespace Project.Character
{
    public class IdleState : IState
    {
        private readonly CharacterBrain _brain;

        public IdleState(CharacterBrain brain)
        {
            _brain = brain;
        }

        public void OnEnter() { }

        public void OnUpdate(float deltaTime)
        {
            if (_brain.MoveDirection.sqrMagnitude > 0.0001f)
            {
                _brain.StateMachine.ChangeState(_brain.MoveState);
            }
        }

        public void OnExit() { }
    }
}
