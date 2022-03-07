using System;

public class Health
{
    public event EventHandler OnHealthChanged;
    private int m_MaxHealth;
    private int m_CurrentHealth;

    public int CurrentHealth { get => m_CurrentHealth; }
    private int MaxHealth { get => m_MaxHealth; }
    public float GetHealthPercent { get => m_CurrentHealth / MaxHealth; }

    public Health(int a_MaxHealth)
    {
        m_MaxHealth = a_MaxHealth;
        m_CurrentHealth = m_MaxHealth;
    }

    public void TakeDamage(int a_DamageAmount)
    {
        m_CurrentHealth -= a_DamageAmount;
        if (m_CurrentHealth < 0) m_CurrentHealth = 0;
        if (OnHealthChanged != null) OnHealthChanged(this, EventArgs.Empty);
    }

    public void Heal(int a_HealthAmount)
    {
        m_CurrentHealth += a_HealthAmount;
        if (m_CurrentHealth > m_MaxHealth) m_CurrentHealth = m_MaxHealth;
        if (OnHealthChanged != null) OnHealthChanged(this, EventArgs.Empty);
    }
}