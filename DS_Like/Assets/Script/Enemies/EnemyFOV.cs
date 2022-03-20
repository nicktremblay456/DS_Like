using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFOV : MonoBehaviour
{
    [SerializeField] private Transform m_EyeTransform;
    [SerializeField] private float m_Radius;
    [SerializeField, Range(0, 360)] private float m_Angle;
    [Space]
    [SerializeField] private LayerMask m_TargetLayer;
    [SerializeField] private LayerMask m_ObstacleLayer;

    private float m_Delay = 0.1f;
    private WaitForSeconds m_Wait;

    private BaseEnemy m_Self;
    private bool m_CanSeePlayer = false;

    public GameObject Target { get => m_Self.Target; }
    public float Radius { get => m_Radius; }
    public float Angle { get => m_Angle; }
    public bool CanSeePlayer { get => m_CanSeePlayer; }

    private void Start()
    {
        m_Self = GetComponent<BaseEnemy>();
        m_Wait = new WaitForSeconds(m_Delay);
        StartCoroutine(FOV_Routine());
    }

    private IEnumerator FOV_Routine()
    {
        while(!m_Self.IsDeath)
        {
            yield return m_Wait;
            FOV_Check();
        }
    }

    private void FOV_Check()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, m_Radius, m_TargetLayer);

        if (rangeChecks.Length != 0)
        {
            Transform target = rangeChecks[0].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            if (Vector3.Angle(m_EyeTransform.forward, directionToTarget) < m_Angle * 0.5f)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);
                if (!Physics.Raycast(m_EyeTransform.position, directionToTarget, distanceToTarget, m_ObstacleLayer))
                    m_CanSeePlayer = true;
                else m_CanSeePlayer = false;
            }
            else m_CanSeePlayer = false;
        }
        else if (m_CanSeePlayer) m_CanSeePlayer = false;
    }
}