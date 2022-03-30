using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BladeTrap : MonoBehaviour
{
    [SerializeField] private LayerMask m_TargetLayer;
    [SerializeField] private Animation m_BladeAnimation;
    private bool m_IsInRange = false;

    private void OnTriggerEnter(Collider other)
    {
        if (0 != (m_TargetLayer.value & 1 << other.gameObject.layer))
        {
            m_BladeAnimation.Play();
        }
    }
}