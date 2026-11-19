using Project.Stats;
using UnityEngine;

namespace Project.Visuals
{
    /// "Model/Animation" without a Unity Editor: no FBX import, no Animator Controller asset
    /// (a complex graph that can't safely be hand-written as YAML). The stand-in is a
    /// GameObject.CreatePrimitive shape, tinted by code, with two purely procedural
    /// "animations" (hit-flash, attack-pulse) driven by plain timers in Update() — no
    /// Coroutines, no Animator, nothing an Editor would normally configure.
    /// Rendering-dependent (Material/Shader/Renderer, none of which the .NET shim has), so —
    /// like the UI scripts — this is review-only, never unit-tested. Takes no dependency on
    /// CharacterBrain/MinionBrain/TowerBrain: whoever spawns a unit wires
    /// `brain.OnAttackPerformed += view.PlayAttackPulse` itself, so this class stays generic.
    public class CharacterView : MonoBehaviour
    {
        private const float FlashDuration = 0.1f;
        private const float AttackPulseDuration = 0.1f;
        private const float AttackPulseScale = 1.2f;

        private CharacterStats _stats;
        private Transform _model;
        private Material _material;
        private Color _baseColor;
        private Vector3 _baseScale;
        private float _flashTimer;
        private float _pulseTimer;

        public void Configure(CharacterStats stats, PrimitiveType shape, Color color)
        {
            _stats = stats;
            _baseColor = color;
            BuildModel(shape, color);
            _stats.OnDamaged += HandleDamaged;
        }

        /// Hook this up to any brain's OnAttackPerformed event from the spawning code.
        public void PlayAttackPulse() => _pulseTimer = AttackPulseDuration;

        private void BuildModel(PrimitiveType shape, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(shape);
            primitive.transform.SetParent(transform, false);

            // The root GameObject already carries its own Physics collider sized for
            // gameplay; CreatePrimitive's default collider would just be a duplicate.
            Collider primitiveCollider = primitive.GetComponent<Collider>();
            if (primitiveCollider != null) Destroy(primitiveCollider);

            _model = primitive.transform;
            _baseScale = _model.localScale;

            _material = new Material(Shader.Find("Standard"));
            _material.color = color;
            primitive.GetComponent<Renderer>().material = _material;
        }

        private void HandleDamaged(float amount) => _flashTimer = FlashDuration;

        private void Update()
        {
            if (_flashTimer > 0f)
            {
                _flashTimer -= Time.deltaTime;
                _material.color = _flashTimer > 0f ? Color.red : _baseColor;
            }

            if (_pulseTimer > 0f)
            {
                _pulseTimer -= Time.deltaTime;
                _model.localScale = _pulseTimer > 0f ? _baseScale * AttackPulseScale : _baseScale;
            }
        }

        private void OnDestroy()
        {
            if (_stats != null) _stats.OnDamaged -= HandleDamaged;
        }
    }
}
