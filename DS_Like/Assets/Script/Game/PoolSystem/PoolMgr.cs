using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PoolInfo
{
    public bool IsUIobj = false;
    public GameObject ObjectToSpawn;
    public int InitialCount = 1;
    public int MaxCount = -1;// -1 -> Infinite
}

public struct PoolSpawnData
{
    public int CreatedCount;
    public PoolInfo PoolInfo;
    public Transform Container;
    public List<GameObject> SpawnedObjects;

    public PoolSpawnData(PoolInfo poolInfo)
    {
        CreatedCount = 0;
        PoolInfo = poolInfo;
        Container = null;
        SpawnedObjects = new List<GameObject>();
    }

    public void IncrementCount()
    {
        CreatedCount++;
    }

    public void AddObject(GameObject obj)
    {
        SpawnedObjects.Add(obj);
    }
}

public class PoolMgr : MonoBehaviour
{
    #region Variables/Props
    private static PoolMgr m_Instance;
    public static PoolMgr Instance { get => m_Instance; }

    [SerializeField] private List<PoolInfo> m_PoolInfos = new List<PoolInfo>();
    private Dictionary<string, PoolSpawnData> m_SpawnData = new Dictionary<string, PoolSpawnData>();
    #endregion

    private void Awake()
    {
        if (m_Instance == null) m_Instance = this;
        else if (m_Instance != this) Destroy(this);

        InitPool();
    }

    public bool IsInPool(string objName)
    {
        return m_SpawnData.ContainsKey(objName);
    }

    public GameObject Spawn(string objName, Vector3 position = default, Quaternion rotation = default, RectTransform rect = default)
    {
        if (!m_SpawnData.ContainsKey(objName))
        {
            Debug.LogError($"Can't spawn {objName} since it doesn't exist in Pool Info!");
            return null;
        }

        GameObject availableObj = null;
        int spawnedCount = m_SpawnData[objName].SpawnedObjects.Count;
        if (spawnedCount == 0)
        {
            int maxCount = m_SpawnData[objName].PoolInfo.MaxCount;
            if (maxCount >= 0 && m_SpawnData[objName].CreatedCount > maxCount)
            {
                Debug.LogError($"Can't spawn more object ({objName}) since the limit has been reached!");
                return null;
            }

            availableObj = CreateObject(m_SpawnData[objName].PoolInfo.ObjectToSpawn);
        }
        else
        {
            availableObj = m_SpawnData[objName].SpawnedObjects[0];
        }

        m_SpawnData[objName].SpawnedObjects.RemoveAt(0);

        availableObj.SetActive(true);
        if (m_SpawnData[objName].PoolInfo.IsUIobj)
        {
            availableObj.transform.SetParent(rect);
            availableObj.transform.localPosition = rect.anchoredPosition;
        }
        else
        {
            availableObj.transform.position = position;
            availableObj.transform.rotation = rotation;
        }
        IPoolable[] poolables = availableObj.GetComponents<IPoolable>();
        for (int i = 0; i < poolables.Length; i++) poolables[i].OnSpawn();

        return availableObj;
    }

    public void Despawn(GameObject obj)
    {
        if (obj != null)
        {
            IPoolable[] poolables = obj.GetComponents<IPoolable>();
            for (int i = 0; i < poolables.Length; i++) poolables[i].OnDespawn();
            
            AddToPool(obj);
        }
    }

    private void InitPool()
    {
        for (int i = 0; i < m_PoolInfos.Count; i++)
        {
            string objName = m_PoolInfos[i].ObjectToSpawn.name;
            if (!IsInPool(objName))
            {
                PoolSpawnData newSpawnData = new PoolSpawnData(m_PoolInfos[i]);
                m_SpawnData.Add(objName, newSpawnData);
                GameObject container = new GameObject($"{objName}_Pool");
                container.transform.SetParent(transform);

                newSpawnData.Container = container.transform;
                m_SpawnData[objName] = newSpawnData;

                for (int j = 0; j < m_PoolInfos[i].InitialCount; j++)
                {
                    CreateObject(m_PoolInfos[i].ObjectToSpawn);
                }
            }
        }
    }

    private GameObject CreateObject(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("You are trying to create a null object");
            return null;
        }

        GameObject newObj = Instantiate(prefab);
        newObj.name = newObj.name.Replace("(Clone)", "");
        PoolSpawnData temp = m_SpawnData[prefab.name];
        temp.CreatedCount++;
        m_SpawnData[prefab.name] = temp;
        AddToPool(newObj);

        return newObj;
    }

    private void AddToPool(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogError("You are trying to add to pool a null object");
            return;
        }

        if (!m_SpawnData.ContainsKey(obj.name))
        {
            Debug.LogError($"Can't add to pool {obj.name} since it doesn't exist in the pool");
            return;
        }

        obj.SetActive(false);
        obj.transform.SetParent(m_SpawnData[obj.name].Container);
        m_SpawnData[obj.name].AddObject(obj);
    }
}