using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    [SerializeField] private LayerMask m_TargetLayer;

    private void OnTriggerEnter(Collider other)
    {
        if (0 != (m_TargetLayer.value & 1 << other.gameObject.layer))
        {
            IDamageable target = other.gameObject.GetComponent<IDamageable>();
            if (target != null) target.TakeDamage(10000, true);
            GameObject hitFx = PoolMgr.Instance.Spawn("FX_Blood", other.transform.position, other.transform.rotation);
            hitFx.transform.localScale = other.gameObject.transform.localScale * 0.5f;
        }
    }
}