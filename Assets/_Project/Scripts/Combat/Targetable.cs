using Project.Shared;
using Project.Stats;
using UnityEngine;

namespace Project.Combat
{
    [RequireComponent(typeof(CharacterStats))]
    public class Targetable : MonoBehaviour, ITargetable
    {
        [SerializeField] private Team _team;
        [SerializeField] private TargetType _type;

        private CharacterStats _stats;
        private bool _isTargetableOverride = true;

        public Vector3 Position => transform.position;
        public Team Team => _team;
        public TargetType Type => _type;
        public bool IsAlive => _isTargetableOverride && _stats.GetValue(StatType.Health) > 0f;

        private void Awake()
        {
            _stats = GetComponent<CharacterStats>();
        }

        public void Initialize(Team team, TargetType type)
        {
            _team = team;
            _type = type;
        }

        /// Lets a skill (e.g. untargetability) hide this unit from TargetSelector without touching its CharacterBrain.
        public void SetTargetableOverride(bool value) => _isTargetableOverride = value;
    }
}
