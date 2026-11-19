using System.Collections.Generic;
using Project.Shared;
using UnityEngine;

namespace Project.Lane
{
    /// Separate from Project.Combat's TargetSelector on purpose — a Tower's priority rule
    /// (defend allies first, then farm minions, then engage heroes) is genuinely different
    /// from Hero/Minion's (highest-value target in range), not a variant worth forcing into
    /// the same function.
    public static class TowerTargetSelector
    {
        public static ITargetable SelectTarget(
            Vector3 towerPosition,
            Team towerTeam,
            float range,
            IEnumerable<ITargetable> candidatesInRange)
        {
            ITargetable aggressor = FindAggressorAttackingAlly(towerPosition, towerTeam, range, candidatesInRange);
            if (aggressor != null) return aggressor;

            ITargetable nearestMinion = FindNearestOfType(towerPosition, towerTeam, candidatesInRange, TargetType.Minion);
            if (nearestMinion != null) return nearestMinion;

            return FindNearestOfType(towerPosition, towerTeam, candidatesInRange, TargetType.Hero);
        }

        private static ITargetable FindAggressorAttackingAlly(
            Vector3 towerPosition, Team towerTeam, float range, IEnumerable<ITargetable> candidates)
        {
            float rangeSqr = range * range;

            foreach (ITargetable candidate in candidates)
            {
                if (candidate == null || !candidate.IsAlive) continue;
                if (candidate.Team == towerTeam) continue;

                ITargetable victim = candidate.CurrentAttackTarget;
                if (victim == null || !victim.IsAlive) continue;
                if (victim.Team != towerTeam) continue;

                float victimDistanceSqr = (victim.Position - towerPosition).sqrMagnitude;
                if (victimDistanceSqr > rangeSqr) continue;

                return candidate;
            }

            return null;
        }

        private static ITargetable FindNearestOfType(
            Vector3 towerPosition, Team towerTeam, IEnumerable<ITargetable> candidates, TargetType type)
        {
            ITargetable nearest = null;
            float nearestDistanceSqr = float.MaxValue;

            foreach (ITargetable candidate in candidates)
            {
                if (candidate == null || !candidate.IsAlive) continue;
                if (candidate.Team == towerTeam) continue;
                if (candidate.Type != type) continue;

                float distanceSqr = (candidate.Position - towerPosition).sqrMagnitude;
                if (distanceSqr < nearestDistanceSqr)
                {
                    nearest = candidate;
                    nearestDistanceSqr = distanceSqr;
                }
            }

            return nearest;
        }
    }
}
