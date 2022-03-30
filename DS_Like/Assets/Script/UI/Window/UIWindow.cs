using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class UIWindow : MonoBehaviour
{
    [System.Serializable]
    public class UIWIndowActionEvent : UnityEngine.Events.UnityEvent
    { }

    public event Action OnHide;
    public event Action OnShow;

    [Header("Window Behavior")]
    [SerializeField] private string m_WindowName = "";
    [SerializeField] private bool m_ResetPositionOnStart = true;
    [SerializeField] private bool m_HideOnStart = true;
    [SerializeField] private bool m_BlockUIInput = false;
    [SerializeField] private bool m_BlockPlayerInput = false;

    [Header("Action")]
    public UIWIndowActionEvent m_OnShowActions = new UIWIndowActionEvent();
    public UIWIndowActionEvent m_OnHideActions = new UIWIndowActionEvent();

    /// <summary>
    /// The window is visible or not? Used for toggling.
    /// </summary>
    public bool IsVisible { get; protected set; }

    public virtual UIWindowPage DefaultPage
    {
        get { return Pages.FirstOrDefault(o => o.IsDefaultPage); }
    }

    private List<UIWindowPage> m_Pages = new List<UIWindowPage>();
    public List<UIWindowPage> Pages
    {
        get { return m_Pages; }
    }

    public UIWindowPage CurrentPage { get; set; }

    private RectTransform m_RectTransform;
    protected RectTransform RectTransform
    {
        get
        {
            if (m_RectTransform == null)
            {
                m_RectTransform = GetComponent<RectTransform>();
            }
            return m_RectTransform;
        }
    }

    protected virtual void Awake()
    {
        if (m_ResetPositionOnStart)
        {
            RectTransform.anchoredPosition = Vector2.zero;
        }

        UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneLoaded;
    }

    private void SceneLoaded(UnityEngine.SceneManagement.Scene i_Scene, UnityEngine.SceneManagement.LoadSceneMode i_Mode)
    {
        LevelStart();
    }

    protected virtual void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= SceneLoaded;
    }

    protected virtual void Start()
    {
        LevelStart();
    }

    protected virtual void LevelStart()
    {
        if (m_HideOnStart)
        {
            HideFirst();
        }
        else
        {
            IsVisible = true;
        }
    }

    public void NotifyChildShown(UIWindowPage i_Page)
    {
        CurrentPage = i_Page;
        HideAllPagesExceptCurrent();

        Show();
    }

    protected void HideAllPagesExceptCurrent(float i_WaitTime = 0.0f)
    {
        UIWindowPage cur = CurrentPage;
        for (int i = 0; i < Pages.Count; i++)
        {
            if (Pages[i] == cur)
            {
                continue;
            }

            Pages[i].Hide(i_WaitTime);
        }
    }

    protected void NotifyWindowHidden()
    {
        m_OnHideActions.Invoke();
        if (OnHide != null)
        {
            OnHide();
        }
    }

    protected void NotifyWindowShown()
    {
        m_OnShowActions.Invoke();
        if (OnShow != null)
        {
            OnShow();
        }
    }

    protected virtual void SetChildrenActive(bool i_Active)
    {
        foreach (Transform t in transform)
        {
            t.gameObject.SetActive(i_Active);
        }

        UnityEngine.UI.Graphic[] img = gameObject.GetComponents<UnityEngine.UI.Graphic>();
        foreach (UnityEngine.UI.Graphic graphic in img)
        {
            graphic.enabled = i_Active;
        }
    }

    public void Toggle()
    {
        if (IsVisible)
        {
            Hide();
        }
        else
        {
            Show();
        }
    }

    public void Show()
    {
        DoShow();
    }

    public void Show(float i_WaitTime)
    {
        if (i_WaitTime > 0f)
        {
            StartCoroutine(_Show(i_WaitTime));
        }
        else
        {
            DoShow();
        }
    }

    protected IEnumerator _Show(float i_WaitTime)
    {
        if (IsVisible)
        {
            yield break;
        }

        if (DefaultPage != null)
        {
            DefaultPage.Show();
        }

        yield return StartCoroutine(CoroutineUtility.WaitRealTime(i_WaitTime));

        DoShow();
    }

    protected virtual void DoShow()
    {
        if (IsVisible)
        {
            return;
        }

        IsVisible = true;
        SetChildrenActive(true);
        CurrentPage = DefaultPage;
        if (CurrentPage != null)
        {
            CurrentPage.Show();
        }

        NotifyWindowShown();
    }

    public virtual void HideFirst()
    {
        IsVisible = false;

        SetChildrenActive(false);
    }

    public void Hide()
    {
        DoHide();
    }

    public void Hide(float i_WaitTime)
    {
        if (i_WaitTime > 0f)
        {
            StartCoroutine(_Hide(i_WaitTime));
        }
        else
        {
            DoHide();
        }
    }

    protected IEnumerator _Hide(float i_WaitTime)
    {
        if (!IsVisible)
        {
            yield break;
        }

        yield return StartCoroutine(CoroutineUtility.WaitRealTime(i_WaitTime));

        DoHide();
    }

    protected virtual void DoHide()
    {
        if (!IsVisible)
        {
            return;
        }

        IsVisible = false;

        if (!IsVisible)
        {
            SetChildrenActive(false);
        }

        NotifyWindowHidden();
    }
}
