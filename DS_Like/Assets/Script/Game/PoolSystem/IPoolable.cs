public interface IPoolable
{
    void OnSpawn();
    void OnDespawn();
    bool IsInPool();
}