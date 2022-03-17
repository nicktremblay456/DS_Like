using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PotionSlot : MonoBehaviour
{
    [SerializeField] private HealthBars m_HealthBars;
    [SerializeField] private TextMeshProUGUI m_PotionAmountText;

    private PlayerController m_Player;

    private Potion m_Potion;
    private int m_MaxPotionAmount = 3;
    private int m_CurrentPotionAmount;
    private int m_ResetPotionAmount;

    private void Awake()
    {
        m_Potion = new Potion(50);
        m_CurrentPotionAmount = m_MaxPotionAmount;
        m_ResetPotionAmount = m_MaxPotionAmount;
        m_PotionAmountText.text = m_CurrentPotionAmount.ToString();
    }

    private void Start()
    {
        m_Player = FindObjectOfType<PlayerController>();
    }

    public void DrinkPotion()
    {
        if (m_CurrentPotionAmount <= 0) return;

        m_HealthBars.Health.RegenHealth(m_Potion.RegenAmount);
        m_CurrentPotionAmount--;
        m_PotionAmountText.text = m_CurrentPotionAmount.ToString();
    }

    public void IncreasePotionRegenValue(int increasedValue)
    {
        m_Potion.IncreasePotionRegenValue(increasedValue);
    }

    public void AddPotion()
    {
        m_MaxPotionAmount++;
        m_ResetPotionAmount = m_MaxPotionAmount;
    }

    public void ResetPotionAmount()
    {
        m_CurrentPotionAmount = m_ResetPotionAmount;
    }
}