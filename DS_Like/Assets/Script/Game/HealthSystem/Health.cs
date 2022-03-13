using System;

public class Health
{
    public event EventHandler OnHealthChanged;

    private int m_MaxHealth;
    private int m_CurrentHealth;
    private int m_MaxStamina;
    private int m_CurrentStamina;

    public int MaxHealth { get => m_MaxHealth; }
    public int CurrentHealth { get => m_CurrentHealth; }
    public float GetHealthPercent { get => m_CurrentHealth / MaxHealth; }
    public int MaxStamina { get => m_MaxStamina; }
    public int CurrentStamina { get => m_CurrentStamina; }

    public Health(int a_MaxHealth, int a_MaxStamina)
    {
        m_MaxHealth = a_MaxHealth;
        m_CurrentHealth = m_MaxHealth;
        m_MaxStamina = a_MaxStamina;
        m_CurrentStamina = m_MaxStamina;
    }

    public void TakeDamage(int a_DamageAmount)
    {
        m_CurrentHealth -= a_DamageAmount;
        if (m_CurrentHealth < 0) m_CurrentHealth = 0;
        if (OnHealthChanged != null) OnHealthChanged(this, EventArgs.Empty);
    }

    public void UseStamina(int a_StaminaAount)
    {
        m_CurrentStamina -= a_StaminaAount;
        if (m_CurrentStamina < 0) m_CurrentStamina = 0;
        if (OnHealthChanged != null) OnHealthChanged(this, EventArgs.Empty);
    }

    public void RegenHealth(int a_HealthAmount)
    {
        m_CurrentHealth += a_HealthAmount;
        if (m_CurrentHealth > m_MaxHealth) m_CurrentHealth = m_MaxHealth;
        if (OnHealthChanged != null) OnHealthChanged(this, EventArgs.Empty);
    }

    public void RegenStamina(int a_StaminaAount)
    {
        m_CurrentStamina += a_StaminaAount;
        if (m_CurrentStamina > m_MaxStamina) m_CurrentStamina = m_MaxStamina;
        if (OnHealthChanged != null) OnHealthChanged(this, EventArgs.Empty);
    }

    public void UpdateMaxHealthStats(int a_MaxHealth, int a_MaxStamina)
    {
        m_MaxHealth = a_MaxHealth;
        m_MaxStamina = a_MaxStamina;
        if (m_CurrentHealth > m_MaxHealth) m_CurrentHealth = m_MaxHealth;
        if (m_CurrentStamina > m_MaxStamina) m_CurrentStamina = m_MaxStamina;
        if (OnHealthChanged != null) OnHealthChanged(this, EventArgs.Empty);
    }
}