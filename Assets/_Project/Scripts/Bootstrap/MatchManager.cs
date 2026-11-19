using System;
using Project.Core;
using Project.Shared;
using Project.Stats;
using UnityEngine;

namespace Project.Bootstrap
{
    /// Win condition: a Nexus dying ends the match for its team. Uses only
    /// CharacterStats.OnHealthChanged and GUI.Label (both already shimmed from earlier
    /// phases — DebugStatsDisplay uses the same OnGUI approach), so unlike the rest of
    /// Bootstrap this class stays fully unit-testable.
    public class MatchManager : MonoBehaviour
    {
        private CharacterStats _blueNexus;
        private CharacterStats _redNexus;

        public bool MatchEnded { get; private set; }
        public Team? Winner { get; private set; }

        public event Action<Team> OnMatchEnded;

        public void Configure(CharacterStats blueNexus, CharacterStats redNexus)
        {
            _blueNexus = blueNexus;
            _redNexus = redNexus;
            _blueNexus.OnHealthChanged += (_, __) => CheckForGameOver();
            _redNexus.OnHealthChanged += (_, __) => CheckForGameOver();
        }

        private void CheckForGameOver()
        {
            if (MatchEnded) return;

            if (!_blueNexus.IsAlive) EndMatch(Team.Red);
            else if (!_redNexus.IsAlive) EndMatch(Team.Blue);
        }

        private void EndMatch(Team winner)
        {
            MatchEnded = true;
            Winner = winner;
            if (LogicTicker.Instance != null) LogicTicker.Instance.enabled = false;
            OnMatchEnded?.Invoke(winner);
        }

        private void OnGUI()
        {
            if (!MatchEnded || Winner == null) return;
            GUI.Label(new Rect(Screen.width / 2f - 100f, 40f, 200f, 40f), $"Team {Winner} Wins!");
        }
    }
}
