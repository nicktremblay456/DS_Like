using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class TriggerEvent : MonoBehaviour
{
    [SerializeField] private LayerMask m_TriggerLayer;
    [Space]
    [SerializeField] private UnityEvent m_OnTriggerEnter;
    [SerializeField] private UnityEvent m_OnTriggerExit;

    private Collider m_Collider;


    private void Awake()
    {
        m_Collider = GetComponent<Collider>();
        if (!m_Collider.isTrigger) m_Collider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (0 != (m_TriggerLayer.value & 1 << other.gameObject.layer))
        {
            if (m_OnTriggerEnter != null) m_OnTriggerEnter.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (0 != (m_TriggerLayer.value & 1 << other.gameObject.layer))
        {
            if (m_OnTriggerExit != null) m_OnTriggerExit.Invoke();
        }
    }
}