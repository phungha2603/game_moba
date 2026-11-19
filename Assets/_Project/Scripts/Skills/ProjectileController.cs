using Project.Combat;
using Project.Core;
using Project.Shared;
using UnityEngine;

namespace Project.Skills
{
    /// Physics-touching (Physics.OverlapSphereNonAlloc), so — like TargetScanner/
    /// AutoAttackController — this stays untested; the pure logic it calls into
    /// (SkillExecutor.ApplyEffect) is what's unit-tested. Flies in a straight line
    /// (transform.position += direction*speed*deltaTime, no Rigidbody — nothing to
    /// configure through an Editor), hits the first enemy Targetable it overlaps, and
    /// returns itself to the pool either way (first hit, or max range exceeded).
    public class ProjectileController : MonoBehaviour, IPoolable
    {
        private const int MaxOverlapResults = 8;
        private const float HitRadius = 0.5f;

        private static readonly Collider[] s_Buffer = new Collider[MaxOverlapResults];

        private SkillSO _skillData;
        private ITargetable _caster;
        private Team _casterTeam;
        private Vector3 _direction;
        private float _traveled;
        private PoolManager<ProjectileController> _pool;

        public void Launch(
            SkillSO skillData,
            ITargetable caster,
            Team casterTeam,
            Vector3 origin,
            Vector3 direction,
            PoolManager<ProjectileController> pool)
        {
            _skillData = skillData;
            _caster = caster;
            _casterTeam = casterTeam;
            _direction = direction.normalized;
            _traveled = 0f;
            _pool = pool;
            transform.position = origin;
        }

        private void Update()
        {
            float step = _skillData.ProjectileSpeed * Time.deltaTime;
            transform.position += _direction * step;
            _traveled += step;

            if (CheckForHit()) { Despawn(); return; }
            if (_traveled >= _skillData.ProjectileRange) Despawn();
        }

        private bool CheckForHit()
        {
            int count = Physics.OverlapSphereNonAlloc(transform.position, HitRadius, s_Buffer);

            for (int i = 0; i < count; i++)
            {
                if (!s_Buffer[i].TryGetComponent(out Targetable targetable)) continue;
                if (targetable.Team == _casterTeam) continue;
                if (!targetable.IsAlive) continue;

                foreach (SkillEffectData effect in _skillData.Effects)
                {
                    SkillExecutor.ApplyEffect(effect, _skillData.DamageType, _caster, targetable);
                }

                return true;
            }

            return false;
        }

        private void Despawn() => _pool.Return(this);

        public void OnSpawned() { }

        public void OnDespawned() { }
    }
}
