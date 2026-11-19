using Project.Core;

namespace Project.Character
{
    public class StunnedState : IState
    {
        private readonly CharacterBrain _brain;

        public StunnedState(CharacterBrain brain)
        {
            _brain = brain;
        }

        public void OnEnter()
        {
            _brain.ClearTarget();
        }

        public void OnUpdate(float deltaTime)
        {
            if (_brain.StatusEffects == null || !_brain.StatusEffects.IsStunned)
            {
                _brain.StateMachine.ChangeState(_brain.IdleState);
            }
        }

        public void OnExit() { }
    }
}
