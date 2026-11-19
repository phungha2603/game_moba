using Project.Core;
using UnityEngine;

namespace Project.Character
{
    public class DeadState : IState
    {
        private readonly CharacterBrain _brain;

        public DeadState(CharacterBrain brain)
        {
            _brain = brain;
        }

        public void OnEnter()
        {
            _brain.MoveDirection = Vector3.zero;
        }

        public void OnUpdate(float deltaTime) { }

        public void OnExit() { }
    }
}
