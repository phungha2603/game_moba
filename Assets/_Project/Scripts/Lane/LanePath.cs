using UnityEngine;

namespace Project.Lane
{
    /// Code-driven straight-line path — no Editor-placed waypoints needed, since a single
    /// Mid lane vertical slice doesn't need turns yet. A richer multi-point path can replace
    /// this later without changing MinionBrain's API (it only ever reads Direction).
    public class LanePath
    {
        public readonly Vector3 Start;
        public readonly Vector3 End;

        public LanePath(Vector3 start, Vector3 end)
        {
            Start = start;
            End = end;
        }

        public Vector3 Direction => (End - Start).normalized;
    }
}
