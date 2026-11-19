namespace Project.Skills
{
    /// Runtime cooldown state for one SkillSO on one character; keeps SkillSO a pure data
    /// template that many characters/instances can share.
    public class SkillInstance
    {
        public readonly SkillSO Data;

        private float _cooldownRemaining;

        public SkillInstance(SkillSO data)
        {
            Data = data;
        }

        public bool IsReady => _cooldownRemaining <= 0f;

        public void Tick(float deltaTime)
        {
            if (_cooldownRemaining <= 0f) return;
            _cooldownRemaining -= deltaTime;
            if (_cooldownRemaining < 0f) _cooldownRemaining = 0f;
        }

        public void StartCooldown() => _cooldownRemaining = Data.Cooldown;
    }
}
