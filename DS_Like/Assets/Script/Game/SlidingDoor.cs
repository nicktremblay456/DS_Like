using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidingDoor : MonoBehaviour
{
    [SerializeField] private Transform m_Door;
    [SerializeField] private Transform m_StartPosition;
    [SerializeField] private Transform m_EndPosition;
    [SerializeField] private float m_Duration;

    private Coroutine m_OpenRoutine;
    private Coroutine m_CloseRoutine;

    [SerializeField] private int m_NumberOfSwitchRequire;
    private int m_CurrentSwitchActivated;

    private bool m_IsOpen = false;

    public void DoorSwitchActivated()
    {
        Debug.Log("Activated");
        m_CurrentSwitchActivated++;
        if (m_CurrentSwitchActivated == m_NumberOfSwitchRequire)
        {
            TriggerDoorOpen();
        }
    }

    public void DoorSwitchDeactivated()
    {
        m_CurrentSwitchActivated--;
        if (m_CurrentSwitchActivated != m_NumberOfSwitchRequire && m_IsOpen)
        {
            TriggerDoorClose();
        }
    }

    public void TriggerDoorOpen()
    {
        if (m_OpenRoutine != null) StopCoroutine(m_OpenRoutine);
        m_OpenRoutine = StartCoroutine(DoorCoroutine(true));
    }

    public void TriggerDoorClose()
    {
        if (m_CloseRoutine != null) StopCoroutine(m_CloseRoutine);
        m_CloseRoutine = StartCoroutine(DoorCoroutine(false));
    }

    public IEnumerator DoorCoroutine(bool open)
    {
        Vector3 target = open ? m_EndPosition.position : m_StartPosition.position;
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / m_Duration;
            m_Door.position = Vector3.Lerp(m_Door.position, target, t);
            yield return null;
        }
        m_Door.position = target;
        m_IsOpen = open ? true : false;
    }
}