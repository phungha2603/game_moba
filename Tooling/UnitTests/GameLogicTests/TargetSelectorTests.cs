using System.Collections.Generic;
using Project.Combat;
using Project.Shared;
using UnityEngine;

internal static class TargetSelectorTests
{
    public static void PicksHeroOverFartherMinion_ByPriorityNotDistance()
    {
        var closeMinion = new FakeTargetable { Position = new Vector3(1f, 0f, 0f), Team = Team.Red, Type = TargetType.Minion };
        var fartherHero = new FakeTargetable { Position = new Vector3(5f, 0f, 0f), Team = Team.Red, Type = TargetType.Hero };
        var candidates = new List<ITargetable> { closeMinion, fartherHero };

        ITargetable best = TargetSelector.GetBestTarget(Vector3.zero, Team.Blue, range: 10f, candidates);

        TestRunner.Assert(ReferenceEquals(best, fartherHero), "Hero priority should win even though the Minion is closer");
    }

    public static void TiesOnPriority_PicksClosestCandidate()
    {
        var farMinion = new FakeTargetable { Position = new Vector3(5f, 0f, 0f), Team = Team.Red, Type = TargetType.Minion };
        var closeMinion = new FakeTargetable { Position = new Vector3(1f, 0f, 0f), Team = Team.Red, Type = TargetType.Minion };
        var candidates = new List<ITargetable> { farMinion, closeMinion };

        ITargetable best = TargetSelector.GetBestTarget(Vector3.zero, Team.Blue, range: 10f, candidates);

        TestRunner.Assert(ReferenceEquals(best, closeMinion), "Same priority should tie-break to the closest candidate");
    }

    public static void SkipsAllies()
    {
        var ally = new FakeTargetable { Position = new Vector3(1f, 0f, 0f), Team = Team.Blue, Type = TargetType.Hero };
        var candidates = new List<ITargetable> { ally };

        ITargetable best = TargetSelector.GetBestTarget(Vector3.zero, Team.Blue, range: 10f, candidates);

        TestRunner.Assert(best == null, "A same-team candidate must never be selected");
    }

    public static void SkipsDeadCandidates()
    {
        var dead = new FakeTargetable { Position = new Vector3(1f, 0f, 0f), Team = Team.Red, Type = TargetType.Hero, IsAlive = false };
        var candidates = new List<ITargetable> { dead };

        ITargetable best = TargetSelector.GetBestTarget(Vector3.zero, Team.Blue, range: 10f, candidates);

        TestRunner.Assert(best == null, "A dead candidate must never be selected");
    }

    public static void SkipsCandidatesOutOfRange()
    {
        var farAway = new FakeTargetable { Position = new Vector3(100f, 0f, 0f), Team = Team.Red, Type = TargetType.Hero };
        var candidates = new List<ITargetable> { farAway };

        ITargetable best = TargetSelector.GetBestTarget(Vector3.zero, Team.Blue, range: 10f, candidates);

        TestRunner.Assert(best == null, "A candidate beyond range must never be selected");
    }
}
