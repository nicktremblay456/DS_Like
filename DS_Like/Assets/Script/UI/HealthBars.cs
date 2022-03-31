using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBars : MonoBehaviour
{
    private static HealthBars m_Instance;
    public static HealthBars Instance { get => m_Instance; }

    private Health m_Health;
    private Level m_Level;

    [SerializeField] private Image m_HpBar;
    [SerializeField] private Image m_SpBar;
    [SerializeField] private Image m_ExpBar;

    private Coroutine m_StaminaRegenRoutine;
    private WaitForSeconds m_RegenTick = new WaitForSeconds(0.1f);

    public Health Health { get => m_Health; }
    public Level Level { get => m_Level; }

    private void Awake()
    {
        if (m_Instance == null) m_Instance = this;

        m_Health = new Health(100, 100);
        m_Health.OnHealthChanged += Health_OnHealthChanged;

        m_Level = new Level();
        m_Level.OnLevelChanged += Level_OnLevelChanged;
        m_Level.OnExpChanged += Level_OnExpChanged;
    }

    private void Health_OnHealthChanged(object sender, EventArgs e)
    {
        UpdateHealthInfo();
    }

    private void UpdateHealthInfo()
    {
        float health = (100f / m_Health.MaxHealth) * m_Health.CurrentHealth;
        float stamina = (100f / m_Health.MaxStamina) * m_Health.CurrentStamina;

        m_HpBar.fillAmount = health / 100;
        m_SpBar.fillAmount = stamina / 100;
    }

    private IEnumerator RegenStamina()
    {
        yield return new WaitForSeconds(3.5f);
        while (m_Health.CurrentStamina < m_Health.MaxStamina)
        {
            m_Health.RegenStamina(m_Health.MaxStamina / 100);
            yield return m_RegenTick;
        }
        m_StaminaRegenRoutine = null;
    }

    public void TakeDamage(int a_Damage)
    {
        m_Health.TakeDamage(a_Damage);
    }

    public void UseStamina(int a_Stamina)
    {
        m_Health.UseStamina(a_Stamina);
        if (m_StaminaRegenRoutine != null) StopCoroutine(m_StaminaRegenRoutine);
        m_StaminaRegenRoutine = StartCoroutine(RegenStamina());
    }

    public void UpdateMaxHealth(int a_MaxHealth, int a_MaxStamina)
    {
        if (m_Health != null) m_Health.UpdateMaxHealthStats(a_MaxHealth, a_MaxStamina);
        else m_Health = new Health(a_MaxHealth, a_MaxStamina);
    }

    #region Level/Experience Methods

    private void Level_OnLevelChanged(object sender, EventArgs e)
    {
        m_ExpBar.rectTransform.sizeDelta += new Vector2(10, 0);
    }

    private void Level_OnExpChanged(object sender, EventArgs e)
    {
        m_ExpBar.fillAmount = m_Level.GetExpNormalized;
    }
    #endregion
}