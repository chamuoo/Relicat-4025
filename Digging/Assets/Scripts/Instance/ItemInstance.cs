using UnityEngine;

[System.Serializable]
public class ItemInstance
{
    #region Field
    public Item _item;
    public int _count;
    public Sprite itemImage;

    public ItemInstance(Item item)
    {
        _item = item;
        itemImage = item.itemImage;
        _count = item.count;
    }

    public Sprite GetSprite()
    {
        if(_item != null)
            return _item.itemImage;

        return null;
    }

    public void AddItemCount(int num)
    {
        _count += num;
    }

    #endregion Field

}

[System.Serializable]
public class WeaponInstance
{
    #region Field
    public WeaponTemplate _template;

    public long _id;
    public float _damage;
    public int _level;
    public float _energy; // 드릴만 필요
    public Vector2 _range;

    public Sprite itemImage;

    #endregion // Field

    #region Method

    public WeaponInstance(WeaponTemplate template)
    {
        _template = template;
        _id = ItemIDGenerator.Generate(template.type);

        _level = 1;
        _damage = template.damage;
        _energy = template.type == ItemTypes.Drill ? 100 : -1;
        _range = new Vector2(1, 1);

        itemImage = template.icon;
    }

    // [무기] 
    public Sprite GetSprite()
    {
        if(_template != null)
        {
            return _template.GetSpriteForLevel(_level);
        }

        return null;
    }

    public void AddDamge(float add)
    {
        _damage += add;
    }
}

#endregion // Method
