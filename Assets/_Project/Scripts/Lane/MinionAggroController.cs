using Project.Combat;
using Project.Core;
using Project.Shared;
using UnityEngine;

namespace Project.Lane
{
    /// Physics-touching glue (same split as Character/AutoAttackController): scans for a
    /// target each tick while the minion is marching, and hands MinionBrain an already-
    /// resolved target. Not unit-tested — TargetSelector/MinionBrain carry the tested logic.
    [RequireComponent(typeof(MinionBrain))]
    [RequireComponent(typeof(Targetable))]
    public class MinionAggroController : MonoBehaviour
    {
        private MinionBrain _brain;
        private Targetable _targetable;

        private void Awake()
        {
            _brain = GetComponent<MinionBrain>();
            _targetable = GetComponent<Targetable>();
        }

        private void OnEnable()
        {
            if (LogicTicker.Instance != null) LogicTicker.Instance.OnTick += HandleTick;
        }

        private void OnDisable()
        {
            if (LogicTicker.Instance != null) LogicTicker.Instance.OnTick -= HandleTick;
        }

        private void HandleTick(float fixedDeltaTime)
        {
            if (_brain.StateMachine.CurrentState != _brain.MoveState) return;

            var candidates = TargetScanner.FindInRadius(transform.position, _brain.AggroRange);
            ITargetable best = TargetSelector.GetBestTarget(transform.position, _targetable.Team, _brain.AggroRange, candidates);
            if (best != null) _brain.EngageTarget(best);
        }
    }
}
