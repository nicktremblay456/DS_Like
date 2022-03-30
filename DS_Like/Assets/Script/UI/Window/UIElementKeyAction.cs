using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIElementKeyAction : MonoBehaviour
{
    [System.Serializable]
    public class KeyAction : UnityEngine.Events.UnityEvent
    { }

    [Header("Action")]
    [SerializeField] private KeyCode m_KeyCode;

    [SerializeField] private KeyAction m_KeyActions = new KeyAction();

    [Header("Timers")]
    [SerializeField] private float m_ActivationTime = 0.0f;
    [SerializeField] private bool m_Continous = false;

    private float m_ActiveTime { get; set; }
    private bool m_FiredInActiveTime { get; set; }
    private UIWindow m_Window { get; set; }

    protected virtual void Awake ()
    {
        m_Window = GetComponent<UIWindow>();
    }

    protected virtual void Update ()
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        if (m_Window != null && !m_Window.IsVisible)
        {
            return;
        }

        if (m_ActivationTime <= 0.01f)
        {
            if (m_Continous)
            {
                if (Input.GetKey(m_KeyCode))
                {
                    Activate();
                }
            }
            else
            {
                if (Input.GetKeyDown(m_KeyCode))
                {
                    Activate();
                }
            }

            return;
        }

        if (Input.GetKey(m_KeyCode))
        {
            m_ActiveTime += Time.deltaTime;
        }
        else
        {
            m_ActiveTime = 0.0f;
            m_FiredInActiveTime = false;
        }

        if (m_ActiveTime < m_ActivationTime)
        {
            return;
        }

        if (m_Continous)
        {
            if (Input.GetKey(m_KeyCode))
            {
                m_KeyActions.Invoke();
            }
        }
        else
        {
            if (m_FiredInActiveTime)
            {
                return;
            }

            if (Input.GetKey(m_KeyCode))
            {
                m_KeyActions.Invoke();
                m_FiredInActiveTime = true;
            }
        }
    }

    protected virtual void Activate ()
    {
        m_KeyActions.Invoke();
    }
}
