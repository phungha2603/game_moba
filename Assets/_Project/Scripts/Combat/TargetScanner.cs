using System.Collections.Generic;
using UnityEngine;

namespace Project.Combat
{
    /// Thin Physics wrapper, kept separate from TargetSelector so the priority-picking
    /// logic stays pure and unit-testable while this stays untested (needs a real Physics world).
    public static class TargetScanner
    {
        private const int MaxResults = 32;
        private static readonly Collider[] s_Buffer = new Collider[MaxResults];

        public static IEnumerable<Project.Shared.ITargetable> FindInRadius(Vector3 center, float radius)
        {
            var results = new List<Project.Shared.ITargetable>();
            int count = Physics.OverlapSphereNonAlloc(center, radius, s_Buffer);

            for (int i = 0; i < count; i++)
            {
                if (s_Buffer[i].TryGetComponent<Targetable>(out Targetable targetable))
                {
                    results.Add(targetable);
                }
            }

            return results;
        }
    }
}
