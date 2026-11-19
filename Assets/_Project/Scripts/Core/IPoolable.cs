namespace Project.Core
{
    public interface IPoolable
    {
        void OnSpawned();
        void OnDespawned();
    }
}
