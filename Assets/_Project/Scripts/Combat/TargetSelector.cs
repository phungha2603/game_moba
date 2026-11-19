using System.Collections.Generic;
using Project.Shared;
using UnityEngine;

namespace Project.Combat
{
    public static class TargetSelector
    {
        /// Picks the best target by priority (Hero > Minion > Tower > Monster), then by
        /// closest distance as a tie-break. Allies and dead/out-of-range candidates are skipped.
        public static ITargetable GetBestTarget(
            Vector3 seekerPosition,
            Team seekerTeam,
            float range,
            IEnumerable<ITargetable> candidates)
        {
            ITargetable best = null;
            int bestPriorityRank = int.MaxValue;
            float bestDistanceSqr = float.MaxValue;
            float rangeSqr = range * range;

            foreach (ITargetable candidate in candidates)
            {
                if (candidate == null) continue;
                if (!candidate.IsAlive) continue;
                if (candidate.Team == seekerTeam) continue;

                float distanceSqr = (candidate.Position - seekerPosition).sqrMagnitude;
                if (distanceSqr > rangeSqr) continue;

                int rank = PriorityRank(candidate.Type);
                bool isBetter = rank < bestPriorityRank
                    || (rank == bestPriorityRank && distanceSqr < bestDistanceSqr);

                if (!isBetter) continue;

                best = candidate;
                bestPriorityRank = rank;
                bestDistanceSqr = distanceSqr;
            }

            return best;
        }

        private static int PriorityRank(TargetType type)
        {
            switch (type)
            {
                case TargetType.Hero: return 0;
                case TargetType.Minion: return 1;
                case TargetType.Tower: return 2;
                case TargetType.Monster: return 3;
                default: return 99;
            }
        }
    }
}
