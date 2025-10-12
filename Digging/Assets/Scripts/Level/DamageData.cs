public enum ObstacleType
{
    Bomb,
    BombBlock,
    Monster,
    Fall_High,
    Fall_Mid,
    Fall_Low,
    Poison,
    Sand
}

public class DamageData
{
    public ObstacleType Type { get; private set; }
    public int Damage { get; private set; }

    public DamageData(ObstacleType type, int damage)
    {
        Type = type;
        Damage = damage;
    }
}
