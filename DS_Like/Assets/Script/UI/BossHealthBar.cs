using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthBar : MonoBehaviour
{
    private static BossHealthBar m_Instance;
    public static BossHealthBar Instance { get => m_Instance; }

    [SerializeField] private TextMeshProUGUI m_BossNameText;
    [SerializeField] private Image m_HealthImage;

    private Health m_Health;
    public Health Health { get => m_Health; }

    private void Awake()
    {
        if (m_Instance == null) m_Instance = this;
        gameObject.SetActive(false);
    }

    public void SetBossHealth(string name, int maxHealth)
    {
        gameObject.SetActive(true);
        m_BossNameText.text = name;
        m_Health = new Health(maxHealth, 0);
        UpdateHealthInfo();// Call UpdateHealthInfo() to reset the boss health bar
        m_Health.OnHealthChanged += Health_OnHealthChanged;
    }

    private void Health_OnHealthChanged(object sender, EventArgs e)
    {
        UpdateHealthInfo();
    }

    private void UpdateHealthInfo()
    {
        float health = (100f / m_Health.MaxHealth) * m_Health.CurrentHealth;

        m_HealthImage.fillAmount = health / 100;
    }
}