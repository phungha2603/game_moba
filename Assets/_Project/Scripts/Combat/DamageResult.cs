namespace Project.Combat
{
    public readonly struct DamageResult
    {
        public readonly float FinalDamage;
        public readonly bool IsCritical;
        public readonly float LifeStealAmount;

        public DamageResult(float finalDamage, bool isCritical, float lifeStealAmount)
        {
            FinalDamage = finalDamage;
            IsCritical = isCritical;
            LifeStealAmount = lifeStealAmount;
        }
    }
}
