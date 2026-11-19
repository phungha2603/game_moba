using Project.Core;

namespace Project.Lane
{
    public class MinionMoveState : IState
    {
        private readonly MinionBrain _brain;

        public MinionMoveState(MinionBrain brain)
        {
            _brain = brain;
        }

        public void OnEnter() { }

        public void OnUpdate(float deltaTime)
        {
            if (_brain.Path == null) return;
            _brain.Move(_brain.Path.Direction, deltaTime);
        }

        public void OnExit() { }
    }
}
