using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIWindowPage : UIWindow
{
    [Header("Page Specific")]
    public bool IsDefaultPage = true;

    [SerializeField] protected bool m_IsEnabled = true;
    public bool IsEnabled
    {
        get { return m_IsEnabled; }
        set
        {
            m_IsEnabled = value;
            MyButton.enabled = !IsEnabled;

            if (!m_IsEnabled)
            {
                Hide();
            }
        }
    }

    public UnityEngine.UI.Button MyButton;

    private UIWindow m_WindowParent;
    public UIWindow WindowParent
    {
        get
        {
            if (m_WindowParent == null)
            {
                m_WindowParent = transform.parent.GetComponentInParent<UIWindow>();
            }

            return m_WindowParent;
        }
    }

    protected override void Awake ()
    {
        base.Awake();
    }

    protected override void LevelStart ()
    {
        if (!WindowParent.Pages.Contains(this))
        {
            WindowParent.Pages.Add(this);
        }

        base.LevelStart();
    }

    protected override void DoShow ()
    {
        if (IsVisible)
        {
            return;
        }

        base.DoShow();
        WindowParent.NotifyChildShown(this);
    }
}
