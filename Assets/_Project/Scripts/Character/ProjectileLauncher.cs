using Project.Combat;
using Project.Core;
using Project.Skills;
using UnityEngine;

namespace Project.Character
{
    /// Physics-touching glue (owns the PoolManager<ProjectileController>, same tier as
    /// AutoAttackController) — listens for CharacterBrain.OnSkillshotRequested so
    /// CharacterBrain itself never has to touch Physics/pooling directly.
    [RequireComponent(typeof(CharacterBrain))]
    [RequireComponent(typeof(Targetable))]
    public class ProjectileLauncher : MonoBehaviour
    {
        private const int PrewarmCount = 4;

        private CharacterBrain _brain;
        private Targetable _targetable;
        private PoolManager<ProjectileController> _pool;

        private void Awake()
        {
            _brain = GetComponent<CharacterBrain>();
            _targetable = GetComponent<Targetable>();

            var template = new GameObject("ProjectileTemplate (pooled)");
            template.SetActive(false);
            ProjectileController prefab = template.AddComponent<ProjectileController>();
            _pool = new PoolManager<ProjectileController>(prefab, PrewarmCount, transform);
        }

        private void OnEnable() => _brain.OnSkillshotRequested += HandleSkillshotRequested;

        private void OnDisable() => _brain.OnSkillshotRequested -= HandleSkillshotRequested;

        private void HandleSkillshotRequested(SkillSO skillData, Vector3 direction)
        {
            ProjectileController projectile = _pool.Get();
            projectile.Launch(skillData, _brain.Self, _targetable.Team, transform.position, direction, _pool);
        }
    }
}
