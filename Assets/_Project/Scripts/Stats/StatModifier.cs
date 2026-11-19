using Project.Shared;

namespace Project.Stats
{
    public readonly struct StatModifier
    {
        public readonly float Value;
        public readonly ModifierType Type;
        public readonly object Source;

        public StatModifier(float value, ModifierType type, object source)
        {
            Value = value;
            Type = type;
            Source = source;
        }
    }
}
