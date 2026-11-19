using System;
using Project.Shared;

namespace Project.Economy
{
    [Serializable]
    public class ItemStatBonus
    {
        public StatType Stat;
        public float Value;
        public ModifierType ModType;
    }
}
