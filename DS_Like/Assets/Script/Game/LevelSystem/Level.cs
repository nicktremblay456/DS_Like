using UnityEngine;
using System;

public class Level
{
    public event EventHandler OnExpChanged;
    public event EventHandler OnLevelChanged;

    private int m_CurrentLevel;
    private int m_Experience;

    public int CurrentLevel { get => m_CurrentLevel; }
    public int Experience { get => m_Experience; }
    public float GetExpNormalized
    {
        get
        {
            if (IsMaxLevel()) return 1f;
            else return (float)m_Experience / GetExpToNextLevel();
        }
    }

    private readonly int[] m_ExpriencePerLevel = new int[] { 100, 150, 275, 412, 618, 927, 1390, 2085, 3127, 4690 }; // 10 Level

    public Level()
    {
        m_CurrentLevel = 0;
        m_Experience = 0;
    }

    public void AddExp(int amount)
    {
        m_Experience += amount;
        while (m_Experience >= GetExpToNextLevel())
        {
            m_Experience -= GetExpToNextLevel();
            m_CurrentLevel++;
            if (OnLevelChanged != null) OnLevelChanged(this, EventArgs.Empty);
        }
        if (OnExpChanged != null) OnExpChanged(this, EventArgs.Empty);
    }

    public int GetExpToNextLevel()
    {
        if (m_CurrentLevel < m_ExpriencePerLevel.Length) return m_ExpriencePerLevel[m_CurrentLevel];
        else
        {
            Debug.Log($"Level invalid: {m_CurrentLevel}");
            return (m_ExpriencePerLevel[m_CurrentLevel - 1] / 2) + m_ExpriencePerLevel[m_CurrentLevel];
        }
    }

    public bool IsMaxLevel()
    {
        return m_CurrentLevel == m_ExpriencePerLevel.Length - 1;
    }
}