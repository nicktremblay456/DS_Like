using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
    [SerializeField] private int m_Damage;
    [SerializeField] private LayerMask m_DamageableLayer;
    private Collider m_Collider;
    private int m_ResetDamage;

    public Collider Collider { get => m_Collider; }

    private void Awake()
    {
        m_Collider = GetComponent<Collider>();
        m_Collider.isTrigger = true;
        m_Collider.enabled = false;
        m_ResetDamage = m_Damage;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (0 != (m_DamageableLayer.value & 1 << other.gameObject.layer))
        {
            IDamageable damageable = other.GetComponentInParent<IDamageable>();
            // Do damage
            if (damageable != null) damageable.TakeDamage(m_Damage, false);
            // Hit fx
            GameObject hitFx = PoolMgr.Instance.Spawn("FX_Blood", other.transform.position, other.transform.rotation);
            hitFx.transform.localScale = other.gameObject.transform.localScale * 0.5f;
            m_Collider.enabled = false;
        }
    }

    public void SetJumpAttackDamage()
    {
        m_Damage += m_Damage / 2;
    }

    public void ResetDamage()
    {
        m_Damage = m_ResetDamage;
    }

    public void ActivateWeaponCollider()
    {
        if (m_Collider != null)
        {
            if (!m_Collider.enabled) m_Collider.enabled = true;
        }
    }

    public void DeactivateWeaponCollider()
    {
        if (m_Collider != null)
        {
            if (m_Collider.enabled) m_Collider.enabled = false;
        }
    }
}