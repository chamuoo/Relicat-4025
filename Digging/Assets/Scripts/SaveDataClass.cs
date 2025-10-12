using JetBrains.Annotations;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

[System.Serializable]
public class ItemData
{
    public string itemName;
    public int count = 0;
    public int accumulation_count;
    public bool isMineral;
    public bool isRelic;
    public bool isDrillItem;
    public int value;
    public bool ishaveitem;
    public bool isalreadySell;
}

[System.Serializable]
public class InventoryData
{
    public List<ItemData> items;
    public ItemData money_item;
}

[System.Serializable]
public class CollectionData
{
    public int collect_sum;
    public int player_lv;
    public bool[] li_isCollect;
    public bool[] li_isRelicOnTable;
}

[System.Serializable]
public class ShopData
{
    public float pick_damage;
    public float lightRadius;
    public bool isCreateDrill;
}


[System.Serializable]
public class PlayerData
{
    public List<ItemData> items;
    public List<ItemData> minerals;
    public List<ItemData> UseItems;
    public List<ItemData> UpgradeItems;
    public List<ItemData> Drill_Items;
    public float HP;
}

[System.Serializable]
public class PlayerControllerData
{
    public float pickDamage = 6f;
}


[System.Serializable]
public class WeaponsData
{
    public List<WeaponsData> ownedWeapons;
}

[System.Serializable]
public class LoadSceneData
{
    public bool isAlreadyWatchStory = false;
    public int stage_Level = 0;
    public int difficulty_level = 0;
}

[System.Serializable]
public class LevelManageData
{
    public float remainingTime;
}


[System.Serializable]
public class BlockData
{
    public Vector3 blockPosition;
    public int nowBlockType;
    public int blockType;
    public int stageNum;
    public float blockHealth;
}

[System.Serializable]
public class BlocksData
{
    public List<BlockData> blockDatas;
}


[System.Serializable]
public class EnemyData
{
    public List<Vector2> EnemyDatas;
}

[System.Serializable]
public class SlotInfoData   // 슬롯의 데이터들
{
    public int index;
    public SlotType type;
    public SlotData data;

    public SlotInfoData(int index, SlotType type, SlotData data)
    {
        this.index = index;
        this.type = type;
        this.data = data;
    }
}

[System.Serializable]
public class QuickSlotSaveData
{
    public List<SlotInfoData> slots = new List<SlotInfoData>();
    public SlotInfoData currentWeapon;  // 장착된 무기 데이터
}

[System.Serializable]
public class MinimapPixelData
{
    public int x;
    public int y;
    public Color color;
}

[System.Serializable]
public class MinimapPixelsData
{
    public List<MinimapPixelData> minimapPixelsData;
}

[System.Serializable]
public class torchPositionData
{
    public List<Vector2> torchPositionDatas;
}

[System.Serializable]
public class SaveData
{
    public InventoryData inventoryData;
    public CollectionData collectionData;
    public ShopData shopData;
    public PlayerData playerData;
    public WeaponsData weaponData;
    public LoadSceneData loadSceneData;
    public LevelManageData levelManageData;
    public BlocksData blocksData;
    public EnemyData enemyData;
    public QuickSlotSaveData quickSlotInfoData;
    public MinimapPixelsData minimapPixelsData;
    public torchPositionData torchPositionData;
}