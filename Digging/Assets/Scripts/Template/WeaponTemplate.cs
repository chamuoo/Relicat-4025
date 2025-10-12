using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

[System.Serializable]
public class LevelSprite
{
    public int level;
    public Sprite sprite;
}

[CreateAssetMenu(fileName = "Template", menuName = "Inventory/Weapon Template")]
public class WeaponTemplate : ScriptableObject
{
    #region Field
    [Header("공통 정보")]
    public ItemTypes type;
    public string weaponName;
    public string explanation;
    public Sprite icon;
    public List<LevelSprite> levelSprite;

    [Header("스탯(RunTime)")]
    public float damage;
    public int level;
    public float range;
    public float energy;    // 드릴만 필요
    #endregion // Field

    #region Method

    // 스프라이트 이미지
    public Sprite GetSpriteForLevel(int level)
    {
        Sprite result = icon;

        // 높은 레벨부터 순회하여 조건을 만족하는 첫 스프라이트를 찾음
        foreach(var entry in levelSprite.OrderByDescending(entry => entry.level))
        {
            if(level >= entry.level)
            {
                result = entry.sprite;
                Debug.Log($"이미지: {result.name}, 현재 레벨: {level}, 기준 레벨: {entry.level}");
                break;
            }
        }


        return result;
    }

    #endregion // Method

}
