using Project.Combat;
using Project.Shared;
using UnityEngine;

namespace Project.Character
{
    /// Physics-touching glue between the attack button and CharacterBrain: scans for a
    /// target, then hands the pure brain an already-resolved ITargetable. Not unit-tested
    /// (needs a real Physics world) — TargetSelector/CharacterBrain carry the tested logic.
    [RequireComponent(typeof(CharacterBrain))]
    [RequireComponent(typeof(Targetable))]
    public class AutoAttackController : MonoBehaviour
    {
        [SerializeField] private float _scanRange = 6f;

        private CharacterBrain _brain;
        private Targetable _targetable;

        private void Awake()
        {
            _brain = GetComponent<CharacterBrain>();
            _targetable = GetComponent<Targetable>();
        }

        public void RequestAttack()
        {
            var candidates = TargetScanner.FindInRadius(transform.position, _scanRange);
            ITargetable best = TargetSelector.GetBestTarget(transform.position, _targetable.Team, _scanRange, candidates);
            if (best != null) _brain.EngageTarget(best);
        }
    }
}
