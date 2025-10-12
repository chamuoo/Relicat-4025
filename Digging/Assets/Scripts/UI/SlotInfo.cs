using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;

public enum SlotType
{
    Null,
    QuickSlot,
    Inventory
}

public enum ItemTypes
{
    Null = 0,

    // Weapon (index: 0~)
    Pickaxe = ItemCategory.Weapon | 0,
    Drill = ItemCategory.Weapon | 1,

    // Item (index: 0~)
    Bomb = ItemCategory.Item | 0,
    Lamp = ItemCategory.Item | 1,
    Teleport = ItemCategory.Item | 2,
    
}

[System.Serializable]
public class SlotData
{
    public WeaponInstance weapon;
    public ItemInstance item;

    public object ActiveInstance
    {
        get
        {
            if(weapon != null) return weapon;
            if(item != null) return item;
            return null;
        }
    }
}

public class SlotInfo : MonoBehaviour
{
    public int _index;
    public SlotType _typeS;
    public ItemTypes _type;

    public SlotData slot { get; set; }

    [SerializeField] SlotBackground background;
    [SerializeField] SlotInteraction action;

    public void SetImage(Item item)
    {
        Image icon = action.GetComponent<Image>();

        icon.color = new Color(1, 1, 1, 1);
        icon.sprite = item.itemImage;
    }

    public void SetSlotImage()
    {
        action.Apply(this);
    }

    public void Select()
    {
        background.Highlight();
    }

    public void Deselect()
    {
        background.Unhighlight();
    }

    public void ClearSlot()
    {
        slot = null;
        background.Unhighlight();
        action.Clear();
    }

    public SlotInfoData ToData()
    {
        return new SlotInfoData(_index, _typeS, slot);
    }

}
