using Project.Core;
using Project.Shared;
using Project.Stats;
using UnityEngine;

namespace Project.Character
{
    [RequireComponent(typeof(CharacterStats))]
    public class CharacterBrain : MonoBehaviour
    {
        public CharacterStats Stats { get; private set; }
        public StateMachine StateMachine { get; private set; }
        public Vector3 MoveDirection { get; set; }

        public IdleState IdleState { get; private set; }
        public MoveState MoveState { get; private set; }
        public DeadState DeadState { get; private set; }

        private void Awake()
        {
            Stats = GetComponent<CharacterStats>();
            StateMachine = new StateMachine();

            IdleState = new IdleState(this);
            MoveState = new MoveState(this);
            DeadState = new DeadState(this);
        }

        private void OnEnable()
        {
            if (LogicTicker.Instance == null)
            {
                Debug.LogError("CharacterBrain requires a LogicTicker in the scene.", this);
                return;
            }

            LogicTicker.Instance.OnTick += HandleTick;
            StateMachine.ChangeState(IdleState);
        }

        private void OnDisable()
        {
            if (LogicTicker.Instance != null)
                LogicTicker.Instance.OnTick -= HandleTick;
        }

        public void Move(Vector3 direction, float deltaTime)
        {
            float speed = Stats.GetValue(StatType.MoveSpeed);
            transform.position += direction.normalized * speed * deltaTime;
        }

        private void HandleTick(float fixedDeltaTime)
        {
            StateMachine.Tick(fixedDeltaTime);
        }
    }
}
