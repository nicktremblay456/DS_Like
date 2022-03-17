public class Potion
{
    private int m_RegenAmount;
    public int RegenAmount { get => m_RegenAmount; }

    public Potion(int startingRegenAmount)
    {
        m_RegenAmount = startingRegenAmount;
    }

    public void IncreasePotionRegenValue(int increasedValue)
    {
        m_RegenAmount += increasedValue;
    }
}