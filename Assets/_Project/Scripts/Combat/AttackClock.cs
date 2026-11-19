namespace Project.Combat
{
    public class AttackClock
    {
        public float AttacksPerSecond { get; set; }

        private float _elapsed;

        private float AttackInterval => AttacksPerSecond > 0f ? 1f / AttacksPerSecond : float.PositiveInfinity;

        public bool CanAttack => _elapsed >= AttackInterval;

        public void Tick(float deltaTime) => _elapsed += deltaTime;

        public void Reset() => _elapsed = 0f;
    }
}
