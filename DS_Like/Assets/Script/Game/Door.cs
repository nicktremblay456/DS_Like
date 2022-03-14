using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private LayerMask m_TriggerLayer;

    private Animator m_Animator;

    private bool m_IsReady = true;
    private bool m_IsOpen = false;
    private bool m_IsInRange = false;

    private bool m_IsDoorLocked = false;

    private readonly int m_HashOpen = Animator.StringToHash("Open");
    private readonly int m_HashClose = Animator.StringToHash("Close");

    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (!m_IsDoorLocked && m_IsInRange && m_IsReady && Input.GetKeyDown(KeyCode.E))
        {
            m_IsOpen = !m_IsOpen;
            m_IsReady = false;
            if (m_IsOpen) m_Animator.SetTrigger(m_HashOpen);
            else m_Animator.SetTrigger(m_HashClose);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (0 != (m_TriggerLayer.value & 1 << other.gameObject.layer))
        {
            m_IsInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (0 != (m_TriggerLayer.value & 1 << other.gameObject.layer))
        {
            m_IsInRange = false;
        }
    }

    public void OpenAndUnlockDoor()
    {
        m_IsDoorLocked = false;
        if (!m_IsOpen)
        {
            m_IsOpen = true;
            m_Animator.SetTrigger(m_HashOpen);
        }
    }

    public void CloseAndLockDoor()
    {
        m_IsDoorLocked = true;
        if (m_IsOpen)
        {
            m_IsOpen = false;
            m_Animator.SetTrigger(m_HashClose);
        }
    }

    public void OnAnimEnd()
    {
        m_IsReady = true;
    }
}