using System;

namespace Project.Skills
{
    /// Tagged-union instead of a polymorphic [SerializeReference] list: with no Unity Editor
    /// available to verify Inspector behavior, a plain serializable class is the safer bet.
    [Serializable]
    public class SkillEffectData
    {
        public SkillEffectType EffectType;

        /// Damage amount, Heal amount, or Slow percent (0-1). Flat designer-set value for now —
        /// no AD/AP scaling formula yet (nothing has asked for one with enough specificity).
        public float Value;

        /// Stun/Slow duration in seconds; unused for Damage/Heal.
        public float Duration;
    }
}
