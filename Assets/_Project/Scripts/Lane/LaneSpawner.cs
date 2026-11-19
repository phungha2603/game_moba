using Project.Combat;
using Project.Core;
using Project.Shared;
using Project.Stats;
using UnityEngine;

namespace Project.Lane
{
    /// GameObject-construction glue, same tier as Bootstrap/GlobalLoader — not unit-tested.
    public class LaneSpawner : MonoBehaviour
    {
        [SerializeField] private float _waveInterval = 30f;

        private BaseStats _minionStats;
        private LanePath _bluePath;
        private LanePath _redPath;
        private float _elapsed;

        public void Configure(BaseStats minionStats, LanePath bluePath, LanePath redPath)
        {
            _minionStats = minionStats;
            _bluePath = bluePath;
            _redPath = redPath;
            SpawnWave();
        }

        private void OnEnable()
        {
            if (LogicTicker.Instance != null) LogicTicker.Instance.OnTick += HandleTick;
        }

        private void OnDisable()
        {
            if (LogicTicker.Instance != null) LogicTicker.Instance.OnTick -= HandleTick;
        }

        private void HandleTick(float fixedDeltaTime)
        {
            _elapsed += fixedDeltaTime;
            if (_elapsed < _waveInterval) return;
            _elapsed = 0f;
            SpawnWave();
        }

        private void SpawnWave()
        {
            if (_minionStats == null) return;
            SpawnMinion(Team.Blue, _bluePath);
            SpawnMinion(Team.Red, _redPath);
        }

        private void SpawnMinion(Team team, LanePath path)
        {
            var go = new GameObject($"Minion_{team}");
            go.transform.position = path.Start;

            CharacterStats stats = go.AddComponent<CharacterStats>();
            stats.Initialize(_minionStats);

            Targetable targetable = go.AddComponent<Targetable>();
            targetable.Initialize(team, TargetType.Minion);
            go.AddComponent<SphereCollider>().radius = 0.5f;

            MinionBrain brain = go.AddComponent<MinionBrain>();
            brain.SetPath(path);
            go.AddComponent<MinionAggroController>();
        }
    }
}
