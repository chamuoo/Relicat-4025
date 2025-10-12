using System.Collections.Generic;

public class DamageManager
{
    private readonly Dictionary<int, Dictionary<ObstacleType, int>> damageMap;

    public DamageManager()
    {
        damageMap = new()
        {
            [0] = new() // 쉬움
            {
                [ObstacleType.Bomb] = 10,
                [ObstacleType.BombBlock] = 10,
                [ObstacleType.Fall_High] = 8,
                [ObstacleType.Fall_Mid] = 6,
                [ObstacleType.Fall_Low] = 4,
                [ObstacleType.Poison] = 4,
                [ObstacleType.Sand] = 4
            },
            [1] = new() // 보통
            {
                [ObstacleType.Bomb] = 25,
                [ObstacleType.BombBlock] = 25,
                [ObstacleType.Fall_High] = 15,
                [ObstacleType.Fall_Mid] = 10,
                [ObstacleType.Fall_Low] = 5,
                [ObstacleType.Poison] = 5,
                [ObstacleType.Sand] = 5,
                [ObstacleType.Monster] = 20
            },
            [2] = new() // 어려움
            {
                [ObstacleType.Bomb] = 35,
                [ObstacleType.BombBlock] = 35,
                [ObstacleType.Fall_High] = 25,
                [ObstacleType.Fall_Mid] = 15,
                [ObstacleType.Fall_Low] = 10,
                [ObstacleType.Poison] = 10,
                [ObstacleType.Sand] = 10,
                [ObstacleType.Monster] = 30
            }
        };
    }

    public int GetDamage(int difficulty, ObstacleType type) =>
        damageMap.TryGetValue(difficulty, out var table) && table.TryGetValue(type, out var dmg) ? dmg : 0;
}

