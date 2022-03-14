using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sentry : MonoBehaviour
{
    [SerializeField] private Transform[] m_Spawns;
    [SerializeField] private GameObject m_Missile;
    [SerializeField] private float m_ShootForce;
    [SerializeField] private float m_AttackTimer;

    private float m_ResetTimer;
    private bool m_IsPlayerInRange = false;

    private void Awake()
    {
        m_ResetTimer = m_AttackTimer;    
    }

    private void Update()
    {
        if (m_IsPlayerInRange)
        {
            m_AttackTimer -= Time.deltaTime;
            if (m_AttackTimer <= 0)
            {
                for (int i = 0; i < m_Spawns.Length; i++)
                {
                    Shoot(m_Spawns[i]);
                }
                m_AttackTimer = m_ResetTimer;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (0 != (LayerMask.GetMask("Player") & 1 << other.gameObject.layer))
        {
            m_IsPlayerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (0 != (LayerMask.GetMask("Player") & 1 << other.gameObject.layer))
        {
            m_IsPlayerInRange = false;
        }
    }

    private void Shoot(Transform spawn)
    {
        Ray ray = new Ray(spawn.position, spawn.forward);
        RaycastHit hit;

        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit)) targetPoint = hit.point;
        else targetPoint = ray.GetPoint(75);

        Vector3 directionWithoutSpread = targetPoint - transform.position;

        Missile missile = PoolMgr.Instance.Spawn(m_Missile.name, spawn.position, Quaternion.identity).GetComponent<Missile>();
        missile.TargetLayer = LayerMask.GetMask("Player");
        missile.transform.forward = directionWithoutSpread.normalized;

        missile.Rigidbody.AddForce(directionWithoutSpread.normalized * m_ShootForce, ForceMode.Impulse);
        //missile.Rigidbody.AddForce(spawn.up, ForceMode.Impulse);
    }
}