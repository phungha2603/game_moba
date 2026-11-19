using System.Collections.Generic;
using Project.Lane;
using Project.Shared;
using UnityEngine;

internal static class TowerTargetSelectorTests
{
    public static void NoCandidates_ReturnsNull()
    {
        ITargetable result = TowerTargetSelector.SelectTarget(Vector3.zero, Team.Blue, 6f, new List<ITargetable>());
        TestRunner.Assert(result == null, "No candidates should select nothing");
    }

    public static void DefaultsToNearestEnemyMinion()
    {
        var farMinion = new FakeTargetable { Position = new Vector3(5f, 0f, 0f), Team = Team.Red, Type = TargetType.Minion };
        var closeMinion = new FakeTargetable { Position = new Vector3(2f, 0f, 0f), Team = Team.Red, Type = TargetType.Minion };
        var candidates = new List<ITargetable> { farMinion, closeMinion };

        ITargetable result = TowerTargetSelector.SelectTarget(Vector3.zero, Team.Blue, 6f, candidates);

        TestRunner.Assert(ReferenceEquals(result, closeMinion), "Default priority should pick the nearest enemy minion");
    }

    public static void NoMinions_FallsBackToNearestEnemyHero()
    {
        var hero = new FakeTargetable { Position = new Vector3(3f, 0f, 0f), Team = Team.Red, Type = TargetType.Hero };
        var candidates = new List<ITargetable> { hero };

        ITargetable result = TowerTargetSelector.SelectTarget(Vector3.zero, Team.Blue, 6f, candidates);

        TestRunner.Assert(ReferenceEquals(result, hero), "With no minions in range, Tower should fall back to the enemy Hero");
    }

    public static void NeverSelectsAllies()
    {
        var allyMinion = new FakeTargetable { Position = new Vector3(1f, 0f, 0f), Team = Team.Blue, Type = TargetType.Minion };
        var candidates = new List<ITargetable> { allyMinion };

        ITargetable result = TowerTargetSelector.SelectTarget(Vector3.zero, Team.Blue, 6f, candidates);

        TestRunner.Assert(result == null, "An ally must never be selected");
    }

    public static void EnemyAttackingAllyInRange_OverridesDefaultMinionPriority()
    {
        var allyHero = new FakeTargetable { Position = new Vector3(1f, 0f, 0f), Team = Team.Blue, Type = TargetType.Hero };
        var enemyMinion = new FakeTargetable { Position = new Vector3(2f, 0f, 0f), Team = Team.Red, Type = TargetType.Minion };
        var enemyHeroAttackingAlly = new FakeTargetable
        {
            Position = new Vector3(1.5f, 0f, 0f),
            Team = Team.Red,
            Type = TargetType.Hero,
            CurrentAttackTarget = allyHero
        };
        var candidates = new List<ITargetable> { enemyMinion, enemyHeroAttackingAlly };

        ITargetable result = TowerTargetSelector.SelectTarget(Vector3.zero, Team.Blue, 6f, candidates);

        TestRunner.Assert(ReferenceEquals(result, enemyHeroAttackingAlly), "An enemy actively attacking an ally in range must override the default nearest-minion priority");
    }

    public static void EnemyAttackingAllyOutOfTowerRange_DoesNotTriggerSwitch()
    {
        var allyHeroFarAway = new FakeTargetable { Position = new Vector3(50f, 0f, 0f), Team = Team.Blue, Type = TargetType.Hero };
        var enemyMinion = new FakeTargetable { Position = new Vector3(2f, 0f, 0f), Team = Team.Red, Type = TargetType.Minion };
        var enemyHero = new FakeTargetable
        {
            Position = new Vector3(3f, 0f, 0f),
            Team = Team.Red,
            Type = TargetType.Hero,
            CurrentAttackTarget = allyHeroFarAway
        };
        var candidates = new List<ITargetable> { enemyMinion, enemyHero };

        ITargetable result = TowerTargetSelector.SelectTarget(Vector3.zero, Team.Blue, 6f, candidates);

        TestRunner.Assert(ReferenceEquals(result, enemyMinion), "The victim being out of tower range means this is not 'attacking an ally in range' — default priority should hold");
    }

    public static void DeadCandidates_AreIgnored()
    {
        var deadMinion = new FakeTargetable { Position = new Vector3(1f, 0f, 0f), Team = Team.Red, Type = TargetType.Minion, IsAlive = false };
        var candidates = new List<ITargetable> { deadMinion };

        ITargetable result = TowerTargetSelector.SelectTarget(Vector3.zero, Team.Blue, 6f, candidates);

        TestRunner.Assert(result == null, "Dead candidates must never be selected");
    }
}
