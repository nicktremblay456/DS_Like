using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private bool m_IsLockedOnStart = false;

    private Animator m_Animator;

    private bool m_IsReady = true;
    private bool m_IsOpen = false;
    private bool m_IsInRange = false;

    private bool m_IsLocked = false;

    private readonly int m_HashOpen = Animator.StringToHash("Open");
    private readonly int m_HashClose = Animator.StringToHash("Close");

    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
        if (m_IsLockedOnStart) m_IsLocked = true;
    }

    private void Update()
    {
        if (!m_IsLocked && m_IsInRange && m_IsReady && Input.GetKeyDown(KeyCode.E))
        {
            m_IsOpen = !m_IsOpen;
            m_IsReady = false;
            SetDoorTrigger(m_IsOpen);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (0 != (LayerMask.GetMask("Player") & 1 << other.gameObject.layer))
        {
            m_IsInRange = true;
        }
        if (0 != (LayerMask.GetMask("Enemy") & 1 << other.gameObject.layer))
        {
            if (!m_IsOpen)
            {
                m_IsOpen = true;
                SetDoorTrigger(m_IsOpen);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (0 != (LayerMask.GetMask("Player") & 1 << other.gameObject.layer))
        {
            m_IsInRange = false;
        }
    }

    private void SetDoorTrigger(bool open)
    {
        if (open)
        {
            if (!m_IsOpen) m_IsOpen = true;
            m_Animator.SetTrigger(m_HashOpen);
        }
        else
        {
            if (m_IsOpen) m_IsOpen = false;
            m_Animator.SetTrigger(m_HashClose);
        }
    }

    public void UnlockDoor()
    {
        m_IsLocked = false;
    }

    public void OpenAndUnlockDoor()
    {
        m_IsLocked = false;
        if (!m_IsOpen)
        {
            m_IsOpen = true;
            m_Animator.SetTrigger(m_HashOpen);
        }
    }

    public void CloseAndLockDoor()
    {
        m_IsLocked = true;
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