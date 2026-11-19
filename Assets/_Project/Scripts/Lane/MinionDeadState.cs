using Project.Core;

namespace Project.Lane
{
    public class MinionDeadState : IState
    {
        private readonly MinionBrain _brain;

        public MinionDeadState(MinionBrain brain)
        {
            _brain = brain;
        }

        public void OnEnter() => _brain.ClearTarget();

        public void OnUpdate(float deltaTime) { }

        public void OnExit() { }
    }
}
