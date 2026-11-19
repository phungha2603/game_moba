namespace Project.Core
{
    public class StateMachine
    {
        public IState CurrentState { get; private set; }

        public void ChangeState(IState newState)
        {
            if (ReferenceEquals(CurrentState, newState)) return;

            CurrentState?.OnExit();
            CurrentState = newState;
            CurrentState?.OnEnter();
        }

        public void Tick(float deltaTime)
        {
            CurrentState?.OnUpdate(deltaTime);
        }
    }
}
