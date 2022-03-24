using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class MessageText : MonoBehaviour
{
    private static MessageText m_Instance;
    public static MessageText Instance { get => m_Instance; }
    private TextMeshProUGUI m_Text;

    private void Awake()
    {
        if (m_Instance == null) m_Instance = this;
        m_Text = GetComponent<TextMeshProUGUI>();
        gameObject.SetActive(false);
    }

    public void SetText(string message)
    {
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        m_Text.text = message;
    }

    public void HideMessage()
    {
        m_Text.text = string.Empty;
        gameObject.SetActive(false);
    }
}