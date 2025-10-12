using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Repository : Singleton<Repository>
{
    [SerializeField] List<WeaponTemplate> weapons;
    [SerializeField] List<Item> items;

    readonly Dictionary<ItemTypes, WeaponTemplate> weaponDict = new();
    readonly Dictionary<ItemTypes, Item> itemDict= new();

    private void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        weapons.AddRange(Resources.LoadAll<WeaponTemplate>("Item/Weapon"));

        for(int i = 1; i <= 4; i++)
        {
            string path = $"Item/other/Item_Use0{i}";
            Item item = Resources.Load<Item>(path);

            items.Add(item);
        }

        foreach(WeaponTemplate weapon in weapons)
        {
            weaponDict[weapon.type] = weapon;
        }

        foreach(Item item in items)
        {
            itemDict[item.type] = item;
        }
    }

    public ScriptableObject GetTemplate(ItemTypes type)
    {
        int typeValue = (int)type;

        if((typeValue & (int)ItemCategory.Weapon) == (int)ItemCategory.Weapon)
        {
            if(weaponDict.TryGetValue(type, out WeaponTemplate weapon))
                return weapon;
        }
        else if((typeValue & (int)ItemCategory.Item) == (int)ItemCategory.Item)
        {
            if(itemDict.TryGetValue(type, out Item item))
                return item;
        }

        return null;
    }
}