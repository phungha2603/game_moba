using Project.Core;
using UnityEngine;

internal static class PoolManagerTests
{
    private class FakePoolable : Component, IPoolable
    {
        public int SpawnCount;
        public int DespawnCount;
        public void OnSpawned() => SpawnCount++;
        public void OnDespawned() => DespawnCount++;
    }

    public static void Get_ReusesPrewarmedInstances()
    {
        var prefabGO = new GameObject("Prefab");
        var prefab = prefabGO.AddComponent<FakePoolable>();
        var pool = new PoolManager<FakePoolable>(prefab, prewarmCount: 2);

        FakePoolable a = pool.Get();
        FakePoolable b = pool.Get();

        TestRunner.Assert(!ReferenceEquals(a, b), "Two Get() calls after prewarm 2 should return distinct instances");
        TestRunner.AssertEqual(1, a.SpawnCount, "OnSpawned should be called exactly once on Get");
    }

    public static void Return_MakesInstanceAvailableAgainAndCallsOnDespawned()
    {
        var prefabGO = new GameObject("Prefab");
        var prefab = prefabGO.AddComponent<FakePoolable>();
        var pool = new PoolManager<FakePoolable>(prefab);

        FakePoolable a = pool.Get();
        pool.Return(a);
        FakePoolable b = pool.Get();

        TestRunner.Assert(ReferenceEquals(a, b), "Returned instance should be reused by the next Get()");
        TestRunner.AssertEqual(1, a.DespawnCount, "OnDespawned should be called exactly once on Return");
    }
}
