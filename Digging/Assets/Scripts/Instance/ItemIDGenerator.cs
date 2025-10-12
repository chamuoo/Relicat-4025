using System;
using System.Collections.Generic;

// 아이템 고유 ID 생성
public static class ItemIDGenerator
{
    private static Dictionary<ItemTypes, int> sequenceByType = new();

    public static long Generate(ItemTypes type)
    {
        if(type == ItemTypes.Null)
        {
            return 0;
        }

        long timetamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        int categoryCode = (int)type;

        if(!sequenceByType.ContainsKey(type))
            sequenceByType[type] = 0;

        sequenceByType[type]++;
        int currentSequence = sequenceByType[type];

        long uiqueId = (timetamp * 10000) + (categoryCode * 100) + currentSequence;
        return uiqueId;
    }
}
