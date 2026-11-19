using Project.Shared;
using UnityEngine;

internal class FakeTargetable : ITargetable
{
    public Vector3 Position { get; set; }
    public Team Team { get; set; }
    public TargetType Type { get; set; }
    public bool IsAlive { get; set; } = true;
}
