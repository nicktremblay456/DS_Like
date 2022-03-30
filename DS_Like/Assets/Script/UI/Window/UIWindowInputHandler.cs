using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UIWindow))]
public class UIWindowInputHandler : MonoBehaviour, IUIWindowInputHandler
{
    [SerializeField] private KeyCode m_KeyCode = KeyCode.None; 
    protected UIWindow m_Window;
    

    protected virtual void Awake ()
    {
        m_Window = GetComponent<UIWindow>();
    }

    protected virtual void Update ()
    {
        if (Input.GetKeyDown(m_KeyCode))
        {
            m_Window.Toggle();
        }
    }
}