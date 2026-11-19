using Project.CameraSystem;
using Project.Character;
using Project.Core;
using Project.Stats;
using UnityEngine;

namespace Project.Bootstrap
{
    /// Builds the Phase 1 test scene entirely from code (hero, camera, ticker)
    /// so the project is playable without any manual Editor scene wiring.
    public class GlobalLoader : MonoBehaviour
    {
        [SerializeField] private string _heroStatsResourcePath = "DefaultHeroStats";

        private void Awake()
        {
            SpawnLogicTicker();
            Transform hero = SpawnHero();
            SpawnCamera(hero);
        }

        private static void SpawnLogicTicker()
        {
            var tickerGO = new GameObject("LogicTicker");
            tickerGO.AddComponent<LogicTicker>();
        }

        private Transform SpawnHero()
        {
            BaseStats baseStats = Resources.Load<BaseStats>(_heroStatsResourcePath);
            if (baseStats == null)
            {
                Debug.LogError($"GlobalLoader could not find BaseStats at Resources/{_heroStatsResourcePath}.");
            }

            var hero = new GameObject("Hero");
            CharacterStats stats = hero.AddComponent<CharacterStats>();
            stats.Initialize(baseStats);
            hero.AddComponent<CharacterBrain>();
            hero.AddComponent<DebugKeyboardInput>();
            hero.AddComponent<DebugStatsDisplay>();
            return hero.transform;
        }

        private static void SpawnCamera(Transform target)
        {
            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            camGO.AddComponent<Camera>();
            TopDownCamera topDown = camGO.AddComponent<TopDownCamera>();
            topDown.SetTarget(target);
        }
    }
}
