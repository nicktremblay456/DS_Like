using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
    [SerializeField] private int m_Damage;
    [SerializeField] private LayerMask m_DamageableLayer;
    private Collider m_Collider;

    private void Awake()
    {
        m_Collider = GetComponent<Collider>();
        m_Collider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (0 != (m_DamageableLayer.value & 1 << other.gameObject.layer))
        {
            m_Collider.enabled = false;
            // Do damage
        }
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