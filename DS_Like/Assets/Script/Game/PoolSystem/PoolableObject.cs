using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolableObject : MonoBehaviour, IPoolable
{
    [SerializeField] protected float m_LifeTime = 5.0f;
    private float m_ResetTimer;

    protected virtual void Awake()
    {
        m_ResetTimer = m_LifeTime;
    }

    protected virtual void Update()
    {
        m_LifeTime -= Time.deltaTime;
        if (m_LifeTime <= 0f) ClearObject();
    }

    protected void ClearObject()
    {
        if (IsInPool()) PoolMgr.Instance.Despawn(gameObject);
        else Destroy(gameObject);
    }

    protected void ResetTimer()
    {
        m_LifeTime = m_ResetTimer;
    }

    public virtual void OnSpawn()
    {

    }

    public virtual void OnDespawn()
    {
        ResetTimer();
    }

    public virtual bool IsInPool()
    {
        return PoolMgr.Instance.IsInPool(gameObject.name);
    }
}