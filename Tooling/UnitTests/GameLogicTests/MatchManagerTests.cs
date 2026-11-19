using Project.Bootstrap;
using Project.Shared;
using Project.Stats;
using UnityEngine;

internal static class MatchManagerTests
{
    private static (MatchManager manager, CharacterStats blueNexus, CharacterStats redNexus) Spawn()
    {
        var managerGO = new GameObject("MatchManager");
        MatchManager manager = managerGO.AddComponent<MatchManager>();

        var blueGO = new GameObject("BlueNexus");
        CharacterStats blueNexus = blueGO.AddComponent<CharacterStats>();
        blueNexus.Initialize(new BaseStats { baseHealth = 1000f });

        var redGO = new GameObject("RedNexus");
        CharacterStats redNexus = redGO.AddComponent<CharacterStats>();
        redNexus.Initialize(new BaseStats { baseHealth = 1000f });

        manager.Configure(blueNexus, redNexus);
        return (manager, blueNexus, redNexus);
    }

    public static void BothNexusesAlive_MatchHasNotEnded()
    {
        (MatchManager manager, _, _) = Spawn();

        TestRunner.Assert(!manager.MatchEnded, "Match should not have ended while both Nexuses are alive");
    }

    public static void BlueNexusDies_RedTeamWins()
    {
        (MatchManager manager, CharacterStats blueNexus, _) = Spawn();
        Team? raisedWinner = null;
        manager.OnMatchEnded += winner => raisedWinner = winner;

        blueNexus.ApplyDamage(99999f);

        TestRunner.Assert(manager.MatchEnded, "Match should end once a Nexus dies");
        TestRunner.AssertEqual(Team.Red, manager.Winner ?? Team.None, "Red should win when the Blue Nexus dies");
        TestRunner.AssertEqual(Team.Red, raisedWinner ?? Team.None, "OnMatchEnded should report the correct winner");
    }

    public static void RedNexusDies_BlueTeamWins()
    {
        (MatchManager manager, _, CharacterStats redNexus) = Spawn();

        redNexus.ApplyDamage(99999f);

        TestRunner.AssertEqual(Team.Blue, manager.Winner ?? Team.None, "Blue should win when the Red Nexus dies");
    }

    public static void MatchAlreadyEnded_FurtherDamageDoesNotRaiseEventAgain()
    {
        (MatchManager manager, CharacterStats blueNexus, CharacterStats redNexus) = Spawn();
        int raisedCount = 0;
        manager.OnMatchEnded += _ => raisedCount++;

        blueNexus.ApplyDamage(99999f); // match ends, Red wins
        redNexus.ApplyDamage(99999f); // should be a no-op for match state

        TestRunner.AssertEqual(1, raisedCount, "OnMatchEnded must only fire once, for whichever Nexus died first");
        TestRunner.AssertEqual(Team.Red, manager.Winner ?? Team.None, "The winner must stay as whoever won first");
    }
}
