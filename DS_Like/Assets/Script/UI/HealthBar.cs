using System;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private Health m_Health;

    private void Awake()
    {
    }

    public void SetUp(Health a_Health)
    {
        m_Health = a_Health;
        m_Health.OnHealthChanged += Health_OnHealthChanged;
    }

    private void Health_OnHealthChanged(object sender, EventArgs e)
    {
        
    }
}